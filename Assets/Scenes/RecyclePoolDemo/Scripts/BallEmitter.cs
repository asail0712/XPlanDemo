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

        private List<Ball> ballList;

        // Start is called before the first frame update
        void Start()
        {
            InitialPool();
            SpawnBall();
        }

        private void InitialPool()
        {
            ballList = new List<Ball>();

            for (int i = 0; i < defaultParticles; ++i)
            {
                GameObject go = Instantiate(ballPerfab);
                ballList.Add(go.GetComponent<Ball>());
                go.SetActive(false);
            }

            RecyclePool<Ball>.RegisterType(ballList);
        }

        private void SpawnBall()
        {
            Ball ball                   = RecyclePool<Ball>.SpawnOne();
            ball.transform.position     = transform.TransformPoint(Random.insideUnitSphere * 0.5f);
            ball.transform.localScale   = Random.Range(sizeRange.x, sizeRange.y) * Vector3.one;
           
            if(!ballList.Contains(ball))
			{
                ballList.Add(ball);
            }
        }

        public int GetNumInCamera()
        {
            int sum = 0;

            for (int i = 0; i < ballList.Count; ++i)
            {
                Renderer renderer = ballList[i].GetComponent<Renderer>();

                // 有在畫面上的
                if (renderer.isVisible && renderer.gameObject.activeSelf)
                {
                    ++sum;
                }
            }

            return sum;
        }

		private void Update()
		{
            if (Input.GetKeyDown(KeyCode.A))
            {
                SpawnBall();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                for(int i = 0; i < ballList.Count; ++i)
				{
                    if(ballList[i].isActiveAndEnabled)
                    { 
                        RecyclePool<Ball>.Recycle(ballList[i]);
                        break;
                    }
                }
            }
        }
	}
}