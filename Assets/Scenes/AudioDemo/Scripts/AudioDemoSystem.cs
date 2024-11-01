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
                AudioChannel channel = AudioSystem.Instance.GetChannelByClipName("VFX1");

                infoList.Add($"Play Sound VFX1 in {channel.ToString()}!");

                AudioSystem.Instance.PlaySound("VFX1", 1f, 0f, (clipName) => 
                {
                    infoList.Add("Sound VFX1 finish!");
                });
            }

            if (Input.GetKeyUp(KeyCode.S))
            {
                infoList.Add("Play Sound VFX1 Without Channel!");

                AudioSystem.Instance.PlayWithoutChannel("VFX1", 1f, 0f, (clipName) =>
                {
                    infoList.Add("Sound VFX1 finish!");
                });
            }

            if (Input.GetKeyUp(KeyCode.D))
            {
                AudioChannel channel = AudioSystem.Instance.GetChannelByClipName("VFX2");

                infoList.Add($"Play Sound VFX2 in {channel.ToString()}!");

                AudioSystem.Instance.PlaySound("VFX2", 1f, 0f, (clipName) =>
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
