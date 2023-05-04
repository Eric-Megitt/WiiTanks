using System;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;

using AudioManagerClasses;

public class AudioManager : SingletonPersistent<AudioManager>
{
    //private variables
    Dictionary<string, AnimationCurve> fades;

    AnimationCurve defaultFade = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public AnimationCurve DefaultCurve
    {
        get { return defaultFade; }
        set { defaultFade = value; fades["defaultFade"] = defaultFade; }
    }

    float masterVolume = 1f;
    float musicVolume = 1f;
    float sfxVolume = 1f;

    //inspector
    [SerializeField] Fade[] fadeList;
    [Header("AudioSources"), Space(5)]
    [SerializeField] Sound[] musicAudioSources;
    [SerializeField] SFXSound[] sfxAudioSources;

    #region StartUpConfiguration
    protected override void Awake()
    {
        masterVolume = AudioListener.volume;
        base.Awake();

        fades = ConvertToFadeDictionary(ref fadeList);

        CreateAudioSources(musicAudioSources);
        CreateAudioSources(sfxAudioSources);
    }

    private Dictionary<string, AnimationCurve> ConvertToFadeDictionary(ref Fade[] fadeList)
    {
        Dictionary<string, AnimationCurve> fades = new() { { "defaultFade", defaultFade } };

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
    private void CreateAudioSources(Sound[] sounds)
    {
        float tempVolume = (sounds == musicAudioSources) ? musicVolume : sfxVolume;
        for (int i = 0; i < sounds.Length; i++)
        {
            ref Sound sound = ref sounds[i];

            if (sound.audioSource == null)
            {
                sound.audioSource = gameObject.AddComponent<AudioSource>();
                sound.audioSource.playOnAwake = false;
                sound.audioSource.Stop();
                sound.audioSource.clip = sound.audioClip;
                sound.audioSource.volume = tempVolume * sound.volume;
                sound.audioSource.pitch = sound.pitch;
                sound.audioSource.loop = sound.loop;
            }
        }
    }
    private void CreateAudioSources(SFXSound[] sounds)
    {
        float tempVolume = (sounds == musicAudioSources) ? musicVolume : sfxVolume;

        for (int i = 0; i < sounds.Length; i++)
        {
            ref SFXSound sound = ref sounds[i];

            if (sound.audioSource == null)
            {
                sound.audioSource = gameObject.AddComponent<AudioSource>();
                sound.audioSource.playOnAwake = false;
                sound.audioSource.Stop();
                sound.audioSource.clip = sound.audioClip;
                sound.audioSource.volume = tempVolume * sound.volume;
                sound.audioSource.pitch = sound.pitch + (sound.pitch * UnityEngine.Random.Range(-sound.pitchVariation, sound.pitchVariation));
                sound.audioSource.loop = sound.loop;
            }
        }
    }


    #endregion StartUpConfiguration

    #region PublicMethods
    public void ToggleSound(string audioName, string fadeName = "defaultFade", float fadeDuration = 0f, Action callback = null) => PlaySound(audioName, fadeName, fadeDuration, false, callback);

    public void StartSound(string audioName, string fadeName = "defaultFade", float fadeDuration = 0f, Action callback = null) => PlaySound(audioName, fadeName, fadeDuration, true, callback);

    public void StopSound(string audioName, string fadeName = "defaultFade", float fadeDuration = 0f, Action callback = null)
    {
        foreach (Sound sound in SoundsWithName(audioName))
        {
            if (sound.audioSource.isPlaying)
                ToggleSound(audioName, fadeName, fadeDuration, callback);
        }
    }

    public void StopAllSounds()
    {
        foreach (Sound sound in musicAudioSources)
        {
            StopSound(sound.name);
        }
        foreach (Sound sound in sfxAudioSources)
        {
            StopSound(sound.name);
        }
    }

    public AudioClip GetAudioClip(string audioName)
    {
        foreach (Sound sound in SoundsWithName(audioName))
            return sound.audioClip;
        return null;
    }

    public Sound GetSound(string audioName)
    {
        foreach (Sound sound in SoundsWithName(audioName))
            return sound;
        return null;
    }

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
        AudioListener.volume = value * masterVolume;
    }
    #endregion PublicMethods

    private void PlaySound(string audioName, string fadeName, float fadeDuration, bool stopFirst, Action callback)
    {
        TryAssignAnimationCurve(fadeName, out AnimationCurve fadeCurve);

        foreach (Sound sound in musicAudioSources)
        {
            if (sound.name != audioName)
                continue;
            if (stopFirst)
                sound.audioSource.Stop();

            StartCoroutine(SoundFade(sound, fadeCurve, fadeDuration, callback));
        }
        foreach (SFXSound sound in sfxAudioSources)
        {
            if (sound.name != audioName)
                continue;
            if (stopFirst)
                sound.audioSource.Stop();
            sound.audioSource.pitch = sound.pitch + (sound.pitch * UnityEngine.Random.Range(-sound.pitchVariation, sound.pitchVariation));

            StartCoroutine(SoundFade(sound, fadeCurve, fadeDuration, callback));
        }
    }

    private void TryAssignAnimationCurve(string curveName, out AnimationCurve animationCurve)
    {
        if (curveName == null || !fades.TryGetValue(curveName, out animationCurve))
        {
            if (curveName != null)
                Debug.LogWarning($"FadeCurve \"{curveName}\" wasn't found. AnimationCurve: defaultCurve applied");

            animationCurve = defaultFade;
        }
    }

    private List<Sound> SoundsWithName(string audioName)
    {
        bool hasFoundSound = false;

        List<Sound> returnValue = new();

        foreach (Sound sound in musicAudioSources)
            if (sound.name == audioName)
            {
                returnValue.Add(sound);
                hasFoundSound = true;
            }
        foreach (Sound sound in sfxAudioSources)
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

    private IEnumerator SoundFade(Sound sound, AnimationCurve fadeCurve, float fadeDuration, Action callback)
    {
        float tempVolume = GetAudioCategoryVolume(sound);
        if (!sound.audioSource.isPlaying) //fade in
        {
            if (fadeDuration != 0f)
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
            sound.audioSource.volume = 0f;
            sound.audioSource.Stop();
        }
        while (sound.audioSource.isPlaying)
        {
            yield return new WaitForSeconds(0.001f);
        }
        if (callback != null && !sound.loop)
        {
            callback();
        }
    }

    private float GetAudioCategoryVolume(Sound sound) => musicAudioSources.Contains(sound) ? musicVolume : sfxVolume;

    /// <summary>
    /// Assigns <paramref name="level"/> to <paramref name="volume"/>
    /// </summary>
    /// <param name="volume">A reference to the variable which holds the current levels for sound</param>
    /// <param name="level">A value from 0 to 1 holding the wished for volume level</param>
    private void ChangeVolume(ref float volume, float level)
    {
        volume = level;
        UpdateVolume();
    }

    /// <summary>
    /// Goes through all <see cref="Sound"/> and assigns a new level to their <see cref="AudioSource"/>
    /// </summary>
    private void UpdateVolume()
    {
        for (int i = 0; i < musicAudioSources.Length; i++)
            musicAudioSources[i].audioSource.volume = musicVolume * musicAudioSources[i].volume;

        for (int i = 0; i < sfxAudioSources.Length; i++)
            sfxAudioSources[i].audioSource.volume = sfxVolume * sfxAudioSources[i].volume;
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

    [Serializable]//makes sure this shows up in the inspector
    public class SFXSound : Sound
    {
        [SerializeField] public float pitchVariation;
    }

    [Serializable]
    public struct Fade
    {
        [SerializeField] public string name;
        [SerializeField] public AnimationCurve fadeCurve;
    }
}