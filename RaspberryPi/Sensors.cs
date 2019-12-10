using System;
using System.Device.I2c;
using System.IO;
using CCS811_BME280_Library;
using static RaspberryPi.MyMath;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Units;

namespace RaspberryPi
{
    class FlowSensor : I2cSensor
    {
        public FlowSensor(int deviceAddress, int busId = 1) : base(deviceAddress, busId) { }

        public float FlowRate { get; private set; }

        protected override void InternalUpdateValues()
        {
            var command = new byte[1];

            var flowRateData = new byte[2];

            command[0] = 4;
            Device!.WriteRead(command, flowRateData);

            FlowRate = BitConverter.ToInt16(flowRateData);
        }
    }

    class MoistureSensor : I2cSensor
    {
        public MoistureSensor(int deviceAddress, int busId = 1) : base(deviceAddress, busId) { }

        public float Moisture { get; private set; }

        protected override void InternalUpdateValues()
        {
            var command = new byte[1];

            var moistureData = new byte[2];

            command[0] = 0;
            Device!.WriteRead(command, moistureData);

            Moisture = BitConverter.ToInt16(moistureData).Map(0, 1000, 0, 1); ;
        }
    }

    class EnvironmentalSensor : I2cSensor
    {
        public EnvironmentalSensor(int bme280DeviceAddress, int ccs811deviceAddress, int bme280BusId = 1, int ccs811BusId = 1) : base(bme280BusId, bme280DeviceAddress)
        {
            if (Device != null)
            {
                I2cBme280 = new Bme280(Device);

                Ccs811Bme280 = new CCS811BME280Sensor(I2cDevice.Create(new I2cConnectionSettings(ccs811BusId, ccs811deviceAddress)));
                Ccs811Bme280.Initialize();
            }
        }

        Bme280 I2cBme280;
        CCS811BME280Sensor Ccs811Bme280;


        public double Humidity { get; private set; }
        public double Pressure { get; private set; }
        public Temperature Temperature { get; private set; }
        public int CO2 { get; private set; }
        public int TVOC { get; private set; }

        protected override void InternalUpdateValues()
        {
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
    }

    class CPUInfo : Sensor
    {
        public float CpuTemp { get; private set; }

        protected override void InternalUpdateValues()
        {
            CpuTemp = float.Parse(File.ReadAllText("/sys/class/thermal/thermal_zone0/temp")) / 1000f;
        }
    }
}
