using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityMonoTogether
{
    internal sealed class NetworkManager : MonoBehaviour
    {
        private string? _ipAddressStr, _portStr;
        private byte _userId;
        private NetworkClient? _client;

        private Transform? _playerTransform;
        private SortedList<byte, Transform>? _othersTransforms;

        public void CreateClientInstance()
        {
            SceneManager.sceneLoaded += SceneLoadedCallback;

            _client = new NetworkClient
            {
                ReceivePacket = ReceivePacketCallback
            };

            var random = new System.Random();
            _userId = (byte) random.Next(ByteMax);

            _ipAddressStr = IpAddressPHolder;
            _portStr = PortPHolder;
            _othersTransforms = new SortedList<byte, Transform>();
        }

        private void OnGUI()
        {
            _ipAddressStr = GUILayout.TextField(_ipAddressStr);
            _portStr = GUILayout.TextField(_portStr);

            if (_client == null)
                return;

            if (_client.IsConnected)
                if (GUILayout.Button(DisconnectButtonMsg))
                {
                    _client.ClearBuffer();
                    _client.WriteBytesToBuffer((byte)NetworkAction.Disconnect);
                    _client.SendPacket();
                    _client.Disconnect();
                }
            else
                if (GUILayout.Button(ConnectButtonMsg))
                    _client.Connect(_ipAddressStr, int.Parse(_portStr));
        }

        private void ReceivePacketCallback(NetworkPacket packet)
        {
            if (_othersTransforms == null)
                return;

            var otherId = packet.ReadByte();
            var action = (NetworkAction) packet.ReadByte();
            switch (action)
            {
                case NetworkAction.Disconnect:
                    if (_othersTransforms.ContainsKey(otherId))
                        _othersTransforms.Remove(otherId);
                    break;
                case NetworkAction.Transform:
                    if (!_othersTransforms.ContainsKey(otherId))
                        _othersTransforms[otherId] = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;

                    var otherTransform = _othersTransforms[otherId];
                    otherTransform.localPosition = packet.ReadVector();
                    otherTransform.localEulerAngles = packet.ReadVector();
                    break;
            }
        }

        private void SceneLoadedCallback(Scene scene, LoadSceneMode loadSceneMode)
        {
            _playerTransform = null;
            
        }

        private void LoadSceneRootNames()
        {
            
        }

        private const string IpAddressPHolder = "Ip address";
        private const string PortPHolder = "Port";

        private const string ConnectButtonMsg = "Connect";
        private const string DisconnectButtonMsg = "Disconnect";

        private const int ByteMax = 256;
    }
}
