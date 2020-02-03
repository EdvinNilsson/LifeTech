using System;
using System.Collections.Generic;
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

        public static void SensorDataHandler(byte[] bytes) {
            SensorData sensorData = new SensorData(bytes);
            OnReceivedSensorData?.Invoke(sensorData);
            OnReceivedSensorData = null;

            using (sql) {
                sql.Open();
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
            File.WriteAllBytes("Images/" + DateTime.Now.ToString() + ".jpg", bytes);
        }

        static void CreateSensorTables(int sensorCount) {
            using (sql) {
                sql.Open();
                var command = sql.CreateCommand();

                for (int i = 0; i < sensorCount; ++i) {
                    command.CommandText = $"CREATE TABLE S{i} (Timestamp INTEGER PRIMARY KEY, SensorTypeId INTEGER NOT NULL, Value REAL NOT NULL);";
                    try { command.ExecuteNonQuery(); } catch (SqliteException) { }
                }
            }
        }

        public static byte[] GetSensorData(byte[] sensorIds) {
            using (sql) {
                sql.Open();
                var command = sql.CreateCommand();

                Dictionary<byte, Dictionary<byte, List<float>>> values = new Dictionary<byte, Dictionary<byte, List<float>>>();
                int startTime = 0;
                for (byte i = 0; i < sensorIds.Length; ++i) {
                    command.CommandText = $"SELECT * FROM S{sensorIds[i]}";// WHERE Timestamp >= $startTime AND Timestamp <= $endTime AND Timestamp % 2 = 0";
                    values.Add(sensorIds[i], new Dictionary<byte, List<float>>());
                    int time = 0;
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            int t = reader.GetInt32(0);
                            if (time == 0) { time = t - 1; startTime = t; }
                            byte sensorType = reader.GetByte(1);
                            while (t != ++time) {
                                values[sensorIds[i]].GetValueCreateNew(sensorType).Add(0);
                            }
                            values[sensorIds[i]].GetValueCreateNew(sensorType).Add(reader.GetFloat(2));
                        }
                    }
                }

                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                StringBuilder strb = new StringBuilder($"{startTime}|{{");
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
                //return Encoding.UTF8.GetBytes(string.Join(',', values[0]));
                //return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(values);
            }
        }
    }
}
