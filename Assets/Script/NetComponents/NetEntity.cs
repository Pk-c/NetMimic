using UnityEngine;

namespace Game
{
    public class NetEntity : MonoBehaviour
    {
        public string Address { get; private set; }
        public bool Owned { get; private set; } = false;
        public int ID { get; private set; }
        public static NetEntity Create(GameObject go, string address, int id, bool owned)
        {
            NetEntity netEntity = go.AddComponent<NetEntity>();
            netEntity.Setup(owned, id, address);
            return netEntity;
        }

        private void Setup(bool owned, int id, string adress)
        {
            this.Address = adress;
            this.Owned = owned;
            this.ID = id;
        }
    }
}