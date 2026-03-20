using UnityEngine;
using System;

namespace XPlan
{
    public class AppLifecycleWatcher : MonoBehaviour
    {
        public static event Action<bool> OnForegroundChanged; // true=前景, false=背景

        [Header("Lifecycle Settings")]
        [SerializeField] private float backgroundDelaySeconds   = 12f;      // 超過幾秒才視為真正進背景
        [SerializeField] private bool allowRunInBackground      = false;    // 是否允許背景仍視為前景

        private bool _isForeground = true;

        // 記錄是否已進入疑似背景狀態
        private bool _pendingBackground;

        // 記錄進背景時間（UTC 避免時區問題）
        private DateTime _backgroundEnterUtc;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = allowRunInBackground;
        }

        private void OnApplicationPause(bool pause)
        {
            HandleStateChange(!pause, "OnApplicationPause");
        }

        private void OnApplicationFocus(bool focus)
        {
            HandleStateChange(focus, "OnApplicationFocus");
        }

        private void HandleStateChange(bool isForeground, string from)
        {
            if (isForeground)
            {
                HandleReturnToForeground(from);
            }
            else
            {
                HandleEnterBackground(from);
            }
        }

        private void HandleEnterBackground(string from)
        {
            if (allowRunInBackground)
            {
                Debug.Log($"[Lifecycle] Background detected but ignored (allowRunInBackground=true, from {from})");
                return;
            }

            // 已經在等待背景中，避免重複記錄
            if (_pendingBackground || !_isForeground)
                return;

            _pendingBackground  = true;
            _backgroundEnterUtc = DateTime.UtcNow;

            Debug.Log($"[Lifecycle] Background pending... start time = {_backgroundEnterUtc:O} (from {from})");
        }

        private void HandleReturnToForeground(string from)
        {
            // 等回到前景後，才透過時間去判斷自己是否有切到背景過
            if (_pendingBackground)
            {
                _pendingBackground          = false;                
                double backgroundSeconds    = (DateTime.UtcNow - _backgroundEnterUtc).TotalSeconds;
                
                Debug.Log($"[Lifecycle] Returned to foreground after {backgroundSeconds:F2}s (from {from})");

                // 只有超過門檻，才視為真的進過背景
                if (backgroundSeconds >= backgroundDelaySeconds)
                {
                    if (_isForeground)
                    {
                        _isForeground = false;
                        Debug.Log($"[Lifecycle] Foreground=false (background lasted {backgroundSeconds:F2}s)");
                        OnForegroundChanged?.Invoke(false);
                    }

                    _isForeground = true;
                    Debug.Log("[Lifecycle] Foreground=true");
                    OnForegroundChanged?.Invoke(true);
                }

                return;
            }

            // 補償情況：如果目前狀態真的不是前景，強制切回前景
            if (!_isForeground)
            {
                _isForeground = true;
                Debug.Log($"[Lifecycle] Foreground=true (recovered from {from})");
                OnForegroundChanged?.Invoke(true);
            }
        }
    }
}