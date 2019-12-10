using System;
using System.Collections.Generic;
using System.Text;

namespace MainServer {
    class DatabaseHandler {
        public static uint addTimestamp() {
            Console.WriteLine("Timestamp created: " + 1);
            return 1;
        }

        public static void addSensorData(float data, uint sensortype_id, uint timestamp_id) {
            Console.WriteLine(data + ", " + sensortype_id + " , " + timestamp_id);
        }
    }
}
