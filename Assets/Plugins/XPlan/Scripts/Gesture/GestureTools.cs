using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace XPlan.Gesture
{
    public enum MouseTrigger
    {
        LeftMouse,
        MiddleMouse,
        RightMouse,
    }

    public enum InputFingerMode
    {
        OneFinger,
        TwoFingers
    }

    public static class GestureTools
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        static public int MouseKey(MouseTrigger mouseTrigger)
        {
            switch (mouseTrigger)
            {
                case MouseTrigger.LeftMouse:
                    return 0;
                case MouseTrigger.MiddleMouse:
                    return 2;
                case MouseTrigger.RightMouse:
                    return 1;
            }
            return 0;
        }
#endif //UNITY_EDITOR

        static public bool IsPointerOverUI()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return EventSystem.current.IsPointerOverGameObject();
#else
            if (Input.touchCount > 0)
            {
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }
            else
            {
                return false;
            }
#endif
        }
    }
}
