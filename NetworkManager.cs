using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityMonoTogether
{
    public sealed class NetworkManager : MonoBehaviour
    {
        private string? _ipAddressStr, _portStr;
        private byte _userId;
        private NetworkClient? _client;

        private List<byte>? _incomingClients;
        private SortedList<byte, Transform>? _othersTransforms;
        private List<Transform>? _sceneRoot;
        private Transform? _selectedTransform;

        private Vector2 _scrollVector;

        private bool _isURP;

        public void OnLoad()
        {
            _client = new NetworkClient
            {
                ReceivePacket = ReceivePacketCallback
            };

            var random = new System.Random();
            _userId = (byte) random.Next(ByteMax);

            _ipAddressStr = IpAddressPHolder;
            _portStr = PortPHolder;
            _incomingClients = new List<byte>();
            _othersTransforms = new SortedList<byte, Transform>();
            _sceneRoot = new List<Transform>();

            UpdateSceneRoot();
            PacketSendingLoop();
            Screen.SetResolution(800, 600, false);
        }

        private void OnGUI()
        {
            _isURP = GUILayout.Toggle(_isURP, "Is URP");
            _ipAddressStr = GUILayout.TextField(_ipAddressStr);
            _portStr = GUILayout.TextField(_portStr);

            if (_client == null)
                return;

            if (_client.IsConnected)
            {
                if (GUILayout.Button(DisconnectButtonMsg))
                {
                    _client.ClearBuffer();
                    _client.WriteBytesToBuffer(_userId);
                    _client.WriteBytesToBuffer((byte) NetworkAction.Disconnect);
                    _client.SendPacket();
                    _client.Disconnect();
                }

                if (_sceneRoot == null)
                    return;

                GUILayout.BeginHorizontal(GUI.skin.box);
                _scrollVector = GUILayout.BeginScrollView(_scrollVector);

                foreach (var obj in _sceneRoot)
                    if (GUILayout.Button(obj.name))
                        _selectedTransform = obj;

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();

                if (GUILayout.Button(RefreshButtonMsg))
                    UpdateSceneRoot();
            }
            else
                if (GUILayout.Button(ConnectButtonMsg))
                    _client.Connect(_ipAddressStr, int.Parse(_portStr));
        }

        private void ReceivePacketCallback(NetworkPacket packet)
        {
            if (_othersTransforms == null || _incomingClients == null)
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
                    {
                        if (!_incomingClients.Contains(otherId))
                            _incomingClients?.Add(otherId);

                        return;
                    }

                    var otherTransform = _othersTransforms[otherId];
                    otherTransform.localPosition = packet.ReadVector();
                    otherTransform.localEulerAngles = packet.ReadVector();
                    break;
            }
        }

        public void UpdateSceneRoot()
        {
            if (_sceneRoot == null)
                return;

            _sceneRoot.Clear();

            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                var objTransform = obj.transform;
                if (!_sceneRoot.Contains(objTransform))
                    _sceneRoot?.Add(objTransform);
            }
        }

        private async void PacketSendingLoop()
        {
            await Task.Run(() =>
            {
                var isConnected = true;
                while (isConnected)
                    if (_selectedTransform != null && _client != null)
                    {
                        var localPosition = _selectedTransform.localPosition;
                        var localEulerAngles = _selectedTransform.localEulerAngles;

                        _client.ClearBuffer();
                        _client.WriteBytesToBuffer(_userId);
                        _client.WriteBytesToBuffer((byte)NetworkAction.Transform);
                        _client.WriteVectorToBuffer(localPosition);
                        _client.WriteVectorToBuffer(localEulerAngles);
                        _client.SendPacket();

                        isConnected = _client.IsConnected;
                    }
            });
        }

        private void FixedUpdate()
        {
            Application.runInBackground = true;

            if (_incomingClients != null && _othersTransforms != null)
            {
                foreach (var id in _incomingClients)
                {
                    var otherInstance = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    DontDestroyOnLoad(otherInstance);

                    if (_isURP)
                        otherInstance.GetComponent<MeshRenderer>().material = 
                            new Material(Shader.Find("Universal Render Pipeline/Lit"));

                    _othersTransforms.Add(id, otherInstance.transform);

                }

                _incomingClients.Clear();
            }
        }

        private const string IpAddressPHolder = "Ip address";
        private const string PortPHolder = "Port";

        private const string ConnectButtonMsg = "Connect";
        private const string DisconnectButtonMsg = "Disconnect";
        private const string RefreshButtonMsg = "Refresh";

        private const int ByteMax = 256;
    }
}
