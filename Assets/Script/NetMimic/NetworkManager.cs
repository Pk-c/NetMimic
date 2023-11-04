using NetMimic;
using static NetMimic.INetworkManager;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NetMimic
{
    public sealed class NetworkManager : IDisposable
    {
        public enum NetTransport
        {
            UnityNetCode,
            Steam,
            FakeNet
        }

        private INetworkManager _networkManager;
        private readonly List<byte> _disconnected = new List<byte>();
        public List<byte> Disconnected { get { return _disconnected; } }
        public OnNewConnection _OnNewConnection;
        public delegate void OnRecieveMessage(byte[] msg);
        readonly Dictionary<PacketType, OnRecieveMessage> _dataListeners = new Dictionary<PacketType, OnRecieveMessage>();

        private byte? _clientId;
        private bool _isServer;
        public byte ClientID { get { return _clientId == null ? byte.MaxValue : _clientId.Value; } }

        private void InitNetwork(NetTransport netTransport)
        {
            Application.runInBackground = true;
            switch (netTransport)
            {
#if UNITY_2021_3_OR_NEWER
                case NetTransport.UnityNetCode: _networkManager = new UnityNetCodeTransport(); break;
#endif
                case NetTransport.Steam: _networkManager = new SteamTransport(); break;
                case NetTransport.FakeNet: _networkManager = new FakeNetworkTransport(); break;
            }
            NetworkError error = _networkManager.Start(_isServer, OnRecieveData, _isServer ? OnNewConnection : null, OnDisconnected);
            if (error != NetworkError.None)
            {
                Debug.LogError(error.ToString());
            }
        }

        public void StartServer(NetTransport transport)
        {
            if (_networkManager != null)
                return;

            _isServer = true;
            _clientId = 0;
            InitNetwork(transport);
            if (_OnNewConnection != null)
            {
                _OnNewConnection(0);
            }
        }

        public void StartClient(NetTransport transport)
        {
            if (_networkManager != null)
                return;

            _isServer = false;
            InitNetwork(transport);
        }

        public bool IsServer()
        {
            return _isServer;
        }

        public void Subscribe(PacketType type, OnRecieveMessage callback)
        {
            if (_dataListeners.ContainsKey(type))
            {
                _dataListeners[type] += callback;
                return;
            }

            _dataListeners.Add(type, callback);
        }

        public void Unsubscribe(PacketType type, OnRecieveMessage callback)
        {
            if (_dataListeners.ContainsKey(type))
            {
                _dataListeners[type] -= callback;
            }
        }

        public void SendData(NetworkMessage message, bool reliable = false)
        {
            if (_clientId == null)
                return;

            byte[] data = message.GetSerializeData();
            SendData(data, reliable);
        }

        public void SendDataTo(NetworkMessage message, int ClientId, bool reliable = false)
        {
            if (_clientId == null)
                return;

            if (!_isServer)
            {
                Debug.LogError("Client only communicate with server");
            }
            else
            {
                byte[] data = message.GetSerializeData();
                SendDataTo(data, ClientId, reliable);
            }
        }

        public void SendData(byte[] packet, bool reliable = false)
        {
            if (_clientId == null)
                return;

            _networkManager.SendData(packet, reliable);
        }

        public void SendDataTo(byte[] packet, int clientId, bool reliable = false)
        {
            if (_clientId == null)
                return;

            _networkManager.SendDataTo(packet, clientId, reliable);
        }

        private void OnNewConnection(byte clientid)
        {
            if (_OnNewConnection != null)
            {
                _OnNewConnection(clientid);
            }

            if (_isServer)
            {
                //Give client id
                NetworkMessage msg = new NetworkMessage(PacketType.JoinPacket, clientid);
                SendData(msg, true);
            }
        }

        private void OnRecieveData(byte[] data)
        {
            NetworkMessage msg = new NetworkMessage(data);
            PacketType type = (PacketType)(msg.Type);

            if (!_isServer)
            {
                if (type == PacketType.JoinPacket && _clientId == null)
                {
                    _clientId = msg.ClientId;
                    if (_OnNewConnection != null)
                    {
                        _OnNewConnection(msg.ClientId);
                    }
                }

                if (type == PacketType.LeavePacket)
                {
                    _disconnected.Add(msg.ClientId);
                }
            }

            if (_dataListeners.TryGetValue(type, out var action))
            {
                action(data);
            }
        }

        public void OnDisconnected(byte clientId)
        {
            if (_isServer)
            {
                NetworkMessage msg = new NetworkMessage(PacketType.LeavePacket, clientId);
                SendData(msg, true);
                _disconnected.Add(clientId);
            }
        }

        public void Update()
        {
            _networkManager?.Update();
        }

        public void Dispose()
        {
            _networkManager?.Stop();
            _networkManager = null;
        }
    }
}
