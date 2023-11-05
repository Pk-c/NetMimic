using UnityEditorInternal;
using UnityEngine;

namespace Game
{
    public class PlayerController : MonoBehaviour
    {
        public float MovementSpeed = 10.0f;
        public float TurnSpeed = 10.0f;
        private Animator Controller = null;

        public void Start()
        {
            Controller = GetComponent<Animator>();
        }

        public void Update()
        {
            NetEntity net = GameMain.GetNetEntity(gameObject);

            if (net == null)
                return;

            float Speed = (float)GameMain.GetAttributes(gameObject).Get("speed").GetValue();

            if (net.Owned)
            {
                float x = Input.GetAxis("Horizontal");
                float y = Input.GetAxis("Vertical");

                Vector3 moveDirection = new Vector3(x, 0, y);
                Speed = moveDirection.magnitude;

                if (Speed > 0.0f)
                {
                    transform.position += moveDirection * MovementSpeed * Time.deltaTime;
                    Quaternion newRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * TurnSpeed);
                }

                GameMain.GetAttributes(gameObject).Get("speed").SetValue(Speed);
            }

            Controller.SetFloat("speed", Speed);
        }
    }
}