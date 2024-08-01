using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Demo.Recycle
{ 
	public class KillZone : MonoBehaviour
	{
		[SerializeField] private BallEmitter ballEmitter;

		private void OnTriggerEnter2D(Collider2D collision)
		{
			GameObject go = collision.gameObject ;
			
			if(go != null)
			{
				Ball ball				= go.GetComponent<Ball>();
				ball.transform.position = ballEmitter.transform.TransformPoint(Random.insideUnitSphere * 0.5f);
				Rigidbody2D rigi		= ball.GetComponent<Rigidbody2D>();
				// 避免速度累積太多
				rigi.velocity			= Vector2.zero;
			}
		}
	}
}
