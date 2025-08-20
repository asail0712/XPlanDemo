using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XPlan.UI
{
    public class TabNavigation : MonoBehaviour
    {
        [SerializeField] private Selectable[] selectableList; // 依順序指定

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                GameObject current = EventSystem.current.currentSelectedGameObject;

                for (int i = 0; i < selectableList.Length; i++)
                {
                    if (current == selectableList[i].gameObject)
                    {
                        int nextIndex = (i + 1) % selectableList.Length; // 循環
                        selectableList[nextIndex].Select();
                        break;
                    }
                }
            }
        }
    }
}
