using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Extensions;
using XPlan.Utility;

namespace XPlan.Audio
{
	public enum AudioChannel 
	{ 
		None,
		Channel_1,
		Channel_2,
		Channel_3,
		Channel_4,
		Channel_5,
		Channel_6,
		Channel_7,
		Channel_8,
	}

	[System.Serializable]
	public class SoundInfo
	{
		[SerializeField]
		public string clipName		= "";

		[SerializeField]
		public AudioClip clip		= null;

		[SerializeField]
		public float volume			= 1f;

		[SerializeField]
		public bool bLoop			= true;

		[SerializeField]
		public AudioChannel channel = AudioChannel.Channel_1;

		public SoundInfo()
		{
			volume	= 1f;
			bLoop	= true;
			channel = AudioChannel.Channel_1;
		}
	}

	public class AudioSystem : CreateSingleton<AudioSystem>
	{
		[SerializeField]
		private List<SoundInfo> soundBank;

		private Dictionary<AudioChannel, AudioSource> sourceMap = new Dictionary<AudioChannel, AudioSource>();

		protected override void InitSingleton()
		{
			// 使用到的channel
			List<AudioChannel> channelList = new List<AudioChannel>();

			foreach (SoundInfo info in soundBank)
			{
				if (!channelList.Contains(info.channel))
				{
					channelList.Add(info.channel);
				}
			}

			for (int i = 0; i < channelList.Count; ++i)
			{
				AudioSource source = gameObject.AddComponent<AudioSource>();

				sourceMap.Add(channelList[i], source);
			}
		}

		/************************************
		 * Play Sound
		 * **********************************/
		public void PlaySound(string clipName, float fadeInTime = 1f, float delayTime = 0f)
		{
			int idx = soundBank.FindIndex((E04) =>
			{
				return E04.clipName == clipName;
			});

			PlaySound(idx, fadeInTime, delayTime);
		}

		public void PlaySound(int clipIdx, float fadeInTime = 1f, float delayTime = 0f)
		{
			if(delayTime > 0)
			{
				StartCoroutine(DelayToPlay(clipIdx, fadeInTime, delayTime));
			}
			else
			{
				AudioChannel channel = GetChannelByIdx(clipIdx);

				if (channel == AudioChannel.None)
				{
					return;
				}

				StartCoroutine(FadeInOut(channel, fadeInTime, clipIdx));
			}
		}

		private IEnumerator DelayToPlay(int clipIdx, float fadeInTime, float delayTime)
		{
			yield return new WaitForSeconds(delayTime);

			AudioChannel channel = GetChannelByIdx(clipIdx);

			if (channel == AudioChannel.None)
			{
				yield break;
			}

			yield return FadeInOut(channel, fadeInTime, clipIdx);
		}

		/************************************
		 * Stop Sound
		 * **********************************/
		public void StopSound(string clipName, float fadeOutTime = 1f)
		{
			int idx = soundBank.FindIndex((E04) =>
			{
				return E04.clipName == clipName;
			});

			StopSound(idx, fadeOutTime);
		}

		public void StopSound(int clipIdx, float fadeOutTime = 1f)
		{
			AudioChannel channel = GetChannelByIdx(clipIdx);

			if(channel == AudioChannel.None)
			{
				return;
			}

			StartCoroutine(FadeInOut(channel, fadeOutTime));
		}

		/************************************
		 * Pause Sound
		 * **********************************/
		public void PauseSound(string clipName)
		{
			int idx = soundBank.FindIndex((E04) =>
			{
				return E04.clipName == clipName;
			});

			PauseSound(idx);
		}

		public void PauseSound(int clipIdx)
		{
			AudioSource audioSource = GetSourceByClipIndex(clipIdx);

			if(audioSource == null)
			{
				return;
			}

			audioSource.Pause();
		}
		/************************************
		 * Resume Sound
		 * **********************************/
		public void ResumeSound(string clipName)
		{
			int idx = soundBank.FindIndex((E04) =>
			{
				return E04.clipName == clipName;
			});

			ResumeSound(idx);
		}

		public void ResumeSound(int clipIdx)
		{
			AudioSource audioSource = GetSourceByClipIndex(clipIdx);

			if (audioSource == null)
			{
				return;
			}

			audioSource.UnPause();
		}

		/************************************
		 * is playing
		 * **********************************/
		public bool IsPlaying(string clipName)
		{
			int idx = soundBank.FindIndex((E04) =>
			{
				return E04.clipName == clipName;
			});

			return IsPlaying(idx);
		}

		public bool IsPlaying(int clipIdx)
		{
			AudioSource audioSource = GetSourceByClipIndex(clipIdx);

			if(audioSource == null)
			{
				// audioSource不存在
				return false;
			}

			if(audioSource.clip != GetClipByIdx(clipIdx))
			{
				// 當前 clip不為指定的 clip
				return false;
			}

			// 判斷audioSource是否有play
			return audioSource.isPlaying;
		}

		/************************************
		 * Other
		 * **********************************/

		private AudioSource GetSourceByClipIndex(int clipIdx)
		{
			if (!soundBank.IsValidIndex<SoundInfo>(clipIdx))
			{
				Debug.LogError($"soundBank沒有這個Idx {clipIdx}");

				return null;
			}

			AudioChannel channel = soundBank[clipIdx].channel;

			return GetSourceByChannel(channel);
		}

		private AudioSource GetSourceByChannel(AudioChannel channel)
		{
			if (!sourceMap.ContainsKey(channel))
			{
				Debug.LogError($"使用不存在的 {channel}");

				return null;
			}

			AudioSource audioSource = sourceMap[channel];

			return audioSource;
		}

		private AudioClip GetClipByIdx(int clipIdx)
		{
			if (!soundBank.IsValidIndex<SoundInfo>(clipIdx))
			{
				Debug.LogError($"soundBank沒有這個Idx {clipIdx}");

				return null;
			}

			AudioClip clip = soundBank[clipIdx].clip;

			return clip;
		}

		private AudioChannel GetChannelByIdx(int clipIdx)
		{
			if (!soundBank.IsValidIndex<SoundInfo>(clipIdx))
			{
				Debug.LogError($"soundBank沒有這個Idx {clipIdx}");

				return AudioChannel.None;
			}

			AudioChannel channel = soundBank[clipIdx].channel;

			return channel;
		}

		private IEnumerator FadeInOut(AudioChannel channel, float fadeTime = 1f, int clipIdx = -1)
		{
			// 這是在同一個Channel做 Fade in / out的處理

			AudioSource audioSource = GetSourceByChannel(channel);

			if(audioSource == null)
			{
				yield break;
			}

			// fade out
			if (fadeTime > 0f && audioSource.isPlaying)
			{
				yield return FadeOutCoroutine(audioSource, fadeTime);
			}
			else
			{
				// 如果不需要淡出，则直接停止播放
				audioSource.Stop();
			}

			// 检查是否指定了新的音频剪辑
			if (clipIdx == -1)
			{
				yield break;
			}

			audioSource.clip = GetClipByIdx(clipIdx);

			// 检查是否要淡入
			if (fadeTime > 0f)
			{
				yield return FadeInCoroutine(audioSource, fadeTime);
			}
			else
			{
				// 如果不需要淡入，则直接播放
				audioSource.Play();
			}
		}


		private IEnumerator FadeOutCoroutine(AudioSource audioSource, float fadeOutTime)
		{
			float startVolume	= audioSource.volume;
			float startTime		= Time.time;

			while (Time.time < startTime + fadeOutTime)
			{
				audioSource.volume = Mathf.Lerp(startVolume, 0f, (Time.time - startTime) / fadeOutTime);
				yield return null;
			}

			audioSource.volume = 0f;
			audioSource.Stop();
		}

		private IEnumerator FadeInCoroutine(AudioSource audioSource, float fadeInTime)
		{
			audioSource.volume = 0f;
			audioSource.Play();

			float targetVolume	= 1f;
			float startTime		= Time.time;

			while (Time.time < startTime + fadeInTime)
			{
				audioSource.volume = Mathf.Lerp(0f, targetVolume, (Time.time - startTime) / fadeInTime);
				yield return null;
			}

			audioSource.volume = targetVolume;
		}
	}
}
