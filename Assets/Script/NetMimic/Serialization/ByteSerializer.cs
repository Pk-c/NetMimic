namespace NetMimic
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class ByteSerializer
    {
        private List<byte> buffer = new List<byte>();

        private int readIndex = 0;

        public byte ReadByte()
        {
            if (readIndex < buffer.Count)
            {
                return buffer[readIndex++];
            }
            else
            {
                return 0;
            }
        }

        public float ReadFloat()
        {
            if (readIndex + sizeof(float) <= buffer.Count)
            {
                float result = BitConverter.ToSingle(buffer.ToArray(), readIndex);
                readIndex += sizeof(float);
                return result;
            }
            else
            {
                return 0.0f;
            }
        }

        public int ReadInt()
        {
            if (readIndex + sizeof(int) <= buffer.Count)
            {
                int result = BitConverter.ToInt32(buffer.ToArray(), readIndex);
                readIndex += sizeof(int);
                return result;
            }
            else
            {
                return 0;
            }
        }
        public string ReadString()
        {
            int length = ReadInt(); // Read the length of the string in bytes
            if (readIndex + length <= buffer.Count)
            {
                string result = Encoding.UTF8.GetString(buffer.ToArray(), readIndex, length);
                readIndex += length;
                return result;
            }
            else
            {
                return string.Empty; // Return empty string if not enough data
            }
        }

        public uint ReadUInt()
        {
            if (readIndex + sizeof(uint) <= buffer.Count)
            {
                uint result = BitConverter.ToUInt32(buffer.ToArray(), readIndex);
                readIndex += sizeof(uint);
                return result;
            }
            else
            {
                return 0;
            }
        }

        public void WriteByte(byte value)
        {
            buffer.Add(value);
        }

        public void WriteFloat(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            buffer.AddRange(bytes);
        }

        public void WriteInt(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            buffer.AddRange(bytes);
        }

        public void WriteUInt(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            buffer.AddRange(bytes);
        }

        public void WriteString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteInt(bytes.Length); // Write the length of the string in bytes
            buffer.AddRange(bytes);
        }

        public byte[] GetBuffer()
        {
            return buffer.ToArray();
        }

        public void LoadBuffer(byte[] newBuffer)
        {
            buffer.Clear();
            buffer.AddRange(newBuffer);
            readIndex = 0;
        }
    }
}