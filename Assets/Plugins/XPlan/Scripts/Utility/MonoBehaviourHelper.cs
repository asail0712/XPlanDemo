using System.Collections;
using System;
using UnityEngine;

namespace XPlan.Utility
{
    public static class MonoBehaviourHelper
    {
        public static MonoBehavourInstance StartCoroutine(IEnumerator routine, bool persistent = false)
        {
            MonoBehavourInstance MonoHelper = new GameObject("Coroutine => " + routine.ToString()).AddComponent<MonoBehavourInstance>();

            MonoHelper.DestroyWhenComplete(routine, persistent);

            return MonoHelper;
        }

        public static MonoBehavourInstance StartCoroutine(Func<IEnumerator> Func, bool persistent = false)
        {
            if(Func == null)
			{
                Debug.LogError("func not be null !");
			}

            string funcName                 = Func.Method.Name;
            IEnumerator routine             = Func?.Invoke();
            MonoBehavourInstance MonoHelper = new GameObject("Coroutine- " + funcName).AddComponent<MonoBehavourInstance>();

            MonoHelper.DestroyWhenComplete(routine, persistent);

            return MonoHelper;
        }

        public class MonoBehavourInstance : MonoBehaviour
        {
            public IEnumerator routine;

            public Coroutine DestroyWhenComplete(IEnumerator routine, bool bPersistent)
            {
                if (bPersistent)
                { 
                    DontDestroyOnLoad(this.gameObject);
                }

                this.routine = routine;

                return StartCoroutine(WaitToDestroy(routine));
            }

            public void StartCoroutine()
            {
                if(this != null) // 避免在結束時執行到
				{
                    StartCoroutine(routine);
                }
            }

            public void StopCoroutine(bool bNeedToDestroy = true)
            {
                StopCoroutine(routine);

                if(bNeedToDestroy)
				{
                    Destroy(this.gameObject);
                }                
            }

            private IEnumerator WaitToDestroy(IEnumerator routine)
            {
                yield return StartCoroutine(routine);

                Destroy(this.gameObject);
            }
        }
    }
}