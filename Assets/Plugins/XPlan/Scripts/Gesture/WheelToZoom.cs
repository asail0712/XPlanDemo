using UnityEngine;

namespace XPlan.Gesture
{
    public class WheelToZoom : MonoBehaviour
    {
        [SerializeField] private float scaleSpeed   = 1.0f; // �����Y��t�ת��ܼ�
        [SerializeField] public float minScale      = 0.5f; // �̤p�Y�񭭨�
        [SerializeField] public float maxScale      = 3.0f; // �̤j�Y�񭭨�

        void Update()
        {
            // ���o�ƹ��u������J��
            float scroll            = Input.GetAxis("Mouse ScrollWheel");

            // �p��s���Y���
            float newScale          = transform.localScale.x + scroll * scaleSpeed;

            // �����Y��Ȧb�̤p�M�̤j�d��
            newScale                = Mathf.Clamp(newScale, minScale, maxScale);

            // �]�w���󪺷s�Y���
            transform.localScale    = new Vector3(newScale, newScale, newScale);
        }
    }
}
