using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Component
{
    public class RotateEndless : MonoBehaviour
    {
		[SerializeField]
		private Vector3 axis = Vector3.up;

		[SerializeField]
		private float rotationSpeed = 50.0f;

		void Update()
		{
			transform.Rotate(axis, Time.deltaTime * rotationSpeed);
		}
	}
}
