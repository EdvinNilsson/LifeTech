using System;
using System.Collections.Generic;
using System.Text;

namespace SharedStuff {
    public static class SensorList {

        public static Sensor[] sensors;
        public static string[] Units = { "m³/s", "%", "lux", "%", "Pa", "°C", "ppm", "ppb", "°C", "", "" };

        public static void Initialize()
        {
            sensors = new Sensor[]
            {
                new DebugSensor() {SensorName = "Felsökningssensor 1"},
                new DebugSensor() {SensorName = "Felsökningssensor 2"},
                new DebugSensor() {SensorName = "Felsökningssensor 3"},
                new DebugSensor() {SensorName = "Felsökningssensor 4"},
                new pHSensor(8, 0, 1) {SensorName = "pH Sensor"}, 
                new TemperatureSensor(8, 1, 1) {SensorName = "Temperatursensor"},
            };

            for (byte i = 0; i < sensors.Length; ++i) {
                sensors[i].SensorId = i;
            }
        }
    }
}
