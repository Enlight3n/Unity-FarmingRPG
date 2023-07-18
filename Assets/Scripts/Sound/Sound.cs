using UnityEngine;
/// <summary>
/// 放在对象池里，当我从对象池里取出时，播放赋予的audioClip
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Sound : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void SetSound(SoundItem soundItem)
    {
        audioSource.pitch = Random.Range(soundItem.soundPitchRandomVariationMin, soundItem.soundPitchRandomVariationMax);
        audioSource.volume = soundItem.soundVolume;
        audioSource.clip = soundItem.soundClip;
    }

    //SetActive(true)，会触发MonoBehaviour.OnEnable()事件，就算对象之前本就是activeSelf==true，事件依然会发生； 
    private void OnEnable()
    {
        if (audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    //SetActive(false)，会触发MonoBehaviour.OnDisable()事件,就算对象之前本就是activeSelf==false，事件依然会发生；
    private void OnDisable()
    {
        audioSource.Stop();
    }
}