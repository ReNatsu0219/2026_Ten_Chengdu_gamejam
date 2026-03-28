using System;
using UnityEngine;

[Serializable]
public class PlayerMovingConfig
{
    [field: SerializeField] public float AclTime { get; private set; } = 0.5f;
}
