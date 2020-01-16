using System;
using System.Collections.Generic;
using System.IO;
namespace RaspberryPi {
    class SensorData {

        public SensorData(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            using (BinaryReader reader = new BinaryReader(stream)) {
                timeStamp = UnixTimeToDateTime(reader.ReadInt32());
                sensors = new SensorValue[(bytes.Length - 4) / 5];
                for (int i = 0; i < sensors.Length; i++) {
                    sensors[i] = new SensorValue((SensorValueType)reader.ReadByte(),  reader.ReadSingle());
                }
            }
        }

        public SensorData(DateTime timeStamp, Sensor[] sensors)
        {
            this.timeStamp = timeStamp;
            List<SensorValue> tempList = new List<SensorValue>();
            foreach (var sensor in sensors) {
                tempList.AddRange(sensor.GetSensorValues());
            }
            this.sensors = tempList.ToArray();
        }

        DateTime UnixTimeToDateTime(int unixTime) {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime).ToLocalTime();
            return dtDateTime;
        }

        DateTime timeStamp;

        SensorValue[] sensors;

        public byte[] Serialize()
        {
            byte[] bytes = new byte[4 + 5 * sensors.Length];

            Stream stream = new MemoryStream(bytes);

            using (BinaryWriter writer = new BinaryWriter(stream)) {
                writer.Write((int)((DateTimeOffset)timeStamp).ToUnixTimeSeconds());
                foreach (var sensor in sensors) {
                    writer.Write((byte)sensor.type);
                    writer.Write(sensor.value);
                }
            }
            stream.Close();
            return bytes;
        }
    }
}
