using System;
using System.Collections.Generic;
using System.IO;

namespace SharedStuff
{
    public class SensorData {

        public SensorData(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            using BinaryReader reader = new BinaryReader(stream);
            Timestamp = reader.ReadInt32();
            Sensors = new SensorValue[(bytes.Length - 4) / 6];
            for (int i = 0; i < Sensors.Length; i++) {
                Sensors[i] = new SensorValue(reader.ReadByte(), (SensorValueType)reader.ReadByte(),  reader.ReadSingle());
            }
        }

        public SensorData(DateTime timestamp, Sensor[] sensors)
        {
            Timestamp = (int)((DateTimeOffset)timestamp).ToUnixTimeSeconds();
            List<SensorValue> tempList = new List<SensorValue>();
            foreach (var sensor in sensors) {
                if (sensor.Online) tempList.AddRange(sensor.GetSensorValues());
            }
            Sensors = tempList.ToArray();
        }

        DateTime UnixTimeToDateTime(int unixTime) {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime).ToLocalTime();
            return dtDateTime;
        }

        public int Timestamp;

        public SensorValue[] Sensors;

        public byte[] Serialize()
        {
            byte[] bytes = new byte[4 + 6 * Sensors.Length];

            Stream stream = new MemoryStream(bytes);

            using (BinaryWriter writer = new BinaryWriter(stream)) {
                writer.Write(Timestamp);
                foreach (var sensor in Sensors) {
                    writer.Write(sensor.id);
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
        public SensorValue(byte id, SensorValueType type, float value) {
            this.id = id;
            this.type = type;
            this.value = value;
        }

        public SensorValue(Sensor sensor, SensorValueType type, float value)
        {
            id = sensor.SensorId;
            this.type = type;
            this.value = value;
        }

        public byte id;
        public SensorValueType type;
        public float value;
    }
}
