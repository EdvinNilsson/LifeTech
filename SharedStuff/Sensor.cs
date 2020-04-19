using System;
using System.Device.I2c;

namespace SharedStuff
{
    public class Sensor
    {
        protected Sensor(byte sensorId, string sensorName)
        {
            SensorId = sensorId;
            SensorName = sensorName;
        }

        public byte SensorId { get; set; }
        public string SensorName { get; set; }

        public void UpdateValues()
        {
            try
            {
                InternalUpdateValues();
            }
            catch (Exception e)
            {
                foreach (SensorValue sensorValue in GetSensorValues())
                {
                    sensorValue.online = false;
                }
                Console.WriteLine($"Sensoravläsningsfel: {e.Message}");
            }
        }

        protected virtual void InternalUpdateValues() { }

        public virtual SensorValue[] GetSensorValues() { return new SensorValue[0]; }
    }

    public class I2cSensor : Sensor
    {
        protected I2cSensor(byte sensorId, string sensorName, int deviceAddress, byte cmd, int busId) : base(sensorId, sensorName)
        {
            this.cmd = new[] { cmd };
            try
            {
                Device = I2cDevice.Create(new I2cConnectionSettings(busId, deviceAddress));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Enhetsfel: {e.Message}");
            }
        }

        public I2cDevice? Device { get; }

        byte[] cmd;

        protected byte[] ReadBytes(int length)
        {
            var dataArray = new byte[length];
            Device!.WriteRead(cmd, dataArray);
            return dataArray;
        }

        protected short ReadInt16() => BitConverter.ToInt16(ReadBytes(2));

        protected float ReadFloat() => BitConverter.ToSingle(ReadBytes(4));

        protected bool ValidateValue(SensorValue sensorValue, float min, float max) => ValidateValue(sensorValue.value, min, max);
        protected bool ValidateValue(float value, float min, float max) {
            if (value >= min && value <= max) return true;
            return false;
        } 
    }
}
