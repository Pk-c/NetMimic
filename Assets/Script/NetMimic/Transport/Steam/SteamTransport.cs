#nullable disable

using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using static NetMimic.INetworkManager;

namespace NetMimic
{
    public class SteamTransport : INetworkManager
    {
        private bool _isServer;
        readonly List<SteamId> _connections = new List<SteamId>();
        private readonly uint _appId = 480;
        private OnRecieveData _onRecieveData;
        private OnNewConnection _onNewConnection;
        private OnDisconnected _onDisconnect;
        private bool _steamInit;

        public Lobby? CurrentLobby { get; private set; }

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
            SteamId TargetSteamId = !_isServer ? _connections[0] : _connections[clientId];

            if (!SteamNetworking.SendP2PPacket(TargetSteamId, data))
            {
                Debug.LogWarning("Steamwork failed to send data with " + data.Length + " bytes");
            }
        }

        public NetworkError Start(bool isServer, OnRecieveData onRecieveData, OnNewConnection OnNewConnection = null, OnDisconnected onDisconnected = null)
        {
            _isServer = isServer;

            if (!_steamInit)
            {
#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    Steamworks.SteamClient.Init(_appId);
                    _steamInit = true;
                }
                catch (System.Exception e)
                {
                    Debug.Log("Steamwork Initialisation Error : " + e.Data.ToString());
                    return NetworkError.InitializationError;
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            if (isServer)
            {
                _ = CreateLobbyAsync();
                SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
                SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
                SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;
                SteamMatchmaking.OnLobbyMemberDisconnected += SteamMatchmaking_OnLobbyMemberDisconnected;
            }
            else
            {
                SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
                SteamFriends.OnGameLobbyJoinRequested += SteamFriends_OnGameLobbyJoinRequested;
            }

            _onRecieveData = onRecieveData;
            _onNewConnection = OnNewConnection;
            _onDisconnect = onDisconnected;

            return NetworkError.None;
        }

        public void Stop()
        {
            if (CurrentLobby.HasValue)
            {
                CurrentLobby.Value.Leave();
            }

            if (_isServer)
            {
                if (CurrentLobby.HasValue)
                {
                    CurrentLobby.Value.SetData("lobbyStatus", "closed");
                }
                CurrentLobby = null;
                SteamMatchmaking.OnLobbyCreated -= SteamMatchmaking_OnLobbyCreated;
                SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoinedCallback;
                SteamMatchmaking.OnLobbyMemberDisconnected -= SteamMatchmaking_OnLobbyMemberDisconnected;
                SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
            }
            else
            {
                SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
                SteamFriends.OnGameLobbyJoinRequested -= SteamFriends_OnGameLobbyJoinRequested;
            }

            _connections.Clear();
            Steamworks.SteamClient.Shutdown();
        }

        public void Update()
        {
            if (Steamworks.SteamClient.IsValid)
            {
                Steamworks.SteamClient.RunCallbacks();
            }

            while (SteamNetworking.IsP2PPacketAvailable())
            {
                var packet = SteamNetworking.ReadP2PPacket();
                if (packet.HasValue)
                {
                    _onRecieveData(packet.Value.Data);
                }
            }
        }

        //Host

        private async Task CreateLobbyAsync()
        {
            CurrentLobby = await SteamMatchmaking.CreateLobbyAsync().ConfigureAwait(false);
        }

        private void SteamMatchmaking_OnLobbyCreated(Result result, Lobby lobby)
        {
            lobby.SetFriendsOnly();
            lobby.SetData("name", "sample kgp lobby");
            lobby.SetJoinable(true);
            Debug.Log("Lobby has been created");
        }

        private void OnLobbyMemberJoinedCallback(Lobby lobby, Friend friend)
        {
            _connections.Add(friend.Id);
            SteamNetworking.AcceptP2PSessionWithUser(friend.Id);

            if (_onNewConnection != null)
            {
                _onNewConnection((byte)(_connections.Count));
            }
        }

        private void SteamMatchmaking_OnLobbyMemberLeave(Lobby arg1, Friend arg2)
        {
            SteamMatchmaking_OnLobbyMemberDisconnected(arg1, arg2);
        }

        private void SteamMatchmaking_OnLobbyMemberDisconnected(Lobby arg1, Friend arg2)
        {
            _onDisconnect?.Invoke((byte)(_connections.IndexOf(arg2.Id)));
            _connections.Remove(arg2.Id);
        }

        //Client

        private void SteamMatchmaking_OnLobbyEntered(Lobby lobby)
        {
            _connections.Add(lobby.Owner.Id);
            CurrentLobby = lobby;
            SteamNetworking.AcceptP2PSessionWithUser(lobby.Owner.Id);
        }

        private void SteamFriends_OnGameLobbyJoinRequested(Lobby lobby, SteamId id)
        {
            lobby.Join();
        }
    }
}
