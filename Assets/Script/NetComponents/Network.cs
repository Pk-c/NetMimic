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

        public Vector3 m_DebugOffset = Vector3.zero;
        public float m_PacketFrequency = 0.2f;

        private NetworkManager m_NetworkManager = new NetworkManager();
        private Dictionary<int, NetEntity> m_NetEntity = new Dictionary<int, NetEntity>();
        private List<int> m_ToInstantiate = new List<int>();
        private EntityDataMessage m_Data = null;
        private float m_PacketTime = 0.0f;

        public bool IsReady() { return m_Data != null; }

        public static NetworkMode Mode = NetworkMode.Host;
        public static NetworkManager.NetTransport Transport = NetworkManager.NetTransport.UnityNetCode;

        public void Awake()
        {
            GameMain.Register(this);

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
            m_NetworkManager.StartServer(Transport);
            m_NetworkManager.Subscribe(NetMimic.PacketType.EntitiesData, OnRecieveEntityData);
            InitGame(m_NetworkManager.ClientID);
        }

        public void StartClient()
        {
            m_NetworkManager.StartClient(Transport);
            m_NetworkManager.Subscribe(NetMimic.PacketType.EntitiesData, OnRecieveEntityData);
            m_NetworkManager._OnNewConnection += InitGame;
        }

        private void InitGame(byte id)
        {
            m_Data = new EntityDataMessage(id);
        }

        public byte GetClientID()
        {
            return m_NetworkManager.ClientID;
        }

        private void OnRecieveEntityData(byte[] msg)
        {
            if (m_Data == null)
                return;

            m_Data.Clear();
            m_Data.DeSerialize(msg);

            //Update data from remote
            for (int i = 0; i < m_Data.EntityData.Entity.Count; i++)
            {
                if (!m_NetEntity.ContainsKey(m_Data.EntityData.Entity[i]))
                {
                    if (!m_ToInstantiate.Contains(m_Data.EntityData.Entity[i]))
                    {
                        InstantiatePrefabFromAddress(m_Data.EntityData.Asset[i], m_Data.EntityData.Positions[i] + m_DebugOffset, m_Data.EntityData.Rotations[i], m_Data.EntityData.Entity[i], false);
                    }
                }
                else
                {
                    if (!m_NetEntity[m_Data.EntityData.Entity[i]].Owned)
                    {
                        //UpdateEntity
                        m_NetEntity[m_Data.EntityData.Entity[i]].transform.SetPositionAndRotation(m_Data.EntityData.Positions[i] + m_DebugOffset, m_Data.EntityData.Rotations[i]);
                    }
                }
            }
        }

        public void Update()
        {
            m_NetworkManager.Update();

            if (m_Data == null)
                return;

            if (m_PacketTime <= 0.0f)
            {
                m_Data.Clear();

                //Send data to remote
                foreach (var item in m_NetEntity.Keys)
                {
                    if (m_NetworkManager.IsServer() || m_NetEntity[item].Owned)
                    {
                        m_Data.AddEntry(item, m_NetEntity[item]);
                    }
                }

                if (m_Data.EntityData.Entity.Count > 0)
                {
                    m_NetworkManager.SendData(m_Data.GetSerializeData());
                }

                m_PacketTime = m_PacketFrequency;
            }

            m_PacketTime -= Time.deltaTime;
        }

        public void InstantiatePrefabFromAddress(string address, Vector3 position, Quaternion rotation, int id, bool owner)
        {
            m_ToInstantiate.Add(id);
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, position, rotation);
            handle.Completed += (h) => OnPrefabInstantiated(h, address, id, owner);
        }

        private void OnPrefabInstantiated(AsyncOperationHandle<GameObject> handle, string address, int id, bool owner)
        {
            m_ToInstantiate.Remove(id);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                m_NetEntity[id] = NetEntity.Create(handle.Result, address, id, owner);

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
    }
}