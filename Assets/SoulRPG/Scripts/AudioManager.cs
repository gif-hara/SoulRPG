using HK;
using UnityEngine;
using UnityEngine.Audio;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField]
        private AudioSource bgmSource;

        [SerializeField]
        private AudioSource sfxSource;

        [SerializeField]
        private AudioMixer audioMixer;

        void Start()
        {
            SetVolumeMaster(PlayerPrefs.GetFloat("MasterVolume", 0.8f));
            SetVolumeBGM(PlayerPrefs.GetFloat("BgmVolume", 0.8f));
            SetVolumeSFX(PlayerPrefs.GetFloat("SfxVolume", 0.8f));
        }

        public static void PlayBGM(AudioClip clip)
        {
            var instance = TinyServiceLocator.Resolve<AudioManager>();
            instance.bgmSource.clip = clip;
            instance.bgmSource.Play();
        }

        public static void PlaySFX(AudioClip clip)
        {
            var instance = TinyServiceLocator.Resolve<AudioManager>();
            instance.sfxSource.PlayOneShot(clip);
        }

        public static void StopBGM()
        {
            var instance = TinyServiceLocator.Resolve<AudioManager>();
            instance.bgmSource.Stop();
        }

        public static void PlaySFX(string clipName)
        {
            var instance = TinyServiceLocator.Resolve<AudioManager>();
            var clip = TinyServiceLocator.Resolve<GameRule>().AudioDatabase.Get(clipName).Clip;
            instance.sfxSource.PlayOneShot(clip);
        }

        public static void SetVolumeMaster(float volume)
        {
            SetVolume("MasterVolume", volume);
        }

        public static void SetVolumeBGM(float volume)
        {
            SetVolume("BgmVolume", volume);
        }

        public static void SetVolumeSFX(float volume)
        {
            SetVolume("SfxVolume", volume);
        }

        public static float GetVolumeMaster()
        {
            return GetVolume("MasterVolume");
        }

        public static float GetVolumeBGM()
        {
            return GetVolume("BgmVolume");
        }

        public static float GetVolumeSFX()
        {
            return GetVolume("SfxVolume");
        }

        public static void SetVolume(string name, float volume)
        {
            var instance = TinyServiceLocator.Resolve<AudioManager>();
            volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(name, volume);
            if (volume > 0)
            {
                volume = Mathf.Log10(volume) * 20;
            }
            else
            {
                volume = -80;
            }
            instance.audioMixer.SetFloat(name, volume);
        }

        public static float GetVolume(string name)
        {
            var instance = TinyServiceLocator.Resolve<AudioManager>();
            instance.audioMixer.GetFloat(name, out var volume);
            return Mathf.Pow(10, volume / 20);
        }
    }
}
