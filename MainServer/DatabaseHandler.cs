using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using SharedStuff;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;

namespace MainServer {
    static class DatabaseHandler {

        static SqliteConnection sql;
        public static event Action<SensorData> OnReceivedSensorData;

        public static void Initialize() {
            SocketServer.RegisterHandler(MessageType.SensorData, SensorDataHandler);
            SocketServer.RegisterHandler(MessageType.Image1, Image1Handler);
            SocketServer.RegisterHandler(MessageType.Image2, Image2Handler);

            sql = new SqliteConnection("Data Source=Db/sensorData.db");
            Directory.CreateDirectory("Images 1");
            Directory.CreateDirectory("Images 2");

            SensorList.Initialize();

            byte[] sensorIds = SensorList.Sensors.Select(c => c.SensorId).ToArray();
            CreateSensorTables(sensorIds);
        }

        public static void FillAllCacheArrays() {
            byte[] sensorIds = SensorList.Sensors.Select(c => c.SensorId).ToArray();

            foreach (DataPeriod period in Enum.GetValues(typeof(DataPeriod))) {
                FillCacheArray(sensorIds, period);
            }
        }

        static bool dbOpen;

        static void ConnectToDb() {
            while (dbOpen) {
                Thread.Sleep(1);
            }
            dbOpen = true;
            sql.Open();
        }

        static void CloseDb() {
            sql.Close();
            dbOpen = false;
        }

        static Dictionary<byte, Dictionary<byte, List<float>>>[] databaseCache = new Dictionary<byte, Dictionary<byte, List<float>>>[5];

        static int[] firstTimestampCache = new int[5];

        static void AddToCacheArray(SensorData sensorData, int count, DataPeriod dataPeriod, int skipEvery) {
            if (firstTimestampCache[(byte) dataPeriod] == 0) return;

            if (dataPeriod != DataPeriod.AllData) { 
                int removeCount = (sensorData.Timestamp - firstTimestampCache[(byte) dataPeriod] - count * skipEvery) / skipEvery;
                firstTimestampCache[(byte) dataPeriod] += removeCount * skipEvery;

                foreach (var sensor in databaseCache[(byte) dataPeriod].Values) {
                    foreach (var sensorValue in sensor) {
                        if (sensorValue.Value.Count > removeCount) {
                            sensorValue.Value.RemoveRange(0, removeCount);
                        } else {
                            sensor.Remove(sensorValue.Key);
                        }
                    }
                }
            }

            foreach (var sensor in sensorData.Sensors) {
                var list = databaseCache[(byte) dataPeriod].GetValueCreateNew(sensor.id).GetValueCreateNew((byte) sensor.type);
                if (dataPeriod == DataPeriod.AllData) list.Add(sensor.value);
                else list.IndexCreateNew(count - 1, sensor.value);
            }
        }

        public static void SensorDataHandler(byte[] bytes) {
            SensorData sensorData = new SensorData(bytes);
            OnReceivedSensorData?.Invoke(sensorData);
            OnReceivedSensorData = null;

            AddToCacheArray(sensorData, 60, DataPeriod.LastMinute, 1);
            if (sensorData.Timestamp % 60 == 0) AddToCacheArray(sensorData, 60, DataPeriod.LastHour, 60);
            if (sensorData.Timestamp % 300 == 0) AddToCacheArray(sensorData, 288, DataPeriod.LastDay, 300);
            if (sensorData.Timestamp % 1800 == 0) AddToCacheArray(sensorData, 336, DataPeriod.LastWeek, 1800);
            if (sensorData.Timestamp % 3600 == 0) AddToCacheArray(sensorData, int.MaxValue, DataPeriod.AllData, 3600);

            try {
                ConnectToDb();
                using (sql) {
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
                            lastId = sensorValue.id;
                        }

                        typeParameter.Value = sensorValue.type;
                        valueParameter.Value = sensorValue.value;
                        command.ExecuteNonQuery();
                    }
                }
            } finally {
                CloseDb();
            }
        }

        public static void Image1Handler(byte[] bytes) {
            File.WriteAllBytes("Images 1/" + DateTime.Now.ToString("MM-dd-yyyy-HH.mm.ss") + ".jpg", bytes);
        }

        public static void Image2Handler(byte[] bytes) {
            File.WriteAllBytes("Images 2/" + DateTime.Now.ToString("MM-dd-yyyy-HH.mm.ss") + ".jpg", bytes);
        }

        public static FileInfo GetLatestImageInfo(byte imageId) {
            var dir = new DirectoryInfo($"Images {imageId}");
            return dir.GetFiles().OrderByDescending(c => c.CreationTime).First();
        }

        static void CreateSensorTables(byte[] sensorIds) {
            try { 
                ConnectToDb();
                using (sql) {
                    var command = sql.CreateCommand();
                    foreach (int id in sensorIds) {
                        command.CommandText = $"CREATE TABLE S{id} (Timestamp INTEGER, SensorTypeId INTEGER NOT NULL, Value REAL NOT NULL)";
                        try { command.ExecuteNonQuery(); } catch (SqliteException) { }
                    }
                }
            } finally {
                CloseDb();
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

        public static void FillCacheArray(byte[] sensorIds, DataPeriod dataPeriod) {
            try {
                ConnectToDb();
                using (sql) {
                    int firstTimestamp = int.MaxValue, skipEvery = 1;

                    switch (dataPeriod) {
                        case DataPeriod.LastMinute:
                            firstTimestamp = (int)((DateTimeOffset)(DateTime.Now - TimeSpan.FromMinutes(1)))
                                .ToUnixTimeSeconds();
                            break;
                        case DataPeriod.LastHour:
                            skipEvery = 60;
                            firstTimestamp =
                                (int)((DateTimeOffset)(DateTime.Now - TimeSpan.FromHours(1))
                                    .Floor(TimeSpan.FromMinutes(1))).ToUnixTimeSeconds();
                            break;
                        case DataPeriod.LastDay:
                            skipEvery = 300;
                            firstTimestamp =
                                (int)((DateTimeOffset)(DateTime.Now - TimeSpan.FromDays(1))
                                    .Floor(TimeSpan.FromMinutes(5))).ToUnixTimeSeconds();
                            break;
                        case DataPeriod.LastWeek:
                            skipEvery = 1800;
                            firstTimestamp =
                                (int)((DateTimeOffset)(DateTime.Now - TimeSpan.FromDays(7))
                                    .Floor(TimeSpan.FromMinutes(30))).ToUnixTimeSeconds();
                            break;
                        case DataPeriod.AllData:
                            skipEvery = 3600;
                            var command = sql.CreateCommand();

                            foreach (var sensorId in sensorIds) {
                                command.CommandText = $"SELECT Timestamp FROM S{sensorId} LIMIT 1";
                                using var reader = command.ExecuteReader();
                                while (reader.Read()) {
                                    int timestamp = reader.GetInt32(0);
                                    if (timestamp < firstTimestamp) firstTimestamp = timestamp;
                                }
                            }

                            firstTimestamp = (int)((DateTimeOffset)SensorData.UnixTimeToDateTime(firstTimestamp)
                                .Round(TimeSpan.FromHours(1))).ToUnixTimeSeconds();
                            break;
                    }

                    databaseCache[(byte) dataPeriod] = new Dictionary<byte, Dictionary<byte, List<float>>>();

                    for (byte i = 0; i < sensorIds.Length; ++i) {
                        var command = sql.CreateCommand();
                        command.CommandText = $"SELECT * FROM S{sensorIds[i]}";

                        switch (dataPeriod) {
                            case DataPeriod.LastMinute:
                                command.CommandText += " WHERE Timestamp >= $startTime";
                                break;
                            case DataPeriod.LastHour:
                            case DataPeriod.LastDay:
                            case DataPeriod.LastWeek:
                                command.CommandText +=
                                    $" WHERE Timestamp >= $startTime AND Timestamp % {skipEvery} = 0";
                                break;
                            case DataPeriod.AllData:
                                command.CommandText += " WHERE Timestamp % 3600 = 0";
                                break;
                        }

                        if (dataPeriod != DataPeriod.AllData)
                            command.Parameters.AddWithValue("$startTime", firstTimestamp);

                        try { databaseCache[(byte)dataPeriod].Add(sensorIds[i], new Dictionary<byte, List<float>>()); } catch { }

                        Dictionary<byte, int> times = new Dictionary<byte, int>();
                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                int t = reader.GetInt32(0);
                                byte sensorType = reader.GetByte(1);
                                if (!times.ContainsKey(sensorType)) {
                                    times[sensorType] = firstTimestamp;
                                }
                                times[sensorType] += skipEvery;
                                for (; times[sensorType] < t; times[sensorType] += skipEvery) {
                                    databaseCache[(byte)dataPeriod][sensorIds[i]].GetValueCreateNew(sensorType).Add(0);
                                }
                                databaseCache[(byte)dataPeriod][sensorIds[i]].GetValueCreateNew(sensorType).Add(reader.GetFloat(2));
                            }
                        }
                    }
                    firstTimestampCache[(byte)dataPeriod] = firstTimestamp;
                }
            } finally {
                CloseDb();
            }
        }

        public static byte[] GetSensorData(byte[] sensorIds, DataPeriod dataPeriod) {
            var values = databaseCache[(byte)dataPeriod];

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            StringBuilder strb = new StringBuilder($"{firstTimestampCache[(byte)dataPeriod]}|{{");
            int j = 0;
            foreach (var sensorId in sensorIds) {
                var sensor = values[sensorId];
                strb.Append($"\"{sensorId}\":{{");
                int i = 0;
                foreach (var sensorValue in sensor) {
                    strb.Append($"\"{sensorValue.Key}\":[{string.Join(',', sensorValue.Value)}]");
                    if (++i != sensor.Count) strb.Append(',');
                }
                strb.Append('}');
                if (++j != values.Keys.Count) strb.Append(',');
            }
            strb.Append('}');
            return Encoding.UTF8.GetBytes(strb.ToString());
        }
    }
}
