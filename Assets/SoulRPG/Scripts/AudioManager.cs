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
            var saveData = SaveData.LoadSafe();
            SetVolumeMaster(saveData.audioData.masterVolume);
            SetVolumeBGM(saveData.audioData.bgmVolume);
            SetVolumeSFX(saveData.audioData.sfxVolume);
        }

        public static void PlayBgm(AudioClip clip)
        {
            var instance = TinyServiceLocator.Resolve<AudioManager>();
            instance.bgmSource.clip = clip;
            instance.bgmSource.Play();
        }

        public static void PlayBgm(string clipName)
        {
            var clip = TinyServiceLocator.Resolve<GameRule>().AudioDatabase.Get(clipName).Clip;
            PlayBgm(clip);
        }

        public static void PlaySfx(AudioClip clip)
        {
            var instance = TinyServiceLocator.Resolve<AudioManager>();
            instance.sfxSource.PlayOneShot(clip);
        }

        public static void StopBgm()
        {
            var instance = TinyServiceLocator.Resolve<AudioManager>();
            instance.bgmSource.Stop();
        }

        public static void PlaySfx(string clipName)
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
