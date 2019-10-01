using System;
using System.Device.I2c;

namespace RaspberryPi
{
    class Sensor
    {
        public Sensor(int deviceAddress)
        {
            try
            {
                Device = I2cDevice.Create(new I2cConnectionSettings(1, deviceAddress));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Enhetsfel: {e.Message}");
                Online = false;
            }
        }

        public bool Online { get; private set; }

        public I2cDevice? Device { get; }

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
                Console.WriteLine($"Sensoravläsningsfel: {e.Message}\n{e.StackTrace}");
            }
        }

        protected virtual void InternalUpdateValues() { }
    }
}
