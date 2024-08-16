using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.DebugMode
{
    public static class DebugDraw
    {
        static public void DrawSphere(Color color, Vector3 center, float radius, int segments = 12, float duration = 3f)
        {
            // 绘制垂直方向的圆圈
            for (int i = 0; i <= segments; i++)
            {
                float angle1 = (i * Mathf.PI * 2) / segments;
                float angle2 = ((i + 1) * Mathf.PI * 2) / segments;

                Vector3 point1 = new Vector3(center.x + Mathf.Cos(angle1) * radius, center.y, center.z + Mathf.Sin(angle1) * radius);
                Vector3 point2 = new Vector3(center.x + Mathf.Cos(angle2) * radius, center.y, center.z + Mathf.Sin(angle2) * radius);

                Debug.DrawLine(point1, point2, color, duration);
            }

            // 绘制水平方向的圆圈
            for (int i = 0; i <= segments; i++)
            {
                float angle1 = (i * Mathf.PI * 2) / segments;
                float angle2 = ((i + 1) * Mathf.PI * 2) / segments;

                Vector3 point1 = new Vector3(center.x, center.y + Mathf.Cos(angle1) * radius, center.z + Mathf.Sin(angle1) * radius);
                Vector3 point2 = new Vector3(center.x, center.y + Mathf.Cos(angle2) * radius, center.z + Mathf.Sin(angle2) * radius);

                Debug.DrawLine(point1, point2, color, duration);
            }

            // 绘制另一个垂直方向的圆圈
            for (int i = 0; i <= segments; i++)
            {
                float angle1 = (i * Mathf.PI * 2) / segments;
                float angle2 = ((i + 1) * Mathf.PI * 2) / segments;

                Vector3 point1 = new Vector3(center.x + Mathf.Cos(angle1) * radius, center.y + Mathf.Sin(angle1) * radius, center.z);
                Vector3 point2 = new Vector3(center.x + Mathf.Cos(angle2) * radius, center.y + Mathf.Sin(angle2) * radius, center.z);

                Debug.DrawLine(point1, point2, color, duration);
            }
        }
    }
}
