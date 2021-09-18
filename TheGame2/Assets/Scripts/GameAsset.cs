using UnityEngine;
using UnityEngine.SceneManagement;

public class GameAsset : MonoBehaviour
{
    public static GameAsset Instance { get; private set; }
    //public Player player; config the player variable after

    public PSoundAudioClip[] playerAudioClipsArray;
    public SoundAudioClip[] soundAudioClipsArray;

    [System.Serializable]
    public class SoundAudioClip
    {
        public SoundManager.Sound sound;
        public AudioClip audioClip;
    }
    
    [System.Serializable]
    public class PSoundAudioClip
    {
        public SoundManager.PlayerSound sound;
        public AudioClip audioClip;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}