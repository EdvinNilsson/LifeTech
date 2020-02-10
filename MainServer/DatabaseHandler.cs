using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading;
using SharedStuff;
using Microsoft.Data.Sqlite;
using System.IO;

namespace MainServer {
    static class DatabaseHandler {

        static SqliteConnection sql;
        public static event Action<SensorData> OnReceivedSensorData;

        public static void Initialize() {
            SocketServer.RegisterHandler(MessageType.SensorData, SensorDataHandler);
            SocketServer.RegisterHandler(MessageType.Image, ImageHandler);
            
            sql = new SqliteConnection("Data Source=Db/sensorData.db");

            SensorList.Initialize();
            CreateSensorTables(SensorList.sensors.Length);
        }

        static void ConnectToDb() {
            while (sql.State != ConnectionState.Closed) {
                Thread.Sleep(1);
            }
            sql.Open();
        }

        public static void SensorDataHandler(byte[] bytes) {
            SensorData sensorData = new SensorData(bytes);
            OnReceivedSensorData?.Invoke(sensorData);
            OnReceivedSensorData = null;

            using (sql) {
                ConnectToDb();
                var command = sql.CreateCommand();

                command.Parameters.AddWithValue("$time", sensorData.Timestamp);

                SqliteParameter typeParameter = command.CreateParameter();
                typeParameter.ParameterName = "$type";
                command.Parameters.Add(typeParameter);

                SqliteParameter valueParameter = command.CreateParameter();
                valueParameter.ParameterName = "$value";
                command.Parameters.Add(valueParameter);

                byte lastId = byte.MaxValue;
                foreach (var sensorValue in sensorData.Sensors) {
                    if (lastId != sensorValue.id) {
                        command.CommandText = $"INSERT INTO S{sensorValue.id} (Timestamp, SensorTypeId, Value) VALUES ($time, $type, $value)";
                        command.Prepare();
                    }

                    typeParameter.Value = sensorValue.type;
                    valueParameter.Value = sensorValue.value;
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void ImageHandler(byte[] bytes) {
            File.WriteAllBytes("Images/" + DateTime.Now + ".jpg", bytes);
        }

        static void CreateSensorTables(int sensorCount) {
            using (sql) {
                ConnectToDb();
                var command = sql.CreateCommand();

                for (int i = 0; i < sensorCount; ++i) {
                    command.CommandText = $"CREATE TABLE S{i} (Timestamp INTEGER PRIMARY KEY, SensorTypeId INTEGER NOT NULL, Value REAL NOT NULL);";
                    try { command.ExecuteNonQuery(); } catch (SqliteException) { }
                }
            }
        }

        public enum DataPeriod : byte {LastMinute, LastHour, LastDay, LastWeek, AllData}
        
        public static DataPeriod GetDataPeriod(string str) =>
            str switch {
                "timmen" => DataPeriod.LastHour,
                "dygnet" => DataPeriod.LastDay,
                "veckan" => DataPeriod.LastWeek,
                "all" => DataPeriod.AllData,
                _ => DataPeriod.LastMinute
            };
        
        public static byte[] GetSensorData(byte[] sensorIds, DataPeriod dataMode) {
            using (sql) {
                ConnectToDb();

                Dictionary<byte, Dictionary<byte, List<float>>> values = new Dictionary<byte, Dictionary<byte, List<float>>>();
                int firstTimestamp = 0;
                for (byte i = 0; i < sensorIds.Length; ++i) {
                    try {
                        int skipEvery = 1;
                        var command = sql.CreateCommand();
                        command.CommandText = $"SELECT * FROM S{sensorIds[i]}";
                        switch (dataMode) {
                            case DataPeriod.LastMinute:
                                command.CommandText += " WHERE Timestamp >= $startTime";
                                firstTimestamp = (int) ((DateTimeOffset) (DateTime.Now - TimeSpan.FromMinutes(1))).ToUnixTimeSeconds();
                                break;
                            case DataPeriod.LastHour:
                                skipEvery = 60;
                                firstTimestamp = (int) ((DateTimeOffset) (DateTime.Now - TimeSpan.FromHours(1)).Round(TimeSpan.FromMinutes(1))).ToUnixTimeSeconds();
                                break;
                            case DataPeriod.LastDay:
                                skipEvery = 300;
                                firstTimestamp = (int) ((DateTimeOffset) (DateTime.Now - TimeSpan.FromDays(1)).Round(TimeSpan.FromMinutes(5))).ToUnixTimeSeconds();
                                break;
                            case DataPeriod.LastWeek:
                                skipEvery = 1800;
                                firstTimestamp = (int) ((DateTimeOffset) (DateTime.Now - TimeSpan.FromDays(7)).Round(TimeSpan.FromMinutes(30))).ToUnixTimeSeconds();
                                break;
                            case DataPeriod.AllData:
                                command.CommandText += " WHERE Timestamp % 3600 = 0";
                                skipEvery = 3600;
                                break;
                        }
                        switch (dataMode) {
                            case DataPeriod.LastHour:
                            case DataPeriod.LastDay:
                            case DataPeriod.LastWeek:
                                command.CommandText +=
                                    $" WHERE Timestamp >= $startTime AND Timestamp % {skipEvery} = 0";
                                break;
                        }

                        if (firstTimestamp != 0) command.Parameters.AddWithValue("$startTime", firstTimestamp);

                        values.Add(sensorIds[i], new Dictionary<byte, List<float>>());
                        int time = firstTimestamp;
                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                int t = reader.GetInt32(0);
                                if (time == 0) {
                                    time = t - 1;
                                    firstTimestamp = t;
                                }
                                byte sensorType = reader.GetByte(1);
                                time += skipEvery;
                                for (; time < t; time += skipEvery) {
                                    values[sensorIds[i]].GetValueCreateNew(sensorType).Add(0);
                                }
                                values[sensorIds[i]].GetValueCreateNew(sensorType).Add(reader.GetFloat(2));
                            }
                        }
                    } catch (Exception e) {
                        Console.WriteLine(e);
                    }
                }

                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                StringBuilder strb = new StringBuilder($"{firstTimestamp}|{{");
                int j = 0;
                foreach (var sensor in values) {
                    strb.Append($"\"{sensor.Key}\":{{");
                    int i = 0;
                    foreach (var sensorValue in sensor.Value) {
                        strb.Append($"\"{sensorValue.Key}\":[{string.Join(',', sensorValue.Value)}]");
                        if (++i != sensor.Value.Count) strb.Append(',');
                    }
                    strb.Append('}');
                    if (++j != values.Keys.Count) strb.Append(',');
                }
                strb.Append('}');
                return Encoding.UTF8.GetBytes(strb.ToString());
            }
        }
    }
}
