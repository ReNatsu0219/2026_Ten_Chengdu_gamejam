using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayableAnimService
{
    private PlayableOutput _output;
    private PlayableGraph _graph;
    private Dictionary<AnimationClip, AnimationClipPlayable> _playableDict = new Dictionary<AnimationClip, AnimationClipPlayable>();

    private Animator _animator;

    public PlayableAnimService(Animator animator)
    {
        _animator = animator;

        _graph = PlayableGraph.Create();
        _output = AnimationPlayableOutput.Create(_graph, "Anim", _animator);
    }
    public void Destroy() => _graph.Destroy();

    public void AddAnimationClip(AnimationClip clip)
    {
        if (!_playableDict.TryGetValue(clip, out var playable))
        {
            AnimationClipPlayable animPlayable = AnimationClipPlayable.Create(_graph, clip);
            _playableDict.Add(clip, animPlayable);
        }
    }

    public void PlayAnimation(AnimationClip clip)
    {
        if (!_playableDict.TryGetValue(clip, out var playable))
            Debug.LogWarning($"没有找到AnimationClip{clip.name}!");
        _output.SetSourcePlayable(playable);
        _graph.Play();
    }
}
