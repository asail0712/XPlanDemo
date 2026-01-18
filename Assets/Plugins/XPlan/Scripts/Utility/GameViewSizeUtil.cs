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
#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XPlan.Utility
{
    public static class GameViewSizeForce
    {
        /// <summary>
        /// 確保存在指定 FixedResolution，並切換到它（只影響 Editor GameView）。
        /// </summary>
        public static void EnsureAndUseFixed(string baseText, int width, int height)
        {
            var sizesInstance = GetGameViewSizesInstance();
            var group = GetCurrentGroup_Compat(sizesInstance);

            // 1) 找 index（不存在就新增）
            int idx = FindFixedResolutionIndex(group, width, height);
            if (idx < 0)
            {
                AddCustomFixedResolution(group, baseText, width, height);
                idx = FindFixedResolutionIndex(group, width, height);
            }

            if (idx < 0)
            {
                Debug.LogError($"[GameView] 找不到/新增失敗: {baseText} ({width}x{height})");
                return;
            }

            // 2) 切換
            SetGameViewSelectedSizeIndex(idx);
        }

        // -------------------------
        // Find (使用 GetTotalCount + GetGameViewSize)
        // -------------------------
        static int FindFixedResolutionIndex(object group, int width, int height)
        {
            var groupType = group.GetType();

            var mGetTotalCount = groupType.GetMethod("GetTotalCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var mGetGameViewSize = groupType.GetMethod("GetGameViewSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (mGetTotalCount == null || mGetGameViewSize == null)
                throw new MissingMemberException("[GameView] GameViewSizeGroup 缺少 GetTotalCount / GetGameViewSize");

            int total = (int)mGetTotalCount.Invoke(group, null);

            var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            var enumType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
            var fixedResValue = Enum.Parse(enumType, "FixedResolution");

            var pSizeType = gvsType.GetProperty("sizeType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var pW = gvsType.GetProperty("width", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var pH = gvsType.GetProperty("height", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < total; i++)
            {
                var s = mGetGameViewSize.Invoke(group, new object[] { i });
                var st = pSizeType.GetValue(s);

                if (!st.Equals(fixedResValue))
                    continue;

                int w = (int)pW.GetValue(s);
                int h = (int)pH.GetValue(s);

                if (w == width && h == height)
                    return i;
            }

            return -1;
        }

        // -------------------------
        // Add (呼叫 group.AddCustomSize)
        // -------------------------
        static void AddCustomFixedResolution(object group, string baseText, int width, int height)
        {
            var enumType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
            var fixedResValue = Enum.Parse(enumType, "FixedResolution");

            var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            var ctor = gvsType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[] { enumType, typeof(int), typeof(int), typeof(string) },
                modifiers: null
            );
            if (ctor == null)
                throw new MissingMemberException("[GameView] 找不到 GameViewSize ctor(type,width,height,baseText)");

            var newSize = ctor.Invoke(new object[] { fixedResValue, width, height, baseText });

            var mAddCustomSize = group.GetType().GetMethod("AddCustomSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mAddCustomSize == null)
                throw new MissingMemberException("[GameView] 找不到 GameViewSizeGroup.AddCustomSize(GameViewSize)");

            mAddCustomSize.Invoke(group, new object[] { newSize });
        }

        // -------------------------
        // Select
        // -------------------------
        static void SetGameViewSelectedSizeIndex(int index)
        {
            var gvType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            var gvWnd = EditorWindow.GetWindow(gvType);

            var p = gvType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p == null)
                throw new MissingMemberException("[GameView] 找不到 GameView.selectedSizeIndex");

            p.SetValue(gvWnd, index);
            gvWnd.Repaint();
        }

        // -------------------------
        // 取得 current group（不靠 GetCurrentGroupType）
        // -------------------------
        static object GetCurrentGroup_Compat(object sizesInstance)
        {
            var sizesType = sizesInstance.GetType();

            // A) sizesInstance.GetCurrentGroup()
            var mGetCurrentGroup = sizesType.GetMethod("GetCurrentGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mGetCurrentGroup != null)
                return mGetCurrentGroup.Invoke(sizesInstance, null);

            // B) sizesInstance.currentGroupType + GetGroup(int)
            int groupType = TryGetIntMember(sizesInstance, sizesType,
                "currentGroupType", "m_CurrentGroupType", "m_GameViewSizeGroupType");

            if (groupType < 0)
            {
                // C) 從 GameView window 抓 groupType（欄位名各版本不同）
                groupType = TryGetGroupTypeFromGameView();
            }

            if (groupType < 0)
                throw new MissingMemberException("[GameView] 無法取得 current groupType（請用 DumpGameViewMembers 找實際名字）");

            var mGetGroup = sizesType.GetMethod("GetGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mGetGroup == null)
                throw new MissingMemberException("[GameView] 找不到 GameViewSizes.GetGroup(int)");

            return mGetGroup.Invoke(sizesInstance, new object[] { groupType });
        }

        static int TryGetGroupTypeFromGameView()
        {
            var gvType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            var gvWnd = EditorWindow.GetWindow(gvType);

            // 這些名字是常見候選 可以用下面 Dump 方法印出來確認
            string[] names =
            {
            "currentSizeGroupType",
            "selectedSizeGroupType",
            "m_CurrentSizeGroupType",
            "m_SelectedSizeGroupType",
            "m_SizeSelectionGroupType",
        };

            foreach (var n in names)
            {
                var p = gvType.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null) return Convert.ToInt32(p.GetValue(gvWnd));

                var f = gvType.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null) return Convert.ToInt32(f.GetValue(gvWnd));
            }

            return -1;
        }

        static int TryGetIntMember(object obj, Type t, params string[] names)
        {
            foreach (var n in names)
            {
                var p = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null) return Convert.ToInt32(p.GetValue(obj));

                var f = t.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null) return Convert.ToInt32(f.GetValue(obj));
            }
            return -1;
        }

        static object GetGameViewSizesInstance()
        {
            var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            var singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);

            var p = singletonType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
            if (p == null)
                throw new MissingMemberException("[GameView] 找不到 ScriptableSingleton<GameViewSizes>.instance");

            return p.GetValue(null);
        }
    }
}
#endif
