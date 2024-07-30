using UnityEngine;

namespace XPlan.Utility
{
    public abstract class CreateSingleton<T> : SystemBase where T : SystemBase
    {
        private static T instance;
        private static T initInstance;

        private static bool isDestroy;

        protected static void InitMember()
        {
            instance        = null;
            initInstance    = null;
            isDestroy       = false;
        }

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (instance == null && Application.isPlaying)
                    {
                        var obj     = new GameObject(typeof(T).Name);
                        instance    = obj.AddComponent<T>();
                    }
                }

                // 初期化
                InitInstance();

                return instance;
            }
        }

        private static void InitInstance()
        {
            if (initInstance == null && instance != null && Application.isPlaying)
            {
                DontDestroyOnLoad(instance.gameObject);

                var s = instance as CreateSingleton<T>;
                s.InitSingleton();

                initInstance = instance;
            }
        }

        public static bool IsInstance()
        {
            return instance != null && isDestroy == false;
        }

        public static GameObject GetGameObject()
		{
            return instance.gameObject;
		}

        protected new void Awake()
        {
            base.Awake();

            if (instance == null)
            {
                instance = this as T;
                InitInstance();
            }
            else if(instance != this)
            {
                var s = instance as CreateSingleton<T>;
                s.DuplicateDetection(this as T);

                Destroy(this.gameObject);
            }
        }

        protected override void OnRelease(bool bAppQuit)
        {
            if (instance == this)
            {
                isDestroy = true;
            }
        }

        protected virtual void DuplicateDetection(T duplicate) 
        { 
            // to override
        }

        protected abstract void InitSingleton();
    }
}
