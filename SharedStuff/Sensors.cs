using System;
using CCS811_BME280_Library;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Units;
using System.Device.I2c;
using System.IO;
using static SharedStuff.SensorList;


namespace SharedStuff {

    public class FlowSensor : I2cSensor {
        public FlowSensor(int deviceAddress, byte cmd, int busId = 1) : base(deviceAddress, cmd, busId) { }

        public float FlowRate { get; private set; }

        protected override void InternalUpdateValues() {
            FlowRate = ReadInt16();
            ValidateValue(FlowRate, 0, 100);
        }

        public override SensorValue[] GetSensorValues() {
            return new[] {new SensorValue(this, SensorValueType.FlowRate, FlowRate)};
        }
    }

    public class MoistureSensor : I2cSensor {
        public MoistureSensor(int deviceAddress, byte cmd, int busId = 1) : base(deviceAddress, cmd, busId) { }

        public float Moisture { get; private set; }

        protected override void InternalUpdateValues() {
            Moisture = ReadInt16();//100 - .Map(200, 700, 0, 100);
            ValidateValue(Moisture, 0, 1000);
        }
        public override SensorValue[] GetSensorValues() {
            return new[] { new SensorValue(this, SensorValueType.Moisture, Moisture) };
        }
    }

    public class LightSensor : I2cSensor {
        public LightSensor(int deviceAddress, byte cmd, int busId = 1) : base(deviceAddress, cmd, busId) { }

        public float Light { get; private set; }

        protected override void InternalUpdateValues() {
            Light = ReadInt16();
            ValidateValue(Light, 0, 1000);
        }

        public override SensorValue[] GetSensorValues() {
            return new[] { new SensorValue(this, SensorValueType.Light, Light) };
        }
    }

    public class pHSensor : I2cSensor {
        public pHSensor(int deviceAddress, byte cmd, int busId = 1) : base(deviceAddress, cmd, busId) { }

        public float pH { get; private set; }

        protected override void InternalUpdateValues() {
            pH = ReadFloat();
            ValidateValue(pH, 1, 14);
        }

        public override SensorValue[] GetSensorValues() {
            return new[] { new SensorValue(this, SensorValueType.pH, pH) };
        }
    }

    public class TemperatureSensor : I2cSensor {
        public TemperatureSensor(int deviceAddress, byte cmd, int busId = 1) : base(deviceAddress, cmd, busId) { }

        public float Temperature { get; private set; }

        protected override void InternalUpdateValues() {
            Temperature = ReadFloat();
            ValidateValue(Temperature, 0, 100);
        }

        public override SensorValue[] GetSensorValues() {
            return new[] { new SensorValue(this, SensorValueType.Temperature, Temperature) };
        }
    }

    public class EnvironmentalSensor : I2cSensor {
        public EnvironmentalSensor(int bme280DeviceAddress, int ccs811deviceAddress, int busId = 1) : base(bme280DeviceAddress, 0, busId) {
            try {
                if (Device != null) {
                    I2cBme280 = new Bme280(Device);

                    Ccs811Bme280 = new CCS811BME280Sensor(I2cDevice.Create(new I2cConnectionSettings(busId, ccs811deviceAddress)));
                    Ccs811Bme280.Initialize();
                }
            } catch (Exception e) {
                Console.Write(e);
            }
        }

        Bme280? I2cBme280;
        CCS811BME280Sensor? Ccs811Bme280;

        public double Humidity { get; private set; }
        public double Pressure { get; private set; }
        public Temperature Temperature { get; private set; }
        public int CO2 { get; private set; }
        public int TVOC { get; private set; }

        protected override void InternalUpdateValues() {
            I2cBme280.SetHumiditySampling(Sampling.Standard);
            I2cBme280.SetPressureSampling(Sampling.Standard);
            I2cBme280.SetTemperatureSampling(Sampling.Standard);

            I2cBme280.SetPowerMode(Bmx280PowerMode.Forced);

            Humidity = I2cBme280.ReadHumidityAsync().Result;
            Pressure = I2cBme280.ReadPressureAsync().Result / 1000;
            Temperature = I2cBme280.ReadTemperatureAsync().Result;

            var ccsData = Ccs811Bme280.ReadCO2TVOC();
            CO2 = ccsData[0];
            TVOC = ccsData[1];
            ValidateValue(CO2, 400, 8192);
            ValidateValue(TVOC, 0, 1187);
        }

        public override SensorValue[] GetSensorValues() {
            return new[]
            {
                new SensorValue(this, SensorValueType.Humidity, (float)Humidity),
                new SensorValue(this, SensorValueType.Pressure, (float)Pressure),
                new SensorValue(this, SensorValueType.Temperature, (float)Temperature.Celsius),
                new SensorValue(this, SensorValueType.CO2, CO2),
                new SensorValue(this, SensorValueType.TVOC, TVOC)
            };
        }
    }

    public class DebugSensor : Sensor {
        public float Value { get; private set; }

        protected override void InternalUpdateValues() {
            Random rnd = new Random();
            Value = (float)rnd.NextDouble();
            if (rnd.Next(10) == 5) throw new Exception();
        }

        public override SensorValue[] GetSensorValues() {
            return new[] { new SensorValue(this, SensorValueType.pH, Value) };
        }
    }

    public class EnvironmentalDebugSensor : Sensor {
        public double Humidity { get; private set; }
        public double Pressure { get; private set; }
        public Temperature Temperature { get; private set; }
        public int CO2 { get; private set; }
        public int TVOC { get; private set; }

        protected override void InternalUpdateValues() {
            Random rnd = new Random();
            Humidity = rnd.NextDouble();
            Pressure = rnd.NextDouble();
            Temperature = Temperature.FromCelsius(rnd.NextDouble());
            CO2 = rnd.Next(100);
            TVOC = rnd.Next(100);
        }

        public override SensorValue[] GetSensorValues() {
            return new[]
            {
                new SensorValue(this, SensorValueType.Humidity, (float)Humidity),
                new SensorValue(this, SensorValueType.Pressure, (float)Pressure),
                new SensorValue(this, SensorValueType.Temperature, (float)Temperature.Celsius),
                new SensorValue(this, SensorValueType.CO2, CO2),
                new SensorValue(this, SensorValueType.TVOC, TVOC)
            };
        }
    }
}
