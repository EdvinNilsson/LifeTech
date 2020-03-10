using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedStuff {
    public static class SensorList {

        public static Sensor[] Sensors;
        public enum SensorValueType : byte { FlowRate, Moisture, Light, Humidity, Pressure, Temperature, CO2, TVOC, pH }
        public static string[] Units = { "m³/s", "%", "lux", "%", "kPa", "°C", "ppm", "ppb", "°C", "" };
        public static string[] Readings = { "vattenflöde", "jordfuktighet", "belysningsstyrka", "luftfuktighet", "lufttryck", "lufttemperatur", "koldioxidhalt", "flyktiga organiska ämnen (TVOC)", "vattensurhet" };

        public static void Initialize()
        {
            Sensors = new Sensor[]
            {
                /*new DebugSensor() {SensorName = "Felsökningssensor 1", SensorId = 81},
                new DebugSensor() {SensorName = "Felsökningssensor 2", SensorId = 82},
                new DebugSensor() {SensorName = "Felsökningssensor 3", SensorId = 83},
                new DebugSensor() {SensorName = "Felsökningssensor 4", SensorId = 84},*/
                /*new MoistureSensor(8,0, 1) {SensorName = "Fuktighetssensor 1", SensorId = 21},
                new MoistureSensor(8,1,1) {SensorName = "Fuktighetssensor 2", SensorId = 22},
                new MoistureSensor(8,2,1) {SensorName = "Fuktighetssensor 3", SensorId = 23},
                new MoistureSensor(8,3,1) {SensorName = "Fuktighetssensor 4", SensorId = 24},
                new MoistureSensor(8,4,1) {SensorName = "Fuktighetssensor 5", SensorId = 25},*/
                new LightSensor(8, 5, 1) {SensorName = "Ljussensor 1", SensorId = 26}, 
                /*new MoistureSensor(7,0,1) {SensorName = "Fuktighetssensor 6"},
                new MoistureSensor(7,1,1) {SensorName = "Fuktighetssensor 7"},
                new MoistureSensor(7,2,1) {SensorName = "Fuktighetssensor 8"},*/
                /*new pHSensor(8, 0, 1) {SensorName = "pH Sensor"}, 
                new TemperatureSensor(8, 1, 1) {SensorName = "Temperatursensor"},*/
                new EnvironmentalSensor(0x77, 0x5b,3) {SensorName = "Miljösensor 1", SensorId = 0},
                new EnvironmentalSensor(0x76, 0x5a,3) {SensorName = "Miljösensor 2", SensorId = 12},
                /*new EnvironmentalSensor(0x77, 0x5b,4) {SensorName = "Miljösensor 3"},
                new EnvironmentalSensor(0x76, 0x5a,4) {SensorName = "Miljösensor 4"},*/
                /*new EnvironmentalDebugSensor() {SensorName = "Felsökningsmiljösensor 1", SensorId = 40},
                new EnvironmentalDebugSensor() {SensorName = "Felsökningsmiljösensor 2",  SensorId = 41},*/
            };

            if (Sensors.Length != Sensors.Select(c => c.SensorId).Distinct().Count()) throw new Exception("Duplicate SensorId");

            /*for (byte i = 0; i < sensors.Length; ++i) {
                sensors[i].SensorId = i;
            }*/
        }
    }
}
