using UnityEngine;

namespace XPlan.Component
{ 
    public class DestroyIfExist : MonoBehaviour
    {
	    void Awake()
	    {
            GameObject[] objects    = GameObject.FindObjectsOfType<GameObject>();
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