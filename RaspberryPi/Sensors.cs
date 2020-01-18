using CCS811_BME280_Library;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Units;
using System.Device.I2c;
using System.IO;

namespace RaspberryPi {

    public enum SensorValueType : byte { FlowRate, Moisture, Light, Humidity, Pressure, Temperature, CO2, TVOC, CpuTemp }

    class FlowSensor : I2cSensor {
        public FlowSensor(int deviceAddress, byte cmd, int busId = 1) : base(deviceAddress, cmd, busId) { }

        public float FlowRate { get; private set; }

        protected override void InternalUpdateValues() {
            FlowRate = ReadInt16();
        }

        public override SensorValue[] GetSensorValues() {
            return new[] {new SensorValue(SensorValueType.FlowRate, FlowRate)};
        }
    }

    class MoistureSensor : I2cSensor {
        public MoistureSensor(int deviceAddress, byte cmd, int busId = 1) : base(deviceAddress, cmd, busId) { }

        public float Moisture { get; private set; }

        protected override void InternalUpdateValues() {
            Moisture = ReadInt16().Map(0, 1000, 0, 1);
        }
        public override SensorValue[] GetSensorValues() {
            return new[] { new SensorValue(SensorValueType.Moisture, Moisture) };
        }
    }

    class LightSensor : I2cSensor {
        public LightSensor(int deviceAddress, byte cmd, int busId = 1) : base(deviceAddress, cmd, busId) { }

        public float Light { get; private set; }

        protected override void InternalUpdateValues() {
            Light = ReadInt16();
        }

        public override SensorValue[] GetSensorValues() {
            return new[] { new SensorValue(SensorValueType.Light, Light) };
        }
    }

    class EnvironmentalSensor : I2cSensor {
        public EnvironmentalSensor(int bme280DeviceAddress, int ccs811deviceAddress, int busId = 1) : base(bme280DeviceAddress, 0, busId) {
            if (Device != null) {
                I2cBme280 = new Bme280(Device);

                Ccs811Bme280 = new CCS811BME280Sensor(I2cDevice.Create(new I2cConnectionSettings(busId, ccs811deviceAddress)));
                Ccs811Bme280.Initialize();
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
            I2cBme280.SetHumiditySampling(Sampling.LowPower);
            I2cBme280.SetPressureSampling(Sampling.Standard);
            I2cBme280.SetTemperatureSampling(Sampling.Standard);

            I2cBme280.SetPowerMode(Bmx280PowerMode.Forced);

            Humidity = I2cBme280.ReadHumidityAsync().Result;
            Pressure = I2cBme280.ReadPressureAsync().Result;
            Temperature = I2cBme280.ReadTemperatureAsync().Result;

            var ccsData = Ccs811Bme280.ReadCO2TVOC();
            CO2 = ccsData[0];
            TVOC = ccsData[1];
        }

        public override SensorValue[] GetSensorValues() {
            return new[]
            {
                new SensorValue(SensorValueType.Humidity, (float)Humidity),
                new SensorValue(SensorValueType.Pressure, (float)Pressure),
                new SensorValue(SensorValueType.Temperature, (float)Temperature.Celsius),
                new SensorValue(SensorValueType.CO2, CO2),
                new SensorValue(SensorValueType.TVOC, TVOC)
            };
        }
    }

    class CPUInfo : Sensor {
        public float CpuTemp { get; private set; }

        protected override void InternalUpdateValues() {
            CpuTemp = float.Parse(File.ReadAllText("/sys/class/thermal/thermal_zone0/temp")) / 1000f;
        }

        public override SensorValue[] GetSensorValues() {
            return new[] { new SensorValue(SensorValueType.CpuTemp, CpuTemp) };
        }
    }
}
