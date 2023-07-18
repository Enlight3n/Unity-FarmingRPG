using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using UnityEngine.SceneManagement;
public class AudioManager : SingletonMonobehaviour<AudioManager>
{
    [SerializeField] private GameObject soundPrefab = null;

    [Header("Audio Sources")] [SerializeField]
    private AudioSource ambientSoundAudioSource = null;

    [SerializeField] private AudioSource gameMusicAudioSource = null;

    [Header("Audio Mixers")] [SerializeField]
    private AudioMixer gameAudioMixer = null;

    [Header("Audio Snapshots")] [SerializeField]
    private AudioMixerSnapshot gameMusicSnapshot = null;

    [SerializeField] private AudioMixerSnapshot gameAmbientSnapshot = null;

    [Header("Other")]
    // Sound list and dictionary
    [SerializeField] private SO_SoundList so_soundList = null;
    
    [SerializeField] private SO_SceneSoundsList so_sceneSoundsList = null;
    [SerializeField] private float defaultSceneMusicPlayTimeSeconds = 120f;
    [SerializeField] private float sceneMusicStartMinSecs = 20f;
    [SerializeField] private float sceneMusicStartMaxSecs = 40f;
    [SerializeField] private float musicTransitionSecs = 8f;
    
    private Dictionary<SoundName, SoundItem> soundDictionary;
    private Dictionary<SceneName, SceneSoundsItem> sceneSoundsDictionary;

    private Coroutine playSceneSoundsCoroutine;

    protected override void Awake()
    {
        base.Awake();

        // Initialise sound dictionary
        soundDictionary = new Dictionary<SoundName, SoundItem>();

        // Load sound dictionary with sounds
        foreach (SoundItem soundItem in so_soundList.soundDetails)
        {
            soundDictionary.Add(soundItem.soundName, soundItem);
        }
        
        sceneSoundsDictionary = new Dictionary<SceneName, SceneSoundsItem>();

        // Load scene sounds dictionary
        foreach (SceneSoundsItem sceneSoundsItem in so_sceneSoundsList.sceneSoundsDetails)
        {
            sceneSoundsDictionary.Add(sceneSoundsItem.sceneName, sceneSoundsItem);
        }
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += PlaySceneSounds;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= PlaySceneSounds;
    }

    //环境音和背景音乐无时无刻都有，因此写在AudioManager中，AfterSceneLoadEvent事件中调用
    //我们对每个场景，使用一套环境音+背景音乐
    private void PlaySceneSounds()
    {
        SoundItem musicSoundItem = null;
        SoundItem ambientSoundItem = null;

        float musicPlayTime = defaultSceneMusicPlayTimeSeconds;

        // Try Get current scene
        if (Enum.TryParse<SceneName>(SceneManager.GetActiveScene().name, true, out SceneName currentSceneName))
        {
            // Get Music and Ambient Sounds For Scene
            if (sceneSoundsDictionary.TryGetValue(currentSceneName, out SceneSoundsItem sceneSoundsItem))
            {
                soundDictionary.TryGetValue(sceneSoundsItem.musicForScene, out musicSoundItem);
                soundDictionary.TryGetValue(sceneSoundsItem.ambientSoundForScene, out ambientSoundItem);
            }
            else
            {
                return;
            }

            // Stop any scene sounds already playing
            if (playSceneSoundsCoroutine != null)
            {
                StopCoroutine(playSceneSoundsCoroutine);
            }

            // Play scene ambient sounds and music
            playSceneSoundsCoroutine = StartCoroutine(PlaySceneSoundsRoutine(musicPlayTime, musicSoundItem, ambientSoundItem));
        }
    }
    
    private IEnumerator PlaySceneSoundsRoutine(float musicPlaySeconds, SoundItem musicSoundItem, SoundItem ambientSoundItem)
    {

        if (musicSoundItem != null && ambientSoundItem != null)
        {
            // Start with ambient sound
            PlayAmbientSoundClip(ambientSoundItem, 0f);

            // Wait for random range of seconds before playing music
            yield return new WaitForSeconds(UnityEngine.Random.Range(sceneMusicStartMinSecs, sceneMusicStartMaxSecs));

            // Play music
            PlayMusicSoundClip(musicSoundItem, musicTransitionSecs);

            // Wait for music play seconds before transitioning to ambient sounds
            yield return new WaitForSeconds(musicPlaySeconds);

            // Play ambient sound clip
            PlayAmbientSoundClip(ambientSoundItem, musicTransitionSecs);
        }
    }

    private void PlayMusicSoundClip(SoundItem musicSoundItem, float transitionTimeSeconds)
    {
        // Set Volume
        gameAudioMixer.SetFloat("MusicVolume", ConvertSoundVolumeDecimalFractionToDecibels(musicSoundItem.soundVolume));

        // Set clip & play
        gameMusicAudioSource.clip = musicSoundItem.soundClip;
        gameMusicAudioSource.Play();

        // Transition to music snapshot
        gameMusicSnapshot.TransitionTo(transitionTimeSeconds);
    }

    private void PlayAmbientSoundClip(SoundItem ambientSoundItem, float transitionTimeSeconds)
    {
        // Set Volume
        gameAudioMixer.SetFloat("AmbientVolume", ConvertSoundVolumeDecimalFractionToDecibels(ambientSoundItem.soundVolume));

        // Set clip & play
        ambientSoundAudioSource.clip = ambientSoundItem.soundClip;
        ambientSoundAudioSource.Play();

        // Transition to ambient
        gameAmbientSnapshot.TransitionTo(transitionTimeSeconds);
    }

    /// <summary>
    /// Convert volumeDecimalFraction to -80 to +20 decibel range
    /// </summary>
    private float ConvertSoundVolumeDecimalFractionToDecibels(float volumeDecimalFraction)
    {
        // Convert volume from decimal fraction to -80 to +20 decibel range

        return (volumeDecimalFraction * 100f - 80f);
    }
    
    public void PlaySound(SoundName soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out SoundItem soundItem) && soundPrefab != null)
        {
            //从对象池里取出来soundPrefab
            GameObject soundGameObject = PoolManager.Instance.ReuseObject(soundPrefab, Vector3.zero, Quaternion.identity);

            //获取sound脚本
            Sound sound = soundGameObject.GetComponent<Sound>();

            //设置播放的音效
            sound.SetSound(soundItem);
            
            //SetActive(true)，会触发MonoBehaviour.OnEnable()事件，就算对象之前本就是activeSelf==true，事件依然会发生； 
            //SetActive(false)，会触发MonoBehaviour.OnDisable()事件,就算对象之前本就是activeSelf==false，事件依然会发生；
            soundGameObject.SetActive(true);
            
            StartCoroutine(DisableSound(soundGameObject, soundItem.soundClip.length));
        }
    }

    private IEnumerator DisableSound(GameObject soundGameObject, float soundDuration)
    {
        yield return new WaitForSeconds(soundDuration);
        
        soundGameObject.SetActive(false);
    }
}