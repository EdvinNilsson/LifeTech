using System;
using System.Device.I2c;

namespace RaspberryPi
{
    class Sensor
    { 
        public bool Online { get; protected set; }

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
    }

    class I2cSensor : Sensor
    {
        protected I2cSensor(int deviceAddress, int busId)
        {
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
    }
}
