using UnityEngine;

namespace XPlan.Components
{ 
    public class DestroyIfExist : MonoBehaviour
    {
	    void Awake()
	    {
            GameObject[] objects    = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int repeatNum           = 0;

            foreach (GameObject obj in objects)
            {
                if (obj.name == gameObject.name)
                {
                    ++repeatNum;
                }
            }

            if(repeatNum > 1)
		    {
                Debug.Log($"{gameObject.name} need to be Destroy !!");
                DestroyImmediate(gameObject);
		    }
        }
    }
}
