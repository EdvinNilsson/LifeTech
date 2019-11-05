using System;
using System.Device.I2c;
using System.IO;

namespace RaspberryPi
{
    class FlowSensor : I2cSensor
    {
        public FlowSensor(int deviceAddress = 0x04) : base(deviceAddress) { }

        public float FlowRate { get; private set; }

        protected override void InternalUpdateValues()
        {
            var command = new byte[1];

            var flowRateData = new byte[2];

            command[0] = 0x04;
            Device!.WriteRead(command, flowRateData);

            FlowRate = BitConverter.ToInt16(flowRateData);
        }
    }

    class MoistureSensor : I2cSensor
    {
        public MoistureSensor(int deviceAddress = 0x08) : base(deviceAddress) { }

        public float Moisture { get; private set; }

        protected override void InternalUpdateValues()
        {
            var command = new byte[1];

            var moistureData = new byte[2];

            command[0] = 0x00;
            Device!.WriteRead(command, moistureData);

            Moisture = BitConverter.ToInt16(moistureData);
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
