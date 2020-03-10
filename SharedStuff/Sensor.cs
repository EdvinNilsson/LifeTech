using System;
using System.Device.I2c;

namespace SharedStuff
{
    public class Sensor
    { 
        public bool Online { get; protected set; }

        public byte SensorId { get; set; }
        public string SensorName { get; set; }

        public void UpdateValues()
        {
            try
            {
                InternalUpdateValues();
                Online = true;
            }
            catch (Exception e)
            {
                Online = false;
                Console.WriteLine($"Sensoravläsningsfel: {e.Message}");
            }
        }

        protected virtual void InternalUpdateValues() { }

        public virtual SensorValue[] GetSensorValues() { return new SensorValue[0]; }
    }

    public class I2cSensor : Sensor
    {
        protected I2cSensor(int deviceAddress, byte cmd, int busId)
        {
            this.cmd = new[] { cmd };
            try
            {
                Device = I2cDevice.Create(new I2cConnectionSettings(busId, deviceAddress));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Enhetsfel: {e.Message}");
                Online = false;
            }
        }

        public I2cDevice? Device { get; }

        byte[] cmd;

        protected byte[] ReadBytes(int length) {
            var dataArray = new byte[length];
            Device!.WriteRead(cmd, dataArray);
            return dataArray;
        }

        protected short ReadInt16() => BitConverter.ToInt16(ReadBytes(2), 0);

        protected float ReadFloat() => BitConverter.ToSingle(ReadBytes(4));

        protected void ValidateValue(float value, float min, float max) {
            if (!(value >= min && value <= max)) {
                Console.WriteLine($"Invalid value {value}. Min:{min} Max:{max}");
                throw new Exception();
            }
        } 
    }
}
