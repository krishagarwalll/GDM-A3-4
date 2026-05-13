using System;
using UnityEngine;

namespace Game.Core.Audio
{
    [DefaultExecutionOrder(-200)]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Volumes (0..1)")]
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

        [Header("Music Clips")]
        [SerializeField] private AudioClip defaultMusic;

        [Header("SFX Clips")]
        [SerializeField] private SFXEntry[] sfxClips;

        private AudioClip _currentMusic;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null) musicSource = CreateSource("Music", loop: true);
            if (sfxSource == null) sfxSource = CreateSource("SFX", loop: false);

            ApplyVolumes();

            if (defaultMusic != null) PlayMusic(defaultMusic);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void PlayMusic(AudioClip clip, bool restartIfSame = false)
        {
            if (clip == null) return;
            if (!restartIfSame && _currentMusic == clip && musicSource.isPlaying) return;

            _currentMusic = clip;
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
            _currentMusic = null;
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }

        public void SetMusicVolume(float v) { musicVolume = Mathf.Clamp01(v); ApplyVolumes(); }
        public void SetSfxVolume(float v)   { sfxVolume   = Mathf.Clamp01(v); ApplyVolumes(); }

        private void ApplyVolumes()
        {
            if (musicSource != null) musicSource.volume = musicVolume;
            if (sfxSource != null)   sfxSource.volume   = sfxVolume;
        }

        /// <summary>Play a named SFX clip assigned in the Inspector.</summary>
        public void PlaySFXByName(string clipName, float volumeScale = 1f)
        {
            if (sfxClips == null) return;
            foreach (var entry in sfxClips)
            {
                if (string.Equals(entry.name, clipName, StringComparison.OrdinalIgnoreCase))
                {
                    PlaySFX(entry.clip, volumeScale);
                    return;
                }
            }
            Debug.LogWarning($"[AudioManager] SFX '{clipName}' not found.");
        }

        private AudioSource CreateSource(string name, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = loop;
            src.spatialBlend = 0f;
            return src;
        }
    }

    [Serializable]
    public struct SFXEntry
    {
        public string name;
        public AudioClip clip;
    }
}
