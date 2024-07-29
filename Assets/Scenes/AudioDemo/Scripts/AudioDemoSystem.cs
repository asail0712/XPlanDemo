using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Audio;

namespace XPlan.Demo.Audio
{ 
    public class AudioDemoSystem : MonoBehaviour
    {
        private List<string> infoList;
        [SerializeField]private Text textInfo;

        void Start()
        {
            textInfo.text   = "";
            infoList        = new List<string>();

            infoList.Add("Play BGSound!");
            AudioSystem.Instance.PlaySound("BGSound");
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyUp(KeyCode.A))
			{
                infoList.Add("Play Sound VFX1!");

                AudioSystem.Instance.PlaySound("VFX1", (clipName) => 
                {
                    infoList.Add("Sound VFX1 finish!");
                });
            }

            if (Input.GetKeyUp(KeyCode.S))
            {
                infoList.Add("Play Sound VFX1 Indepondence!");

                AudioSystem.Instance.PlaySoundIndependently("VFX1", (clipName) =>
                {
                    infoList.Add("Indepondence Sound VFX1 finish!");
                });
            }

            if (Input.GetKeyUp(KeyCode.D))
            {
                infoList.Add("Play Sound VFX2!");
                AudioSystem.Instance.PlaySound("VFX2", (clipName) =>
                {
                    infoList.Add("Sound VFX2 finish!");
                });
            }

            textInfo.text = "";

            while (infoList.Count > 15)
			{
                infoList.RemoveAt(0);
            }

            foreach(string infoStr in infoList)
			{
                textInfo.text += (infoStr + '\n');
            }
        }
    }
}
