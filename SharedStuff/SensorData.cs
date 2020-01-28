using System;
using System.Collections.Generic;
using System.IO;

namespace SharedStuff
{
    public class SensorData {

        public SensorData(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            using (BinaryReader reader = new BinaryReader(stream)) {
                Timestamp = UnixTimeToDateTime(reader.ReadInt32());
                Sensors = new SensorValue[(bytes.Length - 4) / 5];
                for (int i = 0; i < Sensors.Length; i++) {
                    Sensors[i] = new SensorValue((SensorValueType)reader.ReadByte(),  reader.ReadSingle());
                }
            }
        }

        public SensorData(DateTime timestamp, Sensor[] sensors)
        {
            Timestamp = timestamp;
            List<SensorValue> tempList = new List<SensorValue>();
            foreach (var sensor in sensors) {
                tempList.AddRange(sensor.GetSensorValues());
            }
            Sensors = tempList.ToArray();
        }

        DateTime UnixTimeToDateTime(int unixTime) {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime).ToLocalTime();
            return dtDateTime;
        }

        public DateTime Timestamp;

        public SensorValue[] Sensors;

        public byte[] Serialize()
        {
            byte[] bytes = new byte[4 + 5 * Sensors.Length];

            Stream stream = new MemoryStream(bytes);

            using (BinaryWriter writer = new BinaryWriter(stream)) {
                writer.Write((int)((DateTimeOffset)Timestamp).ToUnixTimeSeconds());
                foreach (var sensor in Sensors) {
                    writer.Write((byte)sensor.type);
                    writer.Write(sensor.value);
                }
            }
            stream.Close();
            return bytes;
        }

    }
    public class SensorValue
    {
        public SensorValue(SensorValueType type, float value)
        {
            this.type = type;
            this.value = value;
        }

        public SensorValueType type;
        public float value;
    }
}
