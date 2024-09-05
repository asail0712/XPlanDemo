using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XPlan.Component
{
    public class FaceToCamera : MonoBehaviour
    {
        private void LateUpdate()
        {
            if(Camera.main == null)
			{
                return;
			}

            // 使物體始終面向相機
            transform.LookAt(Camera.main.transform);
            // 使物體保持正面朝向相機
            transform.rotation = Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f);
        }
    }
}