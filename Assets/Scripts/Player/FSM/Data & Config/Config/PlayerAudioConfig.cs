using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerAudioConfig
{
    [field: SerializeField] public List<AudioClip> Step { get; private set; }
    [field: SerializeField] public float StepInterval { get; private set; }
}
