using System;
using SharedStuff;
using Microsoft.Data.Sqlite;

namespace MainServer {
    class DatabaseHandler {

        SqliteConnection sql;
        public DatabaseHandler() {
            SocketServer.RegisterHandler(MessageType.SensorData, SensorDataHandler);
            SocketServer.RegisterHandler(MessageType.Image, ImageHandler);
            
            sql = new SqliteConnection("Data Source=Db/sensorData.db");
        }

        public void SensorDataHandler(byte[] bytes) {
            SensorData sensorData = new SensorData(bytes);

            using (sql) {
                sql.Open();
                var command = sql.CreateCommand();

                command.CommandText = "INSERT INTO SensorValue (Timestamp, SensorTypeId, Value) VALUES ($time, $type, $value)";

                command.Parameters.AddWithValue("time", sensorData.Timestamp);

                SqliteParameter typeParameter = command.CreateParameter();
                typeParameter.ParameterName = "$type";
                command.Parameters.Add(typeParameter);

                SqliteParameter valueParameter = command.CreateParameter();
                valueParameter.ParameterName = "$value";
                command.Parameters.Add(valueParameter);

                foreach (var sensorValue in sensorData.Sensors) {
                    typeParameter.Value = sensorValue.type;
                    valueParameter.Value = sensorValue.value;
                    command.ExecuteNonQuery();
                }
            }
        }
        public void ImageHandler(byte[] bytes) {
            Console.Out.WriteLine("Image thing");
        }
    }
}
