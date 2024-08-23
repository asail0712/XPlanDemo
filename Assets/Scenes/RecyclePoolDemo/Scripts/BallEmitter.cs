using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Recycle;

namespace XPlan.Demo.Recycle
{
    public class BallEmitter : MonoBehaviour
    {
        [SerializeField] private GameObject ballPerfab;
        [SerializeField] private int defaultParticles   = 5;
        [SerializeField] private Vector2 sizeRange;

        // Start is called before the first frame update
        void Start()
        {
            InitialPool();
            SpawnBall();
        }

        private void InitialPool()
        {
            RecyclePool<Ball>.RegisterType(ballPerfab, defaultParticles);
        }

        private void SpawnBall()
        {
            Ball ball                   = RecyclePool<Ball>.SpawnOne();
            ball.transform.position     = transform.TransformPoint(Random.insideUnitSphere * 0.5f);
            ball.transform.localScale   = Random.Range(sizeRange.x, sizeRange.y) * Vector3.one;
        }

        public int GetNumInCamera()
        {
            return RecyclePool<Ball>.GetTotalNum() - RecyclePool<Ball>.GetPoolNum();
        }

		private void Update()
		{
            if (Input.GetKeyDown(KeyCode.A))
            {
                SpawnBall();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                Ball ball = GameObject.FindObjectOfType<Ball>();
                RecyclePool<Ball>.Recycle(ball);
            }
        }
	}
}