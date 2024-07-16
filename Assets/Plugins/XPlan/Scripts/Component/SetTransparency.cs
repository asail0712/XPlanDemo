using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Component
{
    public class SetTransparency : MonoBehaviour
    {
        [SerializeField]
        private Renderer objectRenderer; // 要设置透明度的物件的Renderer组件

        [SerializeField, Range(0, 1)]
        private float transparency = 0.25f; // 透明度值，范围在0到1之间

        void Start()
        {
            if(objectRenderer == null)
			{
                objectRenderer = gameObject.GetComponent<Renderer>();
            }

            // 获取物件的材质
            Material material = objectRenderer.material;

            if(material == null)
			{
                return;
			}

            // 设置材质的渲染模式为Transparent
            material.SetFloat("_Mode", 3);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;

            // 设置材质颜色的Alpha通道
            Color color     = material.color;
            color.a         = transparency;
            material.color  = color;
        }

        // 更新透明度
        public void UpdateTransparency(float newTransparency)
        {
            transparency = Mathf.Clamp01(newTransparency); // 确保透明度在0到1之间

            // 获取物件的材质
            Material material = objectRenderer.material;

            // 设置材质颜色的Alpha通道
            Color color     = material.color;
            color.a         = transparency;
            material.color  = color;
        }
    }
}