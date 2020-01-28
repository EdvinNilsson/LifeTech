using System;
using System.Device.I2c;

namespace CCS811_BME280_Library
{
    public class CCS811BME280Sensor
    {
        private I2cDevice I2C { get; }
        internal const byte CCS811_I2C_ADDR = 0x5A;
        internal const byte CCS811BME280_I2C_ADDR = 0x5B;

        private const byte MEAS_MODE = 0x01;
        private const byte MEAS_DRIVE_1SEC = 0x10;
        private const byte MEAS_DRIVE_60SEC = 0x30;
        private const byte APP_START = 0xF4;
        private const byte HW_ID = 0x20;
        private const byte STATUS = 0x00;
        private const byte ERROR_ID = 0xE0;
        private const byte SW_RESET = 0xFF;


        public CCS811BME280Sensor(I2cDevice i2CDevice)
        {
            I2C = i2CDevice;
        }

        internal void Initialize()
        {
            var resetCommand = new byte[] { SW_RESET, 0x11, 0xE5, 0x72, 0x8A };
            I2C.Write(resetCommand);

            byte hwId = ReadDataByte(HW_ID);

            if (hwId == 0x81)
            {
                byte status = ReadDataByte(STATUS);
                Console.WriteLine(status);
                //16 is OK but application mode not started

                //start application mode
                var command = new byte[] { APP_START };
                I2C.Write(command);

                //if 255 : everything is OK - BUT ERROR flag to 1 => check error. should be 254
                status = ReadDataByte(STATUS);
                Console.WriteLine(status);

                //output the error code
                HandleError();

                //set the measurement to 60 seconds
                WriteDataByte(MEAS_MODE, MEAS_DRIVE_1SEC);

            }
            else
            {
                HandleError();
            }
        }

        private void HandleError()
        {
            byte errorId = ReadDataByte(ERROR_ID);

            Console.WriteLine("Error ID: " + errorId);
        }

        internal int[] ReadCO2TVOC()
        {
            //byte status = ReadDataByte(STATUS);

            //Debug.WriteLine("status: "+ status);

            var data = ReadDataBlock(0x02, 8);
            var co2 = (data[0] << 8) | data[1];
            var tvoc = (data[2] << 8) | data[3];
            Console.WriteLine("CO2:" + co2);
            Console.WriteLine("TVOC:" + tvoc);
            return new int[] { co2, tvoc };
        }

        private void WriteDataByte(byte reg, byte val)
        {
            var command = new[] { reg, val };
            I2C.Write(command);
        }

        private byte ReadDataByte(byte reg)
        {
            var command = new[] { reg };
            var data = new byte[1];

            I2C.WriteRead(command, data);

            return data[0];
        }

        byte[] ReadDataBlock(byte reg, int len)
        {
            var command = new[] { reg };
            var data = new byte[len];

            I2C.WriteRead(command, data);

            return data;
        }
    }
}