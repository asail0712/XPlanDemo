using UnityEngine;

// 參考資料
// https://easings.net/zh-tw

namespace XPlan.Utility
{
    public static class EasingFunctionsExtensions
    {
        // Linear Easing Function
        public static float Linear(this float t)
        {
            return t;
        }

        // Quadratic Easing Functions
        public static float EaseInQuad(this float t)
        {
            return t * t;
        }

        public static float EaseOutQuad(this float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        public static float EaseInOutQuad(this float t)
        {
            return (t < 0.5f) ? (2f * t * t) : (1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f);
        }

        // Cubic Easing Functions
        public static float EaseInCubic(this float t)
        {
            return t * t * t;
        }

        public static float EaseOutCubic(this float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        public static float EaseInOutCubic(this float t)
        {
            return (t < 0.5f) ? (4f * t * t * t) : (1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f);
        }

        // Other Easing Functions (such as Quartic, Quintic, etc.) can be added similarly.

        // Custom Easing Function
        public static float CustomEasing(this float t)
        {
            // Implement your own custom easing function here
            return Mathf.Sin(t * Mathf.PI * 0.5f);
        }

        // Bounce Easing Functions
        public static float EaseInBounce(this float t)
        {
            return 1f - EaseOutBounce(1f - t);
        }

        public static float EaseOutBounce(this float t)
        {
            if (t < 1f / 2.75f)
            {
                return 7.5625f * t * t;
            }
            else if (t < 2f / 2.75f)
            {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            else if (t < 2.5f / 2.75f)
            {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / 2.75f;
                return 7.5625f * t * t + 0.984375f;
            }
        }

        public static float EaseInOutBounce(this float t)
        {
            return (t < 0.5f) ? (0.5f * EaseInBounce(t * 2f)) : (0.5f * EaseOutBounce(t * 2f - 1f) + 0.5f);
        }

        // Elastic Easing Functions
        public static float EaseInElastic(this float t)
        {
            const float c4 = (2 * Mathf.PI) / 3f;

            if (t <= 0)
                return 0;

            if (t >= 1)
                return 1;

            return -Mathf.Pow(2f, 10 * t - 10) * Mathf.Sin((t * 10f - 10.75f) * c4);
        }

        public static float EaseOutElastic(this float t)
        {
            const float c4 = (2 * Mathf.PI) / 3f;

            if (t <= 0)
                return 0;

            if (t >= 1)
                return 1;

            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }

        public static float EaseInOutElastic(this float t)
        {
            const float c5 = (2 * Mathf.PI) / 4.5f;

            if (t <= 0)
                return 0;

            if (t >= 1)
                return 1;

            return (t < 0.5f)
                ? -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f
                : (Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f + 1f;
        }

        // Back Easing Functions
        public static float EaseInBack(this float t)
        {
            const float s = 1.70158f;

            return t * t * ((s + 1f) * t - s);
        }

        public static float EaseOutBack(this float t)
        {
            const float s = 1.70158f;

            t -= 1f;

            return t * t * ((s + 1f) * t + s) + 1f;
        }

        public static float EaseInOutBack(this float t)
        {
            float c1 = 1.70158f;
            float c2 = c1 * 1.525f;

            t *= 2f;
            if (t < 1f)
            {
                return 0.5f * (t * t * ((c2 + 1f) * t - c2));
            }
            else
            {
                t -= 2f;
                return 0.5f * (t * t * ((c2 + 1f) * t + c2) + 2f);
            }
        }

        // Quart Easing Functions
        public static float EaseInQuart(this float t)
        {
            return t * t * t * t;
        }

        public static float EaseOutQuart(this float t)
        {
            t -= 1f;
            return 1f - (t * t * t * t);
        }

        public static float EaseInOutQuart(this float t)
        {
            t *= 2f;
            if (t < 1f)
            {
                return 0.5f * t * t * t * t;
            }
            else
            {
                t -= 2f;
                return -0.5f * (t * t * t * t - 2f);
            }
        }

        // Expo Easing Functions
        public static float EaseInExpo(this float t)
        {
            return (Mathf.Pow(2f, 10f * (t - 1f)));
        }

        public static float EaseOutExpo(this float t)
        {
            return (-Mathf.Pow(2f, -10f * t) + 1f);
        }

        public static float EaseInOutExpo(this float t)
        {
            t *= 2f;
            if (t < 1f)
            {
                return 0.5f * Mathf.Pow(2f, 10f * (t - 1f));
            }
            else
            {
                t -= 1f;
                return 0.5f * (-Mathf.Pow(2f, -10f * t) + 2f);
            }
        }

        // Quint Easing Functions
        public static float EaseInQuint(this float t)
        {
            return t * t * t * t * t;
        }

        public static float EaseOutQuint(this float t)
        {
            t -= 1f;
            return 1f + (t * t * t * t * t);
        }

        public static float EaseInOutQuint(this float t)
        {
            t *= 2f;
            if (t < 1f)
            {
                return 0.5f * t * t * t * t * t;
            }
            else
            {
                t -= 2f;
                return 0.5f * (t * t * t * t * t + 2f);
            }
        }

        // Circ Easing Functions
        public static float EaseInCirc(this float t)
        {
            return 1f - Mathf.Sqrt(1f - (t * t));
        }

        public static float EaseOutCirc(this float t)
        {
            t -= 1f;
            return Mathf.Sqrt(1f - (t * t));
        }

        public static float EaseInOutCirc(this float t)
        {
            t *= 2f;
            if (t < 1f)
            {
                return -0.5f * (Mathf.Sqrt(1f - (t * t)) - 1f);
            }
            else
            {
                t -= 2f;
                return 0.5f * (Mathf.Sqrt(1f - (t * t)) + 1f);
            }
        }
    }
}