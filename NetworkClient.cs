using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityMonoTogether
{
    internal sealed class NetworkClient : IDisposable
    {
        private NetworkPacket _packet;
        private UdpClient _udpClient;
        private IPEndPoint _serverEndPoint;

        private bool _isConnected;

        public NetworkClient()
        {
            _packet = new NetworkPacket();
            _udpClient = new UdpClient();
            _serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        public void Connect(string address, int port)
        {
            _isConnected = true;
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

            SendPacket();
            ReceivePackets();
        }

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        public Action<NetworkPacket>? ReceivePacket { get; set; }

        public UdpClient GetUdpClient()
        {
            return _udpClient;
        }

        public IPEndPoint GetServerEndPoint()
        {
            return _serverEndPoint;
        }

        public int GetBufferSize()
        {
            return _packet.BufferSize;
        }

        public void WriteBytesToBuffer(IEnumerable<byte> data)
        {
            _packet.WriteBytes(data);
        }

        public void WriteBytesToBuffer(params byte[] data)
        {
            _packet.WriteBytes(data);
        }

        public void WriteInt16ToBuffer(short data)
        {
            _packet.WriteInt16(data);
        }

        public void WriteInt32ToBuffer(int data)
        {
            _packet.WriteInt32(data);
        }

        public void WriteInt64ToBuffer(long data)
        {
            _packet.WriteInt64(data);
        }

        public void WriteSingleToBuffer(float data)
        {
            _packet.WriteSingle(data);
        }

        public void WriteVectorToBuffer(Vector3 data)
        {
            _packet.WriteVector(data);
        }

        public void WriteStringToBuffer(string? data)
        {
            _packet.WriteString(data);
        }

        public void WriteInt16ArrayToBuffer(params short[] data)
        {
            _packet.WriteInt16Array(data);
        }

        public void WriteInt32ArrayToBuffer(params int[] data)
        {
            _packet.WriteInt32Array(data);
        }

        public void WriteInt64ArrayToBuffer(params long[] data)
        {
            _packet.WriteInt64Array(data);
        }

        public void WriteSingleArrayToBuffer(params float[] data)
        {
            _packet.WriteSingleArray(data);
        }

        public void WriteStringArrayToBuffer(params string[] data)
        {
            _packet.WriteStringArray(data);
        }

        public void ClearBuffer()
        {
            _packet.Clear();
        }

        public void SendPacket()
        {
            var buffer = _packet.ToArray();
            _udpClient.Send(buffer, buffer.Length, _serverEndPoint);
        }

        public void Disconnect()
        {
            _isConnected = false;
            _udpClient.Close();
        }

        private async void ReceivePackets()
        {
            await Task.Run(() =>
            {
                while (_isConnected)
                {
                    var serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
                    var receivedBytes = _udpClient.Receive(ref serverEndpoint);

                    using var packet = new NetworkPacket();
                    packet.WriteBytes(receivedBytes);

                    ReceivePacket?.Invoke(packet);
                }
            });
        }

        ~NetworkClient()
        {
            Release();
        }

        private void Release()
        {
            _packet.Dispose();
            _udpClient.Dispose();
        }

        public void Dispose()
        {
            Release();
            GC.SuppressFinalize(this);
        }
    }
}
