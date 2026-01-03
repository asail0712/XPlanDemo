// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================
using UnityEngine;
using UnityEngine.UI;

namespace XPlan.Components
{
    [RequireComponent(typeof(Text))]
    [ExecuteAlways]
    public class AutoResizeByText : MonoBehaviour
    {
        private Text text;
        private RectTransform rectTransform;

        void Awake()
        {
            text            = GetComponent<Text>();
            rectTransform   = GetComponent<RectTransform>();
        }

        void Update()
        {
            if (text == null || rectTransform == null)
            {
                return;
            }

            float fixedWidth                = rectTransform.rect.width;
            TextGenerationSettings settings = text.GetGenerationSettings(new Vector2(fixedWidth, float.PositiveInfinity));
            float height                    = text.cachedTextGeneratorForLayout.GetPreferredHeight(text.text, settings) / text.pixelsPerUnit;

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}