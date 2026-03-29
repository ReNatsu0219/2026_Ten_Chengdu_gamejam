using System.Collections.Generic;
using System.Linq;
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

    public void PlayMusic(AudioClip music)
    {
        if (music == null) return;

        if (_musicGO == null) _musicGO = Instantiate(_musicPrefab, transform);
        var audioSource = _musicGO.GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = music;
        audioSource.outputAudioMixerGroup = _musicGroup;
        audioSource.Play();
    }
    public void PlayDimensionalSFX(List<AudioClip> clips, Vector2 position, bool isLoop = false)
    {
        var idx = Random.Range(0, clips.Count);
        PlayDimensionalRandomSFX(clips[idx], position, isLoop);
    }
    public void PlayDimensionalSFX(AudioClip clip, Vector2 position, bool isLoop = false)
    => PlayDimensionalRandomSFX(clip, position, isLoop);

    public void PlayNormalSFX(List<AudioClip> clips, Vector2 position, bool isLoop = false)
    {
        var idx = Random.Range(0, clips.Count);
        PlayNormalRandomSFX(clips[idx], isLoop);
    }
    public void PlayNormalSFX(AudioClip clip, Vector2 position, bool isLoop = false)
    => PlayNormalRandomSFX(clip, isLoop);

    private void PlayDimensionalRandomSFX(AudioClip clip, Vector2 position, bool isLoop, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        if (clip == null) return;

        var go = _DimensionalAudioPool.Get();
        go.transform.position = position;

        var audioSource = go.GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = _sfxGroup;
        audioSource.Play();

        if (!isLoop) WaitAudioFinishThenRelease(go).Forget();
    }
    private void PlayNormalRandomSFX(AudioClip clip, bool isLoop, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        if (clip == null) return;

        var go = _normalAudioPool.Get();

        var audioSource = go.GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = _sfxGroup;
        audioSource.Play();

        if (!isLoop) WaitAudioFinishThenRelease(go).Forget();
    }

    private async UniTaskVoid WaitAudioFinishThenRelease(GameObject go)
    {
        var audioSource = go.GetComponent<AudioSource>();
        await UniTask.WaitForSeconds(audioSource.clip.length);
        _DimensionalAudioPool.Release(go);
    }

    private float MapVolume(float x)
    {
        x = Mathf.Clamp01(x);
        return 20f * Mathf.Log10(0.999f * x + 0.001f);
    }
    public float MainVolume { set => _mainGroup.audioMixer.SetFloat("MainVolume", MapVolume(value)); }
    public float MusicVolume { set => _musicGroup.audioMixer.SetFloat("MusicVolume", MapVolume(value)); }
    public float SFXVolume { set => _sfxGroup.audioMixer.SetFloat("SFXVolume", MapVolume(value)); }
}
