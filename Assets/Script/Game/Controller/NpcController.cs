using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class NpcController : MonoBehaviour
    {
        public float m_MinDanceTime = 2.0f;
        public float m_MaxDanceTime = 4.0f;

        private float m_DanceTime = 0.0f;
        private Animator m_Animator;

        private Coroutine speedAdjustmentCoroutine;


        void Start()
        {
            m_Animator = GetComponent<Animator>();
        }

        void Update()
        {
            NetEntity net = GameMain.GetNetEntity(gameObject);

            if (net == null)
                return;


            if (net.Owned)
            {
                if (m_DanceTime <= 0.0f)
                {
                    int dance = m_Animator.GetInteger("dance");
                    dance = dance == 0 ? 1 : 0;
                    m_Animator.SetInteger("dance", dance);
                    m_DanceTime = Random.Range(m_MinDanceTime, m_MaxDanceTime);
                }
                else
                {
                    m_DanceTime -= Time.deltaTime;
                }

                GameMain.GetAttributes(gameObject).Get("animstate").SetValue(m_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash);
                GameMain.GetAttributes(gameObject).Get("animtime").SetValue(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            }
            else
            {
                int statename = GameMain.GetAttributes(gameObject).Get("animstate").intValue;
                float animtime = GameMain.GetAttributes(gameObject).Get("animtime").floatValue;

                if ( m_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash != statename)
                {
                    //fadeto
                    m_Animator.CrossFade(statename, 0.05f, 0, animtime);
                }
                else if( Mathf.Abs(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime - animtime) > 0.3f )
                {
                    m_Animator.Play(statename, 0, animtime);
                }
            }
        }

    }
}
