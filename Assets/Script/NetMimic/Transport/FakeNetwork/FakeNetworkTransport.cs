#nullable disable

using static NetMimic.INetworkManager;

namespace NetMimic
{
    public class FakeNetworkTransport : INetworkManager
    {
        private bool _isServer;
        private OnRecieveData _onRecieveData;
        private OnNewConnection _onNewConnection;
        private OnDisconnected _onDisconnect;
        private int _clientCount = 0;

        public void SendData(byte[] data, bool reliable)
        {
            if (_isServer)
            {
                for (int n = 0; n < NetworkPipeSim.Client.Count; n++)
                {
                    SendDataTo(data, NetworkPipeSim.Client[n], false);
                }
            }
            else
            {
                SendDataTo(data, NetworkPipeSim.Host, false);
            }
        }

        public void SendDataTo(byte[] data, int clientId, bool reliable)
        {
            SendDataTo(data, NetworkPipeSim.Client[clientId], reliable);
        }

        public void SendDataTo(byte[] data, FakeNetworkTransport transport, bool reliable)
        {
            if (transport._onRecieveData != null)
                transport._onRecieveData(data);
        }

        public NetworkError Start(bool isServer, OnRecieveData onRecieveData, OnNewConnection OnNewConnection = null, OnDisconnected onDisconnected = null)
        {
            _isServer = isServer;

            if (!isServer)
            {
                if (!NetworkPipeSim.Client.Contains(this))
                {
                    NetworkPipeSim.Client.Add(this);
                }
            }
            else
            {
                NetworkPipeSim.Host = this;
            }

            _onRecieveData = onRecieveData;
            _onNewConnection = OnNewConnection;
            _onDisconnect = onDisconnected;

            return NetworkError.None;
        }

        public void Stop()
        {
            if (!_isServer)
            {
                int index = NetworkPipeSim.Client.IndexOf(this);
                if (index >= 0)
                {
                    NetworkPipeSim.Client.RemoveAt(index);
                }
            }
        }

        public void Update()
        {
            if (_isServer)
            {
                if (_clientCount < NetworkPipeSim.Client.Count)
                {
                    _clientCount = NetworkPipeSim.Client.Count;
                    _onNewConnection((byte)NetworkPipeSim.Client.Count);
                }
            }
        }
    }
}
