using UnityEngine;

namespace Game
{
    public class RunInCircle : MonoBehaviour
    {
        public float radius = 5f;
        public float speed = 1f;

        private float angle = 0f;
        private Vector3 position= Vector3.zero;
        public void Start()
        {
            position = transform.position;
        }

        void Update()
        {
            NetEntity net = GameMain.GetNetEntity(gameObject);

            if (net == null)
                return;

            if (net.Owned)
            {
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                transform.position = position + new Vector3(x, 0, z);

                angle += speed * Time.deltaTime;

                Vector3 direction = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                transform.forward = direction;
            }
        }
    }
}
