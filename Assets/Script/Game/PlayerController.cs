using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public class PlayerController : MonoBehaviour
    {
        public float m_Speed = 10.0f;

        public void Start()
        {
            enabled = GetComponent<NetEntity>().Owned;
        }

        public void Update()
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            transform.position += new Vector3(x, 0, y) * m_Speed * Time.deltaTime;
        }
    }
}