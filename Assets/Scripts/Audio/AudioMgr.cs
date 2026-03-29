using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

public class AudioMgr : MonoSingleton<AudioMgr>
{
    [SerializeField] private GameObject _dimensionalAudioPrefab;
    [SerializeField] private GameObject _normalAudioPrefab;
    [SerializeField] private GameObject _musicPrefab;
    [SerializeField] private AudioMixerGroup _mainGroup;
    [SerializeField] private AudioMixerGroup _musicGroup;
    [SerializeField] private AudioMixerGroup _sfxGroup;

    private ObjectPool<GameObject> _DimensionalAudioPool;
    private ObjectPool<GameObject> _normalAudioPool;
    private GameObject _musicGO;

    private Dictionary<string, LoopingAudioHandle> _loopingAudio = new Dictionary<string, LoopingAudioHandle>();

    private class LoopingAudioHandle
    {
        public GameObject go;
        public bool isDimensional;
    }

    protected override void Awake()
    {
        base.Awake();

        _DimensionalAudioPool = new ObjectPool<GameObject>(
            () => Instantiate(_dimensionalAudioPrefab, transform),
            go => go.SetActive(true),
            go => go.SetActive(false),
            go => Destroy(go)
        );

        _normalAudioPool = new ObjectPool<GameObject>(
            () => Instantiate(_normalAudioPrefab, transform),
            go => go.SetActive(true),
            go => go.SetActive(false),
            go => Destroy(go)
        );
    }

    #region Music

    public void PlayMusic(AudioClip music, bool isLoop = true, float volume = 1f, float targetDuration = -1f)
    {
        if (music == null) return;

        if (_musicGO == null)
            _musicGO = Instantiate(_musicPrefab, transform);

        var audioSource = _musicGO.GetComponent<AudioSource>();
        audioSource.loop = isLoop;
        audioSource.clip = music;
        audioSource.outputAudioMixerGroup = _musicGroup;
        audioSource.volume = volume;
        audioSource.pitch = GetPitchFromDuration(music, targetDuration);
        audioSource.Play();
    }

    public void StopMusic()
    {
        if (_musicGO == null) return;

        var audioSource = _musicGO.GetComponent<AudioSource>();
        audioSource.Stop();
    }

    public void PauseMusic()
    {
        if (_musicGO == null) return;

        var audioSource = _musicGO.GetComponent<AudioSource>();
        audioSource.Pause();
    }

    public void ResumeMusic()
    {
        if (_musicGO == null) return;

        var audioSource = _musicGO.GetComponent<AudioSource>();
        audioSource.UnPause();
    }

    #endregion

    #region Dimensional SFX

    public void PlayDimensionalSFX(
        List<AudioClip> clips,
        Vector2 position,
        bool isLoop = false,
        string key = null,
        float volume = 1f,
        float targetDuration = -1f)
    {
        if (clips == null || clips.Count == 0) return;

        var idx = Random.Range(0, clips.Count);
        PlayDimensionalRandomSFX(clips[idx], position, isLoop, key, volume, targetDuration);
    }

    public void PlayDimensionalSFX(
        AudioClip clip,
        Vector2 position,
        bool isLoop = false,
        string key = null,
        float volume = 1f,
        float targetDuration = -1f)
        => PlayDimensionalRandomSFX(clip, position, isLoop, key, volume, targetDuration);

    private void PlayDimensionalRandomSFX(
        AudioClip clip,
        Vector2 position,
        bool isLoop,
        string key = null,
        float volume = 1f,
        float targetDuration = -1f,
        float minPitch = 0.9f,
        float maxPitch = 1.1f)
    {
        if (clip == null) return;

        if (isLoop && !string.IsNullOrEmpty(key))
            StopLoopSFX(key);

        var go = _DimensionalAudioPool.Get();
        go.transform.position = position;

        var audioSource = go.GetComponent<AudioSource>();
        audioSource.loop = isLoop;
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = _sfxGroup;
        audioSource.volume = volume;

        if (targetDuration > 0f)
            audioSource.pitch = GetPitchFromDuration(clip, targetDuration);
        else
            audioSource.pitch = Random.Range(minPitch, maxPitch);

        audioSource.Play();

        if (isLoop && !string.IsNullOrEmpty(key))
        {
            _loopingAudio[key] = new LoopingAudioHandle
            {
                go = go,
                isDimensional = true
            };
        }
        else
        {
            WaitAudioFinishThenRelease(go, true).Forget();
        }
    }

    #endregion

    #region Normal SFX

    public void PlayNormalSFX(
        List<AudioClip> clips,
        Vector2 position,
        bool isLoop = false,
        string key = null,
        float volume = 1f,
        float targetDuration = -1f)
    {
        if (clips == null || clips.Count == 0) return;

        var idx = Random.Range(0, clips.Count);
        PlayNormalRandomSFX(clips[idx], isLoop, key, volume, targetDuration);
    }

    public void PlayNormalSFX(
        AudioClip clip,
        Vector2 position,
        bool isLoop = false,
        string key = null,
        float volume = 1f,
        float targetDuration = -1f)
        => PlayNormalRandomSFX(clip, isLoop, key, volume, targetDuration);

    private void PlayNormalRandomSFX(
        AudioClip clip,
        bool isLoop,
        string key = null,
        float volume = 1f,
        float targetDuration = -1f,
        float minPitch = 0.9f,
        float maxPitch = 1.1f)
    {
        if (clip == null) return;

        if (isLoop && !string.IsNullOrEmpty(key))
            StopLoopSFX(key);

        var go = _normalAudioPool.Get();

        var audioSource = go.GetComponent<AudioSource>();
        audioSource.loop = isLoop;
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = _sfxGroup;
        audioSource.volume = volume;

        if (targetDuration > 0f)
            audioSource.pitch = GetPitchFromDuration(clip, targetDuration);
        else
            audioSource.pitch = Random.Range(minPitch, maxPitch);

        audioSource.Play();

        if (isLoop && !string.IsNullOrEmpty(key))
        {
            _loopingAudio[key] = new LoopingAudioHandle
            {
                go = go,
                isDimensional = false
            };
        }
        else
        {
            WaitAudioFinishThenRelease(go, false).Forget();
        }
    }

    #endregion

    #region Stop Loop SFX

    public void StopLoopSFX(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (_loopingAudio.TryGetValue(key, out var handle))
        {
            if (handle != null && handle.go != null)
            {
                var audioSource = handle.go.GetComponent<AudioSource>();
                if (audioSource != null)
                    audioSource.Stop();

                if (handle.isDimensional)
                    _DimensionalAudioPool.Release(handle.go);
                else
                    _normalAudioPool.Release(handle.go);
            }

            _loopingAudio.Remove(key);
        }
    }

    public void StopAllLoopSFX()
    {
        var keys = new List<string>(_loopingAudio.Keys);
        foreach (var key in keys)
        {
            StopLoopSFX(key);
        }
    }

    #endregion

    private async UniTaskVoid WaitAudioFinishThenRelease(GameObject go, bool isDimensional)
    {
        var audioSource = go.GetComponent<AudioSource>();
        if (audioSource == null || audioSource.clip == null)
        {
            if (isDimensional)
                _DimensionalAudioPool.Release(go);
            else
                _normalAudioPool.Release(go);
            return;
        }

        float waitTime = audioSource.clip.length;
        if (audioSource.pitch > 0f)
            waitTime /= audioSource.pitch;

        await UniTask.WaitForSeconds(waitTime);

        if (isDimensional)
            _DimensionalAudioPool.Release(go);
        else
            _normalAudioPool.Release(go);
    }

    private float GetPitchFromDuration(AudioClip clip, float targetDuration)
    {
        if (clip == null || targetDuration <= 0f) return 1f;

        float pitch = clip.length / targetDuration;

        // Unity AudioSource.pitch 常用安全范围，避免太夸张
        return Mathf.Clamp(pitch, 0.1f, 3f);
    }

    private float MapVolume(float x)
    {
        x = Mathf.Clamp01(x);
        return 20f * Mathf.Log10(0.999f * x + 0.001f);
    }

    public float MainVolume
    {
        set => _mainGroup.audioMixer.SetFloat("MainVolume", MapVolume(value));
    }

    public float MusicVolume
    {
        set => _musicGroup.audioMixer.SetFloat("MusicVolume", MapVolume(value));
    }

    public float SFXVolume
    {
        set => _sfxGroup.audioMixer.SetFloat("SFXVolume", MapVolume(value));
    }
}