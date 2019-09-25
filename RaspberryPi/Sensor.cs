using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Device.I2c;

namespace RaspberryPi
{
    class Sensor
    {
        public Sensor(SensorHub sensorHub, byte sensorIndex)
        {
            SensorHub = sensorHub;
            SensorIndex = sensorIndex;
        }

        public bool Online { get; protected set; }

        public byte SensorIndex { get; set; }

        public SensorHub SensorHub { get; }

        private ushort _sendFrequency;
        /// <summary>
        /// Automatic updates per minute.
        /// </summary>
        public ushort SendFrequency
        {
            get => _sendFrequency;
            set
            {
                _sendFrequency = value;
                WriteSendFrequency();
            }
        }

        public event Action? OnDataUpdate;

        public Action RequestSensorData()
        {
            throw new NotImplementedException();
            return OnDataUpdate;
        }

        public virtual void OnReceiveSensorData(byte[] bytes)
        {
            OnDataUpdate?.Invoke();
        }

        private void WriteSendFrequency()
        {
            byte[] bytes = BitConverter.GetBytes(SendFrequency);
            SensorHub.WriteData(SensorIndex, bytes);
        }
    }

    class FlowSensor : Sensor
    {
        public FlowSensorData? LatestSensorData { get; private set; }

        public FlowSensor(SensorHub sensorHub, byte sensorIndex) : base(sensorHub, sensorIndex)
        {
        }

        public override void OnReceiveSensorData(byte[] bytes)
        {
            LatestSensorData = Deserialize(bytes);
            base.OnReceiveSensorData(bytes);
        }

        private FlowSensorData Deserialize(byte[] sensorData)
        {
            throw new NotImplementedException();
        }
    }

    class FlowSensorData : SensorData
    {
        public float FlowRate;
    }

    class SensorData { }
}
