#nullable disable

namespace NetMimic
{
    public enum NetworkError
    {
        None,
        FailToBindPort,
        InitializationError
    }

    public interface INetworkManager
    {
        public delegate void OnNewConnection(byte id);
        public delegate void OnRecieveData(byte[] data);
        public delegate void OnDisconnected(byte id);

        public NetworkError Start(bool isServer, OnRecieveData onRecieveData, OnNewConnection OnNewConnection = null, OnDisconnected onDisconnected = null);
        public void Update();
#pragma warning disable CA1716 // Identifiers should not match keywords
        public void Stop();
#pragma warning restore CA1716 // Identifiers should not match keywords
        public void SendData(byte[] data, bool reliable = false);
        public void SendDataTo(byte[] data, int clientId, bool reliable);
    }
}
