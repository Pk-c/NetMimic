using NetMimic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Game
{
    public class Network : MonoBehaviour
    {
        public enum NetworkMode
        {
            Host,
            Client,
            FakeNet
        }

        private int EntityCount { get; set; } = 0;

        public Vector3 DebugOffset = Vector3.zero;
        public float PacketFrequency = 0.2f;

        private NetworkManager NetworkManager = new NetworkManager();
        private Dictionary<int, NetEntity> NetEntity = new Dictionary<int, NetEntity>();
        private List<int> ToInstantiate = new List<int>();
        private EntityDataMessage Data = null;
        private float PacketTime = 0.0f;

        public bool IsReady() { return Data != null; }

        public static NetworkMode Mode = NetworkMode.Host;
        public static NetworkManager.NetTransport Transport = NetworkManager.NetTransport.UnityNetCode;

        public void Awake()
        {
            GameMain.RegisterNet(this);

            switch (Mode)
            {
                case NetworkMode.Host:
                    StartServer();
                    break;
                case NetworkMode.Client:
                    StartClient();
                    break;
                case NetworkMode.FakeNet:
                    Transport = NetworkManager.NetTransport.FakeNet;
                    StartServer();
                    Mode = NetworkMode.Client;
                    SceneManager.LoadSceneAsync("FakeNet", LoadSceneMode.Additive);
                    break;
            }
        }

        public void StartServer()
        {
            NetworkManager.StartServer(Transport);
            NetworkManager.Subscribe(NetMimic.PacketType.EntitiesData, OnRecieveEntityData);
            InitGame(NetworkManager.ClientID);
        }

        public void StartClient()
        {
            NetworkManager.StartClient(Transport);
            NetworkManager.Subscribe(NetMimic.PacketType.EntitiesData, OnRecieveEntityData);
            NetworkManager._OnNewConnection += InitGame;
        }

        private void InitGame(byte id)
        {
            Data = new EntityDataMessage(id);
        }

        public byte GetClientID()
        {
            return NetworkManager.ClientID;
        }

        private void OnRecieveEntityData(byte[] msg)
        {
            if (Data == null)
                return;

            Data.Clear();
            Data.DeSerialize(msg);

            //Update data from remote
            for (int i = 0; i < Data.EntityData.Entity.Count; i++)
            {
                if (!NetEntity.ContainsKey(Data.EntityData.Entity[i]))
                {
                    if (!ToInstantiate.Contains(Data.EntityData.Entity[i]))
                    {
                        InstantiatePrefabFromAddress(Data.EntityData.Asset[i], Data.EntityData.Positions[i] + DebugOffset, Data.EntityData.Rotations[i], Data.EntityData.Entity[i], false);
                    }
                }
                else
                {
                    if (!NetEntity[Data.EntityData.Entity[i]].Owned)
                    {
                        //UpdateEntity
                        NetEntity[Data.EntityData.Entity[i]].SetPositionAndRotation(Data.EntityData.Positions[i] + DebugOffset, Data.EntityData.Rotations[i]);
                        Attributes attributes = GameMain.GetAttributes(NetEntity[Data.EntityData.Entity[i]].gameObject);
                        if( attributes != null )
                        {
                            NetworkAttributes nAttribute = Data.EntityData.NetworkAttributes[i];
                            foreach( var attr in nAttribute.intAttributes )
                            {
                                attributes.Get(attr.Key).intValue = attr.Value;
                            }
                            foreach (var attr in nAttribute.floatAttributes)
                            {
                                attributes.Get(attr.Key).floatValue = attr.Value;
                            }
                            foreach (var attr in nAttribute.stringAttributes)
                            {
                                attributes.Get(attr.Key).stringValue = attr.Value;
                            }
                        }
                    }
                }
            }
        }

        public void Update()
        {
            NetworkManager.Update();

            if (Data == null)
                return;

            if (PacketTime <= 0.0f)
            {
                Data.Clear();

                //Send data to remote
                foreach (var item in NetEntity.Keys)
                {
                    if (NetworkManager.IsServer() || NetEntity[item].Owned)
                    {
                        Data.AddEntry(item, NetEntity[item], GameMain.GetAttributes(NetEntity[item].gameObject));
                    }
                }

                if (Data.EntityData.Entity.Count > 0)
                {
                    NetworkManager.SendData(Data.GetSerializeData());
                }

                PacketTime = PacketFrequency;
            }

            PacketTime -= Time.deltaTime;
        }

        public void InstantiatePrefabFromAddress(string address, Vector3 position, Quaternion rotation, int id, bool owner)
        {
            ToInstantiate.Add(id);
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, position, rotation);
            handle.Completed += (h) => OnPrefabInstantiated(h, address, id, owner);
        }

        private void OnPrefabInstantiated(AsyncOperationHandle<GameObject> handle, string address, int id, bool owner)
        {
            ToInstantiate.Remove(id);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                RegisterNetworkObject(Game.NetEntity.Create(handle.Result, address, id, owner));

                if (Transport == NetworkManager.NetTransport.FakeNet)
                {
                    SceneManager.MoveGameObjectToScene(handle.Result, gameObject.scene);
                }
            }
            else
            {
                Debug.LogError("Failed to instantiate prefab.");
            }
        }

        public void RegisterNetworkObject(NetEntity entity)
        {
            NetEntity[entity.ID] = entity;
        }

        public int CreateNetID()
        {
            EntityCount++;
            NetworkId id = new NetworkId(EntityCount, GetClientID());
            return id.Value;
        }
    }
}