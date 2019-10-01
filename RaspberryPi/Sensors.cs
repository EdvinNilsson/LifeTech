using System;
using System.Device.I2c;

namespace RaspberryPi
{
    class FlowSensor : Sensor
    {
        public FlowSensor(int deviceAddress) : base(deviceAddress) { }

        public float FlowRate { get; private set; }

        protected override void InternalUpdateValues()
        {
            var command = new byte[1];

            var flowRateData = new byte[4];

            command[0] = 0x00;
            Device!.WriteRead(command, flowRateData);

            FlowRate = BitConverter.ToSingle(flowRateData);
        }
    }
}
