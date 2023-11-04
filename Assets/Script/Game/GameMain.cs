using NetMimic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameMain : MonoBehaviour
    {
        public NetworkManager.NetTransport m_Transport = NetworkManager.NetTransport.UnityNetCode;

        public static List<Network> m_Network = new List<Network>();

        public static void Register(Network network)
        {
            m_Network.Add(network);
        }

        public static Network GetNet(Scene scene)
        {
            for (int i = 0; i < m_Network.Count; i++)
            {
                if (m_Network[i].gameObject.scene == scene)
                {
                    return m_Network[i];
                }
            }

            return null;
        }

        public void StartServer()
        {
            Network.Mode = Network.NetworkMode.Host;
            SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
        }

        public void StartClient()
        {
            Network.Mode = Network.NetworkMode.Client;
            SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
        }

        public void StartFakeNet()
        {
            Network.Mode = Network.NetworkMode.FakeNet;
            SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
        }
    }
}