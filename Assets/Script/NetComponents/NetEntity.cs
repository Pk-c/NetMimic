using NetMimic;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public class NetEntity : MonoBehaviour
    {
        [SerializeField]
        private string Address;
        public string GetAdress { get; private set; }
        public bool Owned { get; private set; } = false;
        public int ID { get; private set; }

        private bool Init = false;
        private Vector3? Position;
        private Quaternion? Rotation;
        private const float Ratio = 6.0f;

        public void Update()
        {
            if (!Init)
            {
                Network net = GameMain.GetNet(gameObject.scene);
                if (net != null && net.IsReady())
                {
                    GameMain.RegisterNetEntity(this);
                    Setup(true, net.CreateNetID(), Address);
                    net.RegisterNetworkObject(this);
                }
            }
            else
            {
                if (!Owned && Position != null)
                {
                    transform.position = Vector3.Lerp(transform.position, Position.Value, Time.deltaTime * Ratio);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Rotation.Value, Time.deltaTime * Ratio);
                }
            }
        }

        public static NetEntity Create(GameObject go, string address, int id, bool owned)
        {
            NetEntity netEntity = go.GetOrAddComponent<NetEntity>();
            netEntity.Setup(owned, id, address);
            GameMain.RegisterNetEntity(netEntity);
            return netEntity;
        }

        private void Setup(bool owned, int id, string adress)
        {
            this.GetAdress = adress;
            this.Owned = owned;
            this.ID = id;
            this.Init = true;
        }

        public void SetPositionAndRotation( Vector3 position, Quaternion quat )
        {
            Position = position;
            Rotation = quat;
        }

#if UNITY_EDITOR
        public bool UpdateAssetId(string assetId) 
        {
            if( !Application.isPlaying)
            {
                if( Address != assetId)
                {
                    Address = assetId;
                    return true;
                }
            }

            return false;
        }
#endif
    }
}