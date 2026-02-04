using UnityEngine;
using System;

namespace XPlan
{
    public class AppLifecycleWatcher : MonoBehaviour
    {
        public static event Action<bool> OnForegroundChanged; // true=前景, false=背景

        private bool _isForeground = true;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            // 這個只會讓 Editor/桌面環境背景也能跑；手機上不保證網路不被停
            Application.runInBackground = false;
        }

        private void OnApplicationPause(bool pause)
        {
            // pause == true 代表進背景（或被打斷）
            SetForeground(!pause, "OnApplicationPause");
        }

        private void OnApplicationFocus(bool focus)
        {
            // focus == false 可能是跳出、彈窗、切換 app；有時不等於真正背景
            // 但配合 Pause 一起用通常最穩
            SetForeground(focus, "OnApplicationFocus");
        }

        private void SetForeground(bool isForeground, string from)
        {
            if (_isForeground == isForeground) return;

            _isForeground = isForeground;
            Debug.Log($"[Lifecycle] Foreground={_isForeground} (from {from})");
            OnForegroundChanged?.Invoke(_isForeground);
        }
    }
}