using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XPlan.Component
{
	public class RotateByMouse : MonoBehaviour
	{
		[SerializeField]
		private float moveVel = 5;

		protected Vector2 lastPosition;

		void Update ()
		{
			if(EventSystem.current == null)
			{
				return;
			}

			if (EventSystem.current.IsPointerOverGameObject () || IsPointerOverUIObject())
			{ 
				return;
			}

			#if !UNITY_WEBGL
			if (Input.touchSupported)
			{
				if (Input.touches.Length != 1)
				{ 
					return;
				}

				Touch t = Input.touches[0];
				if (t.phase == TouchPhase.Moved)
				{
					Vector2 delta = t.position - lastPosition;
					transform.Rotate(Vector3.up, (moveVel / 10f) * -delta.x);
				}
				lastPosition = t.position;
			}
			else
			#endif
			{
				if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
				{
					var dx = Input.GetAxis("Mouse X");
					transform.Rotate(Vector3.up, -dx * moveVel);
				}
			}
		}

		protected bool IsPointerOverUIObject()
		{
			PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
			{
				position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
			};
			List<RaycastResult> results = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
			return results.Count > 0;
		}
	}
}