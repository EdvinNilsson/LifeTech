using System;
using CCS811_BME280_Library;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using System.Device.I2c;
using static SharedStuff.SensorList;


namespace SharedStuff {

    public class FlowSensor : I2cSensor {
        public FlowSensor(byte sensorId, string sensorName, int deviceAddress, byte cmd, int busId = 1) : base(sensorId, sensorName, deviceAddress, cmd, busId) {
            FlowRate = new SensorValue(this, SensorValueType.FlowRate);
        }

        public SensorValue FlowRate { get; }

        protected override void InternalUpdateValues() {
            FlowRate.value = ReadInt16();
            FlowRate.online = ValidateValue(FlowRate, 0, 100);
        }

        public override SensorValue[] GetSensorValues() => new [] {FlowRate};
    }

    public class MoistureSensor : I2cSensor {
        public MoistureSensor(byte sensorId, string sensorName, int deviceAddress, byte cmd, int busId = 1) : base(sensorId, sensorName, deviceAddress, cmd, busId) {
            Moisture = new SensorValue(this, SensorValueType.Moisture);
        }

        public SensorValue Moisture { get; }

        protected override void InternalUpdateValues() {
            Moisture.value = ReadInt16();//.Map(200, 700, 0, 100);
            Moisture.online = ValidateValue(Moisture, 0, 1000);
        }

        public override SensorValue[] GetSensorValues() => new[] {Moisture};
    }

    public class LightSensor : I2cSensor {
        public LightSensor(byte sensorId, string sensorName, int deviceAddress, byte cmd, int busId = 1) : base(sensorId, sensorName, deviceAddress, cmd, busId) {
            Light = new SensorValue(this, SensorValueType.Light);
        }

        public SensorValue Light { get; }

        protected override void InternalUpdateValues() {
            Light.value = ReadInt16();
            Light.online = ValidateValue(Light, 0, 1000);
        }

        public override SensorValue[] GetSensorValues() => new[] {Light};
    }

    public class pHSensor : I2cSensor {
        public pHSensor(byte sensorId, string sensorName, int deviceAddress, byte cmd, int busId = 1) : base(sensorId, sensorName, deviceAddress, cmd, busId) {
            pH = new SensorValue(this, SensorValueType.pH);
        }

        public SensorValue pH { get; }

        protected override void InternalUpdateValues() {
            pH.value = ReadFloat();
            pH.online = ValidateValue(pH, 1, 14);
        }

        public override SensorValue[] GetSensorValues() => new[] {pH};
    }

    public class TemperatureSensor : I2cSensor {
        public TemperatureSensor(byte sensorId, string sensorName, int deviceAddress, byte cmd, int busId = 1) : base(sensorId, sensorName, deviceAddress, cmd, busId) {
            Temperature = new SensorValue(this, SensorValueType.Temperature);
        }

        public SensorValue Temperature { get; }

        protected override void InternalUpdateValues() {
            Temperature.value = ReadFloat();
            Temperature.online = ValidateValue(Temperature, 0, 100);
        }

        public override SensorValue[] GetSensorValues() => new[] {Temperature};
    }

    public class EnvironmentalSensor : I2cSensor {
        public EnvironmentalSensor(byte sensorId, string sensorName, int bme280DeviceAddress, int ccs811deviceAddress, int busId = 1) : base(sensorId, sensorName, bme280DeviceAddress, 0, busId) {
            Humidity = new SensorValue(this, SensorValueType.Humidity);
            Pressure = new SensorValue(this, SensorValueType.Pressure);
            Temperature = new SensorValue(this, SensorValueType.Temperature);
            CO2 = new SensorValue(this, SensorValueType.CO2);
            TVOC = new SensorValue(this, SensorValueType.TVOC);
            if (Device == null) return;
            try {
                I2cBme280 = new Bme280(Device);
            } catch (Exception e) {
                Console.Write(e);
            }
            try {
                Ccs811Bme280 = new CCS811BME280Sensor(I2cDevice.Create(new I2cConnectionSettings(busId, ccs811deviceAddress)));
                Ccs811Bme280.Initialize();
            } catch (Exception e) {
                Console.Write(e);
            }
        }

        Bme280? I2cBme280;
        CCS811BME280Sensor? Ccs811Bme280;

        public SensorValue Humidity { get; }
        public SensorValue Pressure { get; }
        public SensorValue Temperature { get; }
        public SensorValue CO2 { get; }
        public SensorValue TVOC { get; }

        protected override void InternalUpdateValues() {
            I2cBme280.HumiditySampling = Sampling.Standard;
            I2cBme280.PressureSampling = Sampling.Standard;
            I2cBme280.TemperatureSampling = Sampling.Standard;

            I2cBme280.SetPowerMode(Bmx280PowerMode.Forced);

            if (I2cBme280.TryReadHumidity(out var humidity)) {
                Humidity.value = (float)humidity; Humidity.online = true;
            } else { Humidity.online = false; }

            if (I2cBme280.TryReadPressure(out var pressure)) {
                Pressure.value = (float)pressure.Kilopascal; Pressure.online = true;
            } else { Pressure.online = false; }

            if (I2cBme280.TryReadTemperature(out var temperature)) {
                Temperature.value = (float)temperature.Celsius; Temperature.online = true;
            } else { Temperature.online = false; }

            var ccsData = Ccs811Bme280.ReadCO2TVOC();
            CO2.value = ccsData[0];
            TVOC.value = ccsData[1];

            CO2.online = ValidateValue(CO2, 400, 8192);
            TVOC.online = ValidateValue(TVOC, 0, 1187);
        }

        public override SensorValue[] GetSensorValues() => new[] {Humidity, Pressure, Temperature, CO2, TVOC};
    }

    public class DebugSensor : Sensor {
        public DebugSensor(byte sensorId, string sensorName) : base(sensorId, sensorName) {
            Value = new SensorValue(this, SensorValueType.pH);
        }

        public SensorValue Value { get; }

        protected override void InternalUpdateValues() {
            Random rnd = new Random();
            Value.value = (float)rnd.NextDouble();
            Value.online = rnd.Next(10) != 5;
        }

        public override SensorValue[] GetSensorValues() => new[] {Value};
    }

    public class EnvironmentalDebugSensor : Sensor {
        public EnvironmentalDebugSensor(byte sensorId, string sensorName) : base(sensorId, sensorName) {
            Humidity = new SensorValue(this, SensorValueType.Humidity);
            Pressure = new SensorValue(this, SensorValueType.Pressure);
            Temperature = new SensorValue(this, SensorValueType.Temperature);
            CO2 = new SensorValue(this, SensorValueType.CO2);
            TVOC = new SensorValue(this, SensorValueType.TVOC);
        }

        public SensorValue Humidity { get; }
        public SensorValue Pressure { get; }
        public SensorValue Temperature { get; }
        public SensorValue CO2 { get; }
        public SensorValue TVOC { get; }

        protected override void InternalUpdateValues() {
            Random rnd = new Random();
            Humidity.value = (float)rnd.NextDouble();
            Pressure.value = (float)rnd.NextDouble();
            Temperature.value = (float)rnd.NextDouble();
            CO2.value = rnd.Next(100);
            TVOC.value = rnd.Next(100);

            foreach (SensorValue sensorValue in GetSensorValues()) {
                sensorValue.online = rnd.Next(10) != 5;
            }
        }

        public override SensorValue[] GetSensorValues() => new[] {Humidity, Pressure, Temperature, CO2, TVOC};
    }
}
