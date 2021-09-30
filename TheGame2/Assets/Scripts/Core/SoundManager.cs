using UnityEngine;
public class SoundManager : MonoBehaviour
{
    public enum PlayerSound
    {
        weaponAKFire,
        weaponGunFire,
        PlayerHurt,
        PlayerDie
    }

    public enum Sound
    {
        DoorOpen,
    }

    private static GameObject oneShotGameObject;
    private static AudioSource oneShotAudioSource;

    public static void PlaySound(Sound sound, float volume, Vector3 position)
    {
        GameObject soundGameObject = new GameObject("Sound");
        soundGameObject.transform.position = position;
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.clip = GetAudioClip(sound);
        audioSource.maxDistance = 100f;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0f;
        audioSource.volume = volume;
        audioSource.Play();
        Object.Destroy(soundGameObject, audioSource.clip.length);
    }
    public static void PlaySound(Sound sound, float volume)
    {
        if (!oneShotGameObject)
        {
            oneShotGameObject = new GameObject("One Shot Sound");
            oneShotAudioSource = oneShotGameObject.AddComponent<AudioSource>();
        }
        oneShotAudioSource.PlayOneShot(GetAudioClip(sound), volume);
    }

    private static AudioClip GetAudioClip(PlayerSound sound)
    {
        foreach (var variable in GameAsset.Instance.playerAudioClipsArray)
        {
            if (variable.sound == sound)
            {
                return variable.audioClip;
            }
        }
        Debug.Log("Sound: "+ sound + " not found!");
        return null;
    }
    
    private static AudioClip GetAudioClip(Sound sound)
    {
        foreach (var variable in GameAsset.Instance.soundAudioClipsArray)
        {
            if (variable.sound == sound)
            {
                return variable.audioClip;
            }
        }
        Debug.Log("Sound: "+ sound + " not found!");
        return null;
    }
}