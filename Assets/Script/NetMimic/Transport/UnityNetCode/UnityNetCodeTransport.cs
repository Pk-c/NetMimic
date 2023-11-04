#nullable disable

using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using static NetMimic.INetworkManager;

namespace NetMimic
{
    public class UnityNetCodeTransport : INetworkManager
    {
        public const int MaxConnection = 16;

        private bool _isServer;
        private NetworkDriver _driver;
        private List<NetworkConnection> _connections;
        private readonly List<byte> _clientsIds = new List<byte>();
        private OnRecieveData _onRecieveData;
        private OnNewConnection _onNewConnection;
        private OnDisconnected _onDisconnect;
        private NetworkPipeline _reliablePipeline;
        private NetworkPipeline _unreliablePipeline;

        public void SendData(byte[] data, bool reliable)
        {
            if (_isServer)
            {
                for (int n = 0; n < _connections.Count; n++)
                {
                    SendDataTo(data, n, reliable);
                }
            }
            else
            {
                SendDataTo(data, 0, reliable);
            }
        }

        public void SendDataTo(byte[] data, int clientId, bool reliable)
        {
            SendDataTo(data, !_isServer ? _connections[0] : _connections[clientId], reliable);
        }

        public void SendDataTo(byte[] data, NetworkConnection connection, bool reliable)
        {
            if (connection != null && connection.IsCreated)
            {
                _driver.BeginSend(reliable ? _reliablePipeline : _unreliablePipeline, connection, out var writer);
                for (int n = 0; n < data.Length; n++)
                {
                    writer.WriteByte(data[n]);
                }
                _driver.EndSend(writer);

                if (writer.HasFailedWrites)
                {
                    UnityEngine.Debug.LogWarning("Writing package has failed with " + data.Length + "  bytes ");
                }
            }
        }

        public NetworkError Start(bool isServer, OnRecieveData onRecieveData, OnNewConnection OnNewConnection = null, OnDisconnected onDisconnected = null)
        {
            _isServer = isServer;
            _onRecieveData = onRecieveData;
            _onNewConnection = OnNewConnection;
            _onDisconnect = onDisconnected;
            _connections = new List<NetworkConnection>();

            if (_isServer)
            {
                _isServer = true;
                _driver = NetworkDriver.Create();
                _reliablePipeline = _driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
                _unreliablePipeline = _driver.CreatePipeline(typeof(UnreliableSequencedPipelineStage));

                NetworkEndpoint endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);
                if (_driver.Bind(endpoint) != 0)
                {
                    return NetworkError.FailToBindPort;
                }
                _driver.Listen();
            }
            else
            {
                ConnectClient();
            }

            return NetworkError.None;
        }

        public void ConnectClient()
        {
            var settings = new NetworkSettings();
#if UNITY_EDITOR || DEV_VERSION
            settings.WithNetworkConfigParameters(disconnectTimeoutMS: 2000);
#else
            settings.WithNetworkConfigParameters(disconnectTimeoutMS: 10000);
#endif
            _driver = NetworkDriver.Create(settings);
            _reliablePipeline = _driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            _unreliablePipeline = _driver.CreatePipeline(typeof(UnreliableSequencedPipelineStage));
            var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);

            _connections.Add(_driver.Connect(endpoint));
        }

        public void Stop()
        {
            if (_driver.IsCreated)
            {
                // Close all connections and shut down the server
                for (int i = 0; i < _connections.Count; i++)
                {
                    _connections[i].Disconnect(_driver);
                }

                _driver.ScheduleUpdate().Complete();
                _connections.Clear();
                _driver.Dispose();
            }
        }

        private void UpdateConnection()
        {
            // Clean up connections.
            for (int i = 0; i < _connections.Count; i++)
            {
                if (!_connections[i].IsCreated || _driver.GetConnectionState(_connections[i]) == NetworkConnection.State.Disconnected)
                {
                    _connections.RemoveAtSwapBack(i);

                    if (_onDisconnect != null)
                    {
                        _onDisconnect(_clientsIds[i]);
                    }

                    _clientsIds.RemoveAt(i);

                    break;
                }
            }

            // Accept new connections.
            NetworkConnection c;
            while ((c = _driver.Accept()) != default)
            {
                _connections.Add(c);

                byte id = (byte)(_connections.Count);
                _clientsIds.Add(id);

                if (_onNewConnection != null)
                {
                    _onNewConnection(id);
                }
            }
        }

        private void RecieveDataInternal()
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            if (_isServer)
            {
                for (int i = 0; i < _connections.Count; i++)
                {
                    while ((cmd = _driver.PopEventForConnection(_connections[i], out stream)) != NetworkEvent.Type.Empty)
                    {
                        if (cmd == NetworkEvent.Type.Disconnect)
                        {
                            _connections[i] = default;
                        }

                        if (cmd == NetworkEvent.Type.Data)
                        {
                            RecieveDataInternal(ref stream);
                        }
                    }
                }
            }
            else
            {
                while ((cmd = _connections[0].PopEvent(_driver, out stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Connect)
                    {
                        //Client Connected
                    }

                    if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        _connections[0] = default;
                    }

                    if (cmd == NetworkEvent.Type.Data)
                    {
                        RecieveDataInternal(ref stream);
                    }
                }
            }
        }

        private void RecieveDataInternal(ref DataStreamReader stream)
        {
            using (NativeArray<byte> data = new NativeArray<byte>(stream.Length, Allocator.Temp))
            {
                stream.ReadBytes(data);
                _onRecieveData(data.ToArray());
            }
        }

        public void Update()
        {
            if (!_driver.IsCreated)
                return;

            _driver.ScheduleUpdate().Complete();

            if (_isServer)
            {
                UpdateConnection();
            }

            RecieveDataInternal();
        }
    }
}
