using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan;
using XPlan.Utility;

namespace XPlan.DebugMode
{ 
    public class DebugManager : CreateSingleton<DebugManager>
    {
        static private bool bIsInitial = false;

        [SerializeField] 
        private GameObject debugConsole;        
        
        [SerializeField, Range(0.1f, 10f)] 
        private float gameSpeedRatio = 1;

        private float currGameSpeed = 1;

        protected override void InitSingleton()
        {
            gameSpeedRatio  = 1;
            currGameSpeed   = 1;
            bIsInitial      = true;

            if(debugConsole != null)
            {
                // 預設先關閉
                debugConsole.SetActive(false);
            }

#if !BUILD_PRODUCTION
            RegisterLogic(new DebugHandler(debugConsole));
#endif //!BUILD_PRODUCTION
        }
#if UNITY_EDITOR
        protected override void OnPostUpdate(float deltaTime)
		{
			if(gameSpeedRatio != currGameSpeed)
			{
                currGameSpeed   = gameSpeedRatio;
                Time.timeScale  = currGameSpeed;

                LogSystem.Record($"Game Speed Change To {currGameSpeed}");
			}
		}
#endif //UNITY_EDITOR
        static public bool IsInitial()
        {
            return bIsInitial;
        }
    }
}
