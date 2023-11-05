using NetMimic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameMain : MonoBehaviour
    {
        public NetworkManager.NetTransport Transport = NetworkManager.NetTransport.UnityNetCode;

        private static List<Network> Network = new List<Network>();
        private static Dictionary<int, Attributes> Attributes = new Dictionary<int, Attributes>();
        private static Dictionary<int, NetEntity> NetEntities = new Dictionary<int, NetEntity>();
        
        public static void RegisterNet(Network network)
        {
            Network.Add(network);
        }

        public static void RegisterAttributes(Attributes attributes)
        {
            Attributes[attributes.gameObject.GetInstanceID()] = attributes;
        }

        public static void RegisterNetEntity(NetEntity netEntity)
        {
            NetEntities[netEntity.gameObject.GetInstanceID()] = netEntity;
        }

        public static Network GetNet(Scene scene)
        {
            for (int i = 0; i < Network.Count; i++)
            {
                if (Network[i].gameObject.scene == scene)
                {
                    return Network[i];
                }
            }

            return null;
        }
        public static Attributes GetAttributes(GameObject go)
        {
            return Attributes.ContainsKey(go.GetInstanceID()) ? Attributes[go.GetInstanceID()] : null;
        }
        public static NetEntity GetNetEntity(GameObject go)
        {
            return NetEntities.ContainsKey(go.GetInstanceID()) ? NetEntities[go.GetInstanceID()] : null;
        }

        public void StartServer()
        {
            Game.Network.Mode = Game.Network.NetworkMode.Host;
            SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
        }

        public void StartClient()
        {
            Game.Network.Mode = Game.Network.NetworkMode.Client;
            SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
        }

        public void StartFakeNet()
        {
            Game.Network.Mode = Game.Network.NetworkMode.FakeNet;
            SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
        }
    }
}