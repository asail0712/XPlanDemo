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
using System;
using System.Collections;

namespace XPlan
{
    public class AppLifecycleWatcher : MonoBehaviour
    {
        public static event Action<bool> OnForegroundChanged; // true=前景, false=背景

        [Header("Lifecycle Settings")]
        [SerializeField] private float backgroundDelaySeconds   = 12f;      // 延遲幾秒才真正視為背景
        [SerializeField] private bool allowRunInBackground      = false;    // 是否允許背景仍視為前景

        private bool _isForeground = true;
        private Coroutine _backgroundDelayRoutine;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            // 這個只會讓 Editor/桌面環境背景也能跑；手機上不保證網路不被停
            Application.runInBackground = allowRunInBackground;
        }

        private void OnApplicationPause(bool pause)
        {
            // pause == true 代表進背景（或被打斷）
            HandleStateChange(!pause, "OnApplicationPause");
        }

        private void OnApplicationFocus(bool focus)
        {
            // focus == false 可能是跳出、彈窗、切換 app；有時不等於真正背景
            // 但配合 Pause 一起用通常最穩
            HandleStateChange(focus, "OnApplicationFocus");
        }

        private void HandleStateChange(bool isForeground, string from)
        {
            if (isForeground)
            {
                // 回前景 → 取消延遲背景處理
                CancelBackgroundDelay();

                if (!_isForeground)
                {
                    _isForeground = true;
                    Debug.Log($"[Lifecycle] Foreground=true (from {from})");
                    OnForegroundChanged?.Invoke(true);
                }
            }
            else
            {
                // 進背景
                if (allowRunInBackground)
                {
                    Debug.Log($"[Lifecycle] Background detected but ignored (allowRunInBackground=true)");
                    return;
                }

                // 已經在背景或正在等待，不重複啟動
                if (!_isForeground || _backgroundDelayRoutine != null)
                    return;

                _backgroundDelayRoutine = StartCoroutine(DelayedBackground(from));
            }
        }

        private IEnumerator DelayedBackground(string from)
        {
            Debug.Log($"[Lifecycle] Background detected, delaying {backgroundDelaySeconds}s...");

            yield return new WaitForSecondsRealtime(backgroundDelaySeconds);

            _backgroundDelayRoutine = null;

            if (_isForeground)
            {
                _isForeground = false;
                Debug.Log($"[Lifecycle] Foreground=false (from {from})");
                OnForegroundChanged?.Invoke(false);
            }
        }

        private void CancelBackgroundDelay()
        {
            if (_backgroundDelayRoutine != null)
            {
                StopCoroutine(_backgroundDelayRoutine);
                _backgroundDelayRoutine = null;
                Debug.Log("[Lifecycle] Background delay cancelled");
            }
        }
    }
}