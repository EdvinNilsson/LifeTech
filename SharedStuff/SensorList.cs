using System;
using System.Linq;

namespace SharedStuff {
    public static class SensorList {

        public static Sensor[] Sensors;
        public enum SensorValueType : byte { FlowRate, Moisture, Light, Humidity, Pressure, Temperature, CO2, TVOC, pH }
        public static string[] Units = { "m³/s", "%", "lux", "%", "kPa", "°C", "ppm", "ppb", "°C", "" };
        public static string[] Readings = { "vattenflöde", "jordfuktighet", "belysningsstyrka", "luftfuktighet",
            "lufttryck", "lufttemperatur", "koldioxidhalt", "flyktiga organiska ämnen (TVOC)", "vattensurhet" };

        public static void Initialize() {
            Sensors = new Sensor[]
            {
                /*new MoistureSensor(21, "Fuktighetssensor 1", 8,0, 1),
                new MoistureSensor(22, "Fuktighetssensor 2", 8, 1, 1),
                new MoistureSensor(23, "Fuktighetssensor 3", 8, 2, 1),
                new MoistureSensor(24, "Fuktighetssensor 4", 8, 3, 1),
                new MoistureSensor(25, "Fuktighetssensor 5", 8, 4, 1),
                new MoistureSensor(27, "Fuktighetssensor 6", 7, 0, 1),
                new MoistureSensor(28, "Fuktighetssensor 7", 7, 1, 1),
                new MoistureSensor(29, "Fuktighetssensor 8", 7, 2, 1),
                new MoistureSensor(30, "Fuktighetssensor 9", 7, 3, 1),
                new MoistureSensor(31, "Fuktighetssensor 10", 7, 4, 1),
                new LightSensor(26, "Ljussensor 1", 8, 5, 1),
                new LightSensor(32, "Ljussensor 2",7, 5, 1),
                new TemperatureSensor(50,"Lamptemperatursensor", 8, 6, 1),*/
                new EnvironmentalSensor(0, "Miljösensor 1", 0x77, 0x5b, 3),
                new EnvironmentalSensor(12, "Miljösensor 2", 0x76, 0x5a, 3),
                new EnvironmentalSensor(13, "Miljösensor 3", 0x77, 0x5b, 4),
                new EnvironmentalSensor(14, "Miljösensor 4", 0x76, 0x5a, 4),
                new EnvironmentalSensor(15, "Miljösensor 5", 0x77, 0x5b, 5),
                new EnvironmentalSensor(16, "Miljösensor 6", 0x76, 0x5a, 5),
            };

            if (Sensors.Length != Sensors.Select(c => c.SensorId).Distinct().Count()) throw new Exception("Duplicate SensorId");
        }
    }
}
