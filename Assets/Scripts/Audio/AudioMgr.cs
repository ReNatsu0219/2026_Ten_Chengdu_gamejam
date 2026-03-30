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
        public List<AudioSource> sources = new List<AudioSource>();
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

        var sources = PrepareAudioSources(_musicGO, volume);

        float pitch = GetPitchFromDuration(music, targetDuration);

        foreach (var source in sources)
        {
            source.loop = isLoop;
            source.clip = music;
            source.outputAudioMixerGroup = _musicGroup;
            source.pitch = pitch;
            source.Play();
        }
    }

    public void StopMusic()
    {
        if (_musicGO == null) return;

        var sources = _musicGO.GetComponents<AudioSource>();
        foreach (var source in sources)
        {
            if (source != null)
                source.Stop();
        }
    }

    public void PauseMusic()
    {
        if (_musicGO == null) return;

        var sources = _musicGO.GetComponents<AudioSource>();
        foreach (var source in sources)
        {
            if (source != null)
                source.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (_musicGO == null) return;

        var sources = _musicGO.GetComponents<AudioSource>();
        foreach (var source in sources)
        {
            if (source != null)
                source.UnPause();
        }
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

        var sources = PrepareAudioSources(go, volume);

        float pitch = targetDuration > 0f
            ? GetPitchFromDuration(clip, targetDuration)
            : Random.Range(minPitch, maxPitch);

        foreach (var audioSource in sources)
        {
            audioSource.loop = isLoop;
            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = _sfxGroup;
            audioSource.pitch = pitch;
            audioSource.Play();
        }

        if (isLoop && !string.IsNullOrEmpty(key))
        {
            _loopingAudio[key] = new LoopingAudioHandle
            {
                go = go,
                isDimensional = true,
                sources = sources
            };
        }
        else
        {
            WaitAudioFinishThenRelease(go, true, pitch).Forget();
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
        var sources = PrepareAudioSources(go, volume);

        float pitch = targetDuration > 0f
            ? GetPitchFromDuration(clip, targetDuration)
            : Random.Range(minPitch, maxPitch);

        foreach (var audioSource in sources)
        {
            audioSource.loop = isLoop;
            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = _sfxGroup;
            audioSource.pitch = pitch;
            audioSource.Play();
        }

        if (isLoop && !string.IsNullOrEmpty(key))
        {
            _loopingAudio[key] = new LoopingAudioHandle
            {
                go = go,
                isDimensional = false,
                sources = sources
            };
        }
        else
        {
            WaitAudioFinishThenRelease(go, false, pitch).Forget();
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
                if (handle.sources != null)
                {
                    foreach (var source in handle.sources)
                    {
                        if (source != null)
                            source.Stop();
                    }
                }
                else
                {
                    var sources = handle.go.GetComponents<AudioSource>();
                    foreach (var source in sources)
                    {
                        if (source != null)
                            source.Stop();
                    }
                }

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

    private async UniTaskVoid WaitAudioFinishThenRelease(GameObject go, bool isDimensional, float pitch)
    {
        var primarySource = GetPrimaryAudioSource(go);
        if (primarySource == null || primarySource.clip == null)
        {
            if (isDimensional)
                _DimensionalAudioPool.Release(go);
            else
                _normalAudioPool.Release(go);
            return;
        }

        float waitTime = primarySource.clip.length;
        if (pitch > 0f)
            waitTime /= pitch;

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

        return Mathf.Clamp(pitch, 0.1f, 3f);
    }

    private float MapVolume(float x)
    {
        x = Mathf.Max(0.0001f, x);
        return 20f * Mathf.Log10(x);
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

    #region Volume > 1 Support

    private List<AudioSource> PrepareAudioSources(GameObject go, float totalVolume)
    {
        totalVolume = Mathf.Max(0f, totalVolume);

        int requiredCount = Mathf.Max(1, Mathf.CeilToInt(totalVolume));
        EnsureAudioSourceCount(go, requiredCount);

        var allSources = go.GetComponents<AudioSource>();
        var result = new List<AudioSource>(requiredCount);

        float remaining = totalVolume;

        for (int i = 0; i < allSources.Length; i++)
        {
            var source = allSources[i];
            if (source == null) continue;

            source.Stop();
            source.clip = null;
            source.loop = false;
            source.pitch = 1f;

            if (i < requiredCount && remaining > 0f)
            {
                float piece = Mathf.Min(1f, remaining);
                source.volume = piece;
                source.enabled = true;
                result.Add(source);
                remaining -= piece;
            }
            else
            {
                source.volume = 0f;
                source.enabled = true;
            }
        }

        return result;
    }

    private void EnsureAudioSourceCount(GameObject go, int count)
    {
        var sources = go.GetComponents<AudioSource>();

        if (sources.Length == 0)
        {
            go.AddComponent<AudioSource>();
            sources = go.GetComponents<AudioSource>();
        }

        var template = sources[0];

        while (sources.Length < count)
        {
            var newSource = go.AddComponent<AudioSource>();
            CopyAudioSourceSettings(template, newSource);
            sources = go.GetComponents<AudioSource>();
        }
    }

    private AudioSource GetPrimaryAudioSource(GameObject go)
    {
        var sources = go.GetComponents<AudioSource>();
        if (sources == null || sources.Length == 0) return null;
        return sources[0];
    }

    private void CopyAudioSourceSettings(AudioSource from, AudioSource to)
    {
        if (from == null || to == null) return;

        to.playOnAwake = from.playOnAwake;
        to.bypassEffects = from.bypassEffects;
        to.bypassListenerEffects = from.bypassListenerEffects;
        to.bypassReverbZones = from.bypassReverbZones;
        to.priority = from.priority;
        to.mute = from.mute;
        to.panStereo = from.panStereo;
        to.spatialBlend = from.spatialBlend;
        to.reverbZoneMix = from.reverbZoneMix;
        to.dopplerLevel = from.dopplerLevel;
        to.spread = from.spread;
        to.rolloffMode = from.rolloffMode;
        to.minDistance = from.minDistance;
        to.maxDistance = from.maxDistance;
        to.ignoreListenerVolume = from.ignoreListenerVolume;
        to.ignoreListenerPause = from.ignoreListenerPause;
        to.outputAudioMixerGroup = from.outputAudioMixerGroup;

#if UNITY_2022_2_OR_NEWER
        to.velocityUpdateMode = from.velocityUpdateMode;
#endif
    }

    #endregion
}