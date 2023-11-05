using NetMimic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class Spawner : MonoBehaviour
    {
        public AssetReference m_ToSpawn = null;

        void Update()
        {
            Network net = GameMain.GetNet(gameObject.scene);
            if( net.IsReady() )
            {
                net.InstantiatePrefabFromAddress(m_ToSpawn.AssetGUID, transform.position, transform.rotation, net.CreateNetID(), true);
                gameObject.SetActive(false);
            }
        }
    }
}
