using System;
using System.Collections.Generic;
using System.Text;

namespace SharedStuff {
    public static class SensorList {

        public static Sensor[] sensors;

        public static void Initialize()
        {
            sensors = new Sensor[]
            {
                new DebugSensor(),
                new DebugSensor(),
                new DebugSensor(),
                new DebugSensor(),
                //new pHSensor(8, 0, 1), 
                //new TemperatureSensor(8, 1, 1), 
            };

            for (byte i = 0; i < sensors.Length; ++i) {
                sensors[i].SensorId = i;
            }
        }
    }
}
