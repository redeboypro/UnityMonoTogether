using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityMonoTogether
{
    internal sealed class NetworkPacket : IDisposable
    {
        private readonly List<byte> _byteBuffer;
        private int _readPosition;
        private bool _isDisposed;

        public NetworkPacket()
        {
            _byteBuffer = new List<byte>();
        }

        public int BufferSize
        {
            get
            {
                return _byteBuffer.Count;
            }
        }

        public int ToRead
        {
            get
            {
                return BufferSize - _readPosition;
            }
        }

        public byte[] ToArray()
        {
            return _byteBuffer.ToArray();
        }

        public byte ReadByte()
        {
            var data = _byteBuffer[_readPosition];
            _readPosition++;
            return data;
        }

        public byte[] ReadBytes(int length)
        {
            var data = _byteBuffer.GetRange(_readPosition, length).ToArray();
            _readPosition += length;
            return data;
        }

        public short ReadInt16()
        {
            return BitConverter.ToInt16(ReadBytes(Int16Size), 0);
        }

        public int ReadInt32()
        {
            return BitConverter.ToInt32(ReadBytes(Int32Size), 0);
        }

        public long ReadInt64()
        {
            return BitConverter.ToInt64(ReadBytes(Int64Size), 0);
        }

        public float ReadSingle()
        {
            return BitConverter.ToSingle(ReadBytes(SingleSize), 0);
        }

        public Vector3 ReadVector()
        {
            Vector3 resultVector;
            {
                resultVector.x = ReadSingle();
                resultVector.y = ReadSingle();
                resultVector.z = ReadSingle();
            }
            return resultVector;
        }

        public string ReadString(Encoding encoding)
        {
            var length = ReadInt32();
            return encoding.GetString(ReadBytes(length), 0, length);
        }

        public string ReadString()
        {
            return ReadString(Encoding.ASCII);
        }

        public bool TryReadBytes(int length, out byte[] data)
        {
            data = Array.Empty<byte>();
            if (length > ToRead)
                return false;

            data = ReadBytes(length);
            return true;
        }

        public bool TryReadInt16(out short data)
        {
            data = 0;
            if (Int16Size > ToRead)
                return false;

            data = ReadInt16();
            return true;
        }

        public bool TryReadInt32(out int data)
        {
            data = 0;
            if (Int32Size > ToRead)
                return false;

            data = ReadInt32();
            return true;
        }

        public bool TryReadInt64(out long data)
        {
            data = 0;
            if (Int64Size > ToRead)
                return false;

            data = ReadInt64();
            return true;
        }

        public bool TryReadSingle(out float data)
        {
            data = 0;
            if (SingleSize > ToRead)
                return false;

            data = ReadSingle();
            return true;
        }

        public bool TryReadString(Encoding encoding, out string data)
        {
            data = string.Empty;

            if (TryReadInt32(out var length))
            {
                if (length > ToRead)
                    return false;

                data = encoding.GetString(ReadBytes(length), 0, length);
            }

            return true;
        }

        public bool TryReadString(out string data)
        {
            return TryReadString(Encoding.ASCII, out data);
        }

        public bool TryReadInt16Array(out short[]? data)
        {
            data = null;

            if (TryReadInt32(out var length))
            {
                if (length > ToRead)
                    return false;

                data = new short[length];
                for (var i = 0; i < length; i++)
                    data[i] = ReadInt16();
            }

            return true;
        }

        public bool TryReadInt32Array(out int[]? data)
        {
            data = null;

            if (TryReadInt32(out var length))
            {
                if (length > ToRead)
                    return false;

                data = new int[length];
                for (var i = 0; i < length; i++)
                    data[i] = ReadInt32();
            }

            return true;
        }

        public bool TryReadInt64Array(out long[]? data)
        {
            data = null;

            if (TryReadInt32(out var length))
            {
                if (length > ToRead)
                    return false;

                data = new long[length];
                for (var i = 0; i < length; i++)
                    data[i] = ReadInt64();
            }

            return true;
        }

        public bool TryReadSingleArray(out float[]? data)
        {
            data = null;

            if (TryReadInt32(out var length))
            {
                if (length > ToRead)
                    return false;

                data = new float[length];
                for (var i = 0; i < length; i++)
                    data[i] = ReadSingle();
            }

            return true;
        }

        public bool TryReadStringArray(Encoding encoding, out string[]? data)
        {
            data = null;

            if (TryReadInt32(out var length))
            {
                if (length > ToRead)
                    return false;

                data = new string[length];
                for (var i = 0; i < length; i++)
                    data[i] = ReadString(encoding);
            }

            return true;
        }

        public bool TryReadStringArray(out string[]? data)
        {
            return TryReadStringArray(Encoding.ASCII, out data);
        }

        public void WriteBytes(IEnumerable<byte> data)
        {
            _byteBuffer.AddRange(data);
        }

        public void WriteInt16(short data)
        {
            var bytes = BitConverter.GetBytes(data);
            _byteBuffer.AddRange(bytes);
        }

        public void WriteInt32(int data)
        {
            var bytes = BitConverter.GetBytes(data);
            _byteBuffer.AddRange(bytes);
        }

        public void WriteInt64(long data)
        {
            var bytes = BitConverter.GetBytes(data);
            _byteBuffer.AddRange(bytes);
        }

        public void WriteSingle(float data)
        {
            var bytes = BitConverter.GetBytes(data);
            _byteBuffer.AddRange(bytes);
        }

        public void WriteVector(Vector3 data)
        {
            WriteSingle(data.x);
            WriteSingle(data.y);
            WriteSingle(data.z);
        }

        public void WriteString(string? data)
        {
            if (data == null)
                return;

            WriteInt32(data.Length);
            _byteBuffer.AddRange(Encoding.ASCII.GetBytes(data));
        }

        public void WriteInt16Array(short[] data)
        {
            WriteInt32(data.Length);
            foreach (var element in data)
                WriteInt16(element);
        }

        public void WriteInt32Array(int[] data)
        {
            WriteInt32(data.Length);
            foreach (var element in data)
                WriteInt32(element);
        }

        public void WriteInt64Array(long[] data)
        {
            WriteInt32(data.Length);
            foreach (var element in data)
                WriteInt64(element);
        }

        public void WriteSingleArray(float[] data)
        {
            WriteInt32(data.Length);
            foreach (var element in data)
                WriteSingle(element);
        }

        public void WriteStringArray(string?[] data)
        {
            WriteInt32(data.Length);
            foreach (var element in data)
                WriteString(element);
        }

        public void Clear()
        {
            _byteBuffer.Clear();
            _readPosition = 0;
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                    Clear();
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
         
        private const int Int16Size = sizeof(short);
        private const int Int32Size = sizeof(int);
        private const int Int64Size = sizeof(long);
        private const int SingleSize = sizeof(float);
    }
}
