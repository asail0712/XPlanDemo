using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Recycle;
using XPlan.Utility;

namespace XPlan.Audio
{
	public enum AudioChannel 
	{ 
		None,
		Channel_Background,
		Channel_1,
		Channel_2,
		Channel_3,
		Channel_4,
		Channel_5,
		Channel_6,
		Channel_7,
	}

	[System.Serializable]
	public class SoundGroup
	{
		// 提供使用者清晰的名稱，不提供邏輯處理
		[SerializeField]
		public string groupName = "";

		[SerializeField]
		public List<SoundInfo> infoList;
	}


	[System.Serializable]
	public class SoundInfo
	{
		[SerializeField]
		public string clipName			= "";

		[SerializeField]
		public List<AudioClip> clipList	= null;

		[SerializeField]
		public float volume				= 1f;

		[SerializeField]
		public bool bLoop				= true;

		[SerializeField]
		public AudioChannel channel		= AudioChannel.Channel_1;

		public SoundInfo()
		{
			volume	= 1f;
			bLoop	= true;
			channel = AudioChannel.Channel_1;
		}
	}

	public class AudioSystem : CreateSingleton<AudioSystem>
	{
		[Tooltip("放置所有要撥放的聲音")]
		[SerializeField] public List<SoundGroup> soundGroup;

		[Tooltip("背景音樂降低音量時的大小")]
		[SerializeField] private float lowerBGVolume = 0.3f;

		[Tooltip("無channel的AudioSource池子大小")]
		[SerializeField] private int sizeOfWithoutChannelPool = 5;

		private Dictionary<AudioChannel, XAudioSource> sourceMap = new Dictionary<AudioChannel, XAudioSource>();
		private List<SoundInfo> soundBank;

		protected override void InitSingleton()
		{
			if(soundGroup == null)
			{
				return;
			}

			// 初始化soundBank
			RefreshBank();

			// 記錄所有使用到的channel
			List<AudioChannel> channelList = new List<AudioChannel>();

			foreach (SoundInfo info in soundBank)
			{
				if (!channelList.Contains(info.channel))
				{
					channelList.Add(info.channel);
				}
			}

			// 依照Channel的數量建立對應數量的AudioSource
			for (int i = 0; i < channelList.Count; ++i)
			{
				XAudioSource audioSource = new XAudioSource();
				audioSource.InitialSource();

				sourceMap.Add(channelList[i], audioSource);
			}

			// 建立 Without Channel Pool
			List<XAudioSource> audioList = new List<XAudioSource>();

			for(int i = 0; i < sizeOfWithoutChannelPool; ++i)
			{
				XAudioSource audioSource = new XAudioSource();
				audioSource.InitialSource();

				audioList.Add(audioSource);
			}

			RecyclePool<XAudioSource>.RegisterType(audioList);
		}

		private void Update()
		{
			// 背景音樂播放時，有其他音效的話要降低背景音樂的聲音大小
			if(IsPlayingSoundOnBGChannel())
			{
				XAudioSource bgAudioSource	= GetBGAudioSource();
				float defaultVolume			= bgAudioSource.soundInfo.volume;
				float currVolume			= bgAudioSource.volume;
				float targetVolume			= 0f;

				if (GetNumOfPlayingSource() > 1)
				{
					targetVolume = lowerBGVolume;
				}
				else
				{
					targetVolume = defaultVolume;
				}

				bgAudioSource.volume = Mathf.Lerp(currVolume, targetVolume, Time.deltaTime);
			}
		}

		/************************************
		 * Play Sound
		 * 播放聲音可以透過clip name或是 clip index
		 * **********************************/
		public void PlayWithoutChannel(string clipName, float fadeInTime = 1f, float delayTime = 0f, Action<string> finishAction = null)
		{
			int clipIdx = soundBank.FindIndex((E04) =>
			{
				return E04.clipName == clipName;
			});

			SoundInfo info = GetSoundByIdx(clipIdx);

			if (info == null)
			{
				return;
			}

			XAudioSource audioSource	= RecyclePool<XAudioSource>.SpawnOne();
			audioSource.soundInfo		= info;

			StartCoroutine(DelayToPlay(clipIdx, audioSource, fadeInTime, delayTime, (clipName) =>
			{
				finishAction?.Invoke(clipName);
				RecyclePool<XAudioSource>.Recycle(audioSource);
			}));
		}

		public void PlaySound(string clipName, float fadeInTime = 1f, float delayTime = 0f, Action<string> finishAction = null)
		{
			int clipIdx = soundBank.FindIndex((E04) =>
			{
				return E04.clipName == clipName;
			});

			PlaySound(clipIdx, fadeInTime, delayTime, finishAction);
		}

		public void PlaySound(int clipIdx, float fadeInTime = 1f, float delayTime = 0f, Action<string> finishAction = null)
		{
			XAudioSource audioSource = GetSourceByClipIndex(clipIdx);

			if (audioSource == null)
			{
				return;
			}

			StartCoroutine(DelayToPlay(clipIdx, audioSource, fadeInTime, delayTime, finishAction));
		}

		private IEnumerator DelayToPlay(int clipIdx, XAudioSource audioSource, float fadeInTime, float delayTime, Action<string> finishAction)
		{
			yield return new WaitForSeconds(delayTime);

			audioSource.soundInfo = GetSoundByIdx(clipIdx);
			audioSource.Play(fadeInTime, finishAction);
		}

		/************************************
		 * Stop Sound
		 * 停止聲音可以透過clip name或是 clip index
		 * **********************************/
		public void StopSound(AudioChannel channel, float fadeOutTime = 1f)
		{
			XAudioSource audioSource = GetSourceByChannel(channel);

			if (audioSource == null)
			{
				return;
			}

			audioSource.Stop(fadeOutTime);
		}

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
			XAudioSource audioSource = GetSourceByClipIndex(clipIdx);

			if (audioSource == null)
			{
				return;
			}

			audioSource.Stop(fadeOutTime);
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
			XAudioSource audioSource = GetSourceByClipIndex(clipIdx);

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
			XAudioSource audioSource = GetSourceByClipIndex(clipIdx);

			if (audioSource == null)
			{
				return;
			}

			audioSource.Resume();
		}

		/************************************
		 * is playing
		 * **********************************/
		public bool IsPlaying(AudioChannel channel)
		{
			foreach(SoundInfo soundInfo in soundBank)
			{
				if(soundInfo.channel == channel)
				{
					if(IsPlaying(soundInfo.clipName))
					{
						return true;
					}
				}
			}

			return false;
		}

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
			XAudioSource audioSource = GetSourceByClipIndex(clipIdx);

			if(audioSource == null)
			{
				// audioSource不存在
				return false;
			}

			SoundInfo info = GetSoundByIdx(clipIdx);

			if(info == null)
			{
				return false;
			}

			if (audioSource.soundInfo != info)
			{
				// 當前 clip不為指定的 clip
				return false;
			}

			// 判斷audioSource是否有play
			return audioSource.IsPlaying();
		}

		/************************************
		 * Other
		 * **********************************/

		private XAudioSource GetSourceByClipIndex(int clipIdx)
		{
			if (!soundBank.IsValidIndex<SoundInfo>(clipIdx))
			{
				Debug.LogWarning($"soundBank沒有這個Idx {clipIdx}");

				return null;
			}

			AudioChannel channel = soundBank[clipIdx].channel;

			return GetSourceByChannel(channel);
		}

		private XAudioSource GetSourceByChannel(AudioChannel channel)
		{
			if (!sourceMap.ContainsKey(channel))
			{
				Debug.LogError($"使用不存在的 {channel}");

				return null;
			}

			XAudioSource audioSource = sourceMap[channel];

			return audioSource;
		}

		private SoundInfo GetSoundByIdx(int clipIdx)
		{
			if (!soundBank.IsValidIndex<SoundInfo>(clipIdx))
			{
				Debug.LogError($"soundBank沒有這個Idx {clipIdx}");

				return null;
			}

			SoundInfo info = soundBank[clipIdx];

			return info;
		}

		public AudioChannel GetChannelByClipName(string clipName)
		{
			int clipIdx = soundBank.FindIndex((E04) =>
			{
				return E04.clipName == clipName;
			});

			if (!soundBank.IsValidIndex<SoundInfo>(clipIdx))
			{
				Debug.LogWarning($"soundBank沒有這個Idx {clipIdx}");

				return AudioChannel.None;
			}

			AudioChannel channel = soundBank[clipIdx].channel;

			return channel;
		}

		public void RefreshBank()
		{
			soundBank = new List<SoundInfo>();
			foreach (SoundGroup group in soundGroup)
			{
				soundBank.AddRange(group.infoList);
			}
		}

		/************************************
		* 其他
		* **********************************/
		private bool IsPlayingSoundOnBGChannel()
		{
			if (!sourceMap.ContainsKey(AudioChannel.Channel_Background))
			{
				return false;
			}

			XAudioSource bgAudioSource = sourceMap[AudioChannel.Channel_Background];

			return bgAudioSource.IsPlaying();
		}

		private int GetNumOfPlayingSource()
		{
			int result = 0;

			foreach(KeyValuePair<AudioChannel, XAudioSource> kvp in sourceMap)
			{
				if(kvp.Value.IsPlaying())
				{
					++result;
				}
			}

			return result;
		}

		private XAudioSource GetBGAudioSource()
		{
			if (sourceMap.ContainsKey(AudioChannel.Channel_Background))
			{
				return sourceMap[AudioChannel.Channel_Background];
			}
			else
			{
				return null;
			}
		}
	}
}
