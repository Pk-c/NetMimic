using NetMimic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class Spawner : MonoBehaviour
    {
        public static int EntityCount { get; private set; } = 0;
        public AssetReference m_ToSpawn = null;

        void Update()
        {
            Network net = GameMain.GetNet(gameObject.scene);
            if( net.IsReady() )
            {
                EntityCount++;
                NetworkId id = new NetworkId(EntityCount, net.GetClientID());
                net.InstantiatePrefabFromAddress(m_ToSpawn.AssetGUID, transform.position, transform.rotation, id.Value, true);
                gameObject.SetActive(false);
            }
        }
    }
}
