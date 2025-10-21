using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Audio;

namespace XPlan.Demo.Audio
{ 
    public class MicrophoneDemo : MonoBehaviour
    {
        [SerializeField] AudioSource playerSource; 

        // Update is called once per frame
        private void Update()
        {
            if(Input.GetKeyUp(KeyCode.A))
			{
                MicrophoneTools.StartRecording(0, (clip) =>
                {
                    playerSource.clip = clip;
                    playerSource.Play();
                });
            }
            else if (Input.GetKeyUp(KeyCode.D))
            {
                MicrophoneTools.EndRecording();
            }
        }
    }
}
