/*
	Eric Scott Megitt, SU22C LBS Gothenburg
	eric.megitt@elev.ga.lbs.se
	Version 0.1.0

	See ChangeLog at Bottom
*/


using System;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;

using ScottsEssentials;
using AudioManagerClasses;

public class SoundSettingScript : SingletonPersistent<SoundSettingScript>
{
	//private variables
	Dictionary<string, AnimationCurve> fades;
	readonly AnimationCurve defaultFade = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	float masterVolume = 1f;
	float musicVolume = 1f;
	float sfxVolume = 1f;

	//inspector
	[SerializeField] Fade[] fadeList;
	[Header("Sounds"), Space(5)]
	[SerializeField] Sound[] musicSounds;
	[SerializeField] Sound[] sfxSounds;

	#region StartUpConfiguration
	protected override void WakeUp()
	{
		fades = ConvertToFadeDictionary(ref fadeList);

		CreateAudioSources(musicSounds);
		CreateAudioSources(sfxSounds);
	}

	Dictionary<string, AnimationCurve> ConvertToFadeDictionary(ref Fade[] fadeList)
	{
		Dictionary<string, AnimationCurve> fades = new();
		foreach (Fade fadeListIndex in fadeList)
			fades.Add(fadeListIndex.name, fadeListIndex.fadeCurve);
		fadeList = null;
		return fades;
	}

	/// <summary>
	/// This goes through the array <paramref name="sounds"/> and checks if every index is null, if it is then it creates an AudioSource on this Object and applies the values associated with the index.
	/// </summary>
	/// <param name="sounds">The <see cref="Sound"/> you wish to create an <see cref="AudioSource"/> for</param>
	/// <param name="autoPlay">Toggeling this makes it so that the audioClip gets played upon it's creation</param>
	void CreateAudioSources(Sound[] sounds)
	{
		float tempVolume = (sounds == musicSounds) ? musicVolume : sfxVolume;

		for (int i = 0; i < sounds.Length; i++)
		{
			ref Sound sound = ref sounds[i];

			if (sound.audioSource == null)
			{
				sound.audioSource = gameObject.AddComponent<AudioSource>();
				sound.audioSource.Stop();
				sound.audioSource.clip = sound.audioClip;
				sound.audioSource.volume = tempVolume * sound.volume;
				sound.audioSource.pitch = sound.pitch;
				sound.audioSource.loop = sound.loop;
			}
		}
	}


	#endregion StartUpConfiguration

	#region PublicMethods
	public void ToggleSound(string audioName, string fadeName = null, float fadeDuration = 0, Action action = null) => PlaySound(audioName, fadeName, fadeDuration, false, action);
	public void StartSound(string audioName, string fadeName = null, float fadeDuration = 0, Action action = null) => PlaySound(audioName, fadeName, fadeDuration, true, action);

	public bool SoundIsPlaying(string audioName) => SoundIsPlaying(audioName, 0);

	/// <param name="soundIndex">0 is the first index</param>
	public bool SoundIsPlaying(string audioName, int soundIndex)
	{
		try
		{
			return SoundsWithName(audioName)[soundIndex].audioSource.isPlaying;
		}
		catch
		{
			throw new Exception($"Sound \"{audioName}\" wasn't found.");
		}

	}

	public void ChangeSFXVolume(float value) => ChangeVolume(ref sfxVolume, value);
	public void ChangeMusicVolume(float value) => ChangeVolume(ref musicVolume, value);
	public void ChangeMasterVolume(float value)
	{
		AudioListener.volume *= value / masterVolume;
		masterVolume = value;
	}
	#endregion PublicMethods

	void PlaySound(string audioName, string fadeName, float fadeDuration, bool stopFirst, Action action)
	{
		TryAssignAnimationCurve(fadeName, out AnimationCurve fadeCurve);

		foreach (Sound sound in SoundsWithName(audioName))
		{
			if (stopFirst)
				sound.audioSource.Stop();

			StartCoroutine(SoundFade(sound, fadeCurve, fadeDuration, action));
		}
	}

	void TryAssignAnimationCurve(string curveName, out AnimationCurve animationCurve)
	{
		if (!fades.TryGetValue(curveName, out animationCurve))
		{
			if (curveName != null)
				Debug.LogWarning($"FadeCurve \"{curveName}\" wasn't found. AnimationCurve: defaultCurve applied");

			animationCurve = defaultFade;
		}
	}

	List<Sound> SoundsWithName(string audioName)
	{
		bool hasFoundSound = false;

		List<Sound> returnValue = new();

		foreach (Sound sound in musicSounds)
			if (sound.name == audioName)
			{
				returnValue.Add(sound);
				hasFoundSound = true;
			}
		foreach (Sound sound in sfxSounds)
			if (sound.name == audioName)
			{
				returnValue.Add(sound);
				hasFoundSound = true;
			}

		if (!hasFoundSound)
		{
			Debug.LogWarning($"Sound \"{audioName}\" was not found, check your spelling");
		}

		return returnValue;
	}

	IEnumerator SoundFade(Sound sound, AnimationCurve fadeCurve, float fadeDuration, Action action)
	{
		float tempVolume = GetAudioCategoryVolume(sound);
		if (!sound.audioSource.isPlaying) //fade in
		{
			sound.audioSource.volume = 0f; //without this the volume is att 100% for a sahort but noticible time
			sound.audioSource.Play();
			for (float i = 0; i < fadeDuration; i += Time.fixedDeltaTime) //i is counted in seconds, every loop Time.fixedDeltaTime is added to it (bcuz its simple to use Time.deltaTime)
			{
				yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
				sound.audioSource.volume = fadeCurve.Evaluate(i / fadeDuration) * sound.volume * tempVolume;
			}
		}
		else //fade out
		{
			for (float i = fadeDuration; i > 0; i -= Time.fixedDeltaTime) //here the fadeCurve is indexed backwards
			{
				yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
				sound.audioSource.volume = fadeCurve.Evaluate(i / fadeDuration) * sound.volume * tempVolume;
			}
			sound.audioSource.Stop();
		}
		while (sound.audioSource.isPlaying)
		{
			yield return new WaitForFixedUpdate();
		}
		if (action != null && !sound.loop)
		{
			action();
		}
	}

	float GetAudioCategoryVolume(Sound sound) => musicSounds.Contains(sound) ? musicVolume : sfxVolume;

	/// <summary>
	/// Assigns <paramref name="level"/> to <paramref name="volume"/>
	/// </summary>
	/// <param name="volume">A reference to the variable which holds the current levels for sound</param>
	/// <param name="level">A value from 0 to 1 holding the wished for volume level</param>
	void ChangeVolume(ref float volume, float level)
	{
		volume = level;
		UpdateVolume();
	}

	/// <summary>
	/// Goes through all <see cref="Sound"/> and assigns a new level to their <see cref="AudioSource"/>
	/// </summary>
	void UpdateVolume()
	{
		for (int i = 0; i < musicSounds.Length; i++)
			musicSounds[i].audioSource.volume = musicVolume * musicSounds[i].volume;

		for (int i = 0; i < sfxSounds.Length; i++)
			sfxSounds[i].audioSource.volume = sfxVolume * sfxSounds[i].volume;
	}
}

namespace AudioManagerClasses
{
	[Serializable]//makes sure this shows up in the inspector
	public class Sound
	{
		public string name;
		[HideInInspector] public AudioSource audioSource;

		[Space(10)]
		[SerializeField] public AudioClip audioClip;
		[SerializeField, Range(0, 1)] public float volume;
		[SerializeField, Range(-3, 3)] public float pitch;
		[SerializeField] public bool loop;
	}

	[Serializable]
	public struct Fade
	{
		[SerializeField] public string name;
		[SerializeField] public AnimationCurve fadeCurve;
	}
}

/*

--Versions-- 

0.0.1:
	ChangeMasterVolume now uses AudioListener.Volume, instead of changing the volume of every AudioSource
0.0.2:
	Can see if audioclip is playing; SoundIsPlaying("clip1");
	ChangeMasterVolume has respect for the already existing AudioListener.Volume
0.1.0
	Sound and Fade are classes instead of structs

*/