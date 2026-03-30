using System;
using UnityEngine;

[Serializable]
public class PlayerAnimConfig
{
    [field: SerializeField] public AnimationClip IdleAnim_Front { get; private set; }
    [field: SerializeField] public AnimationClip IdleAnim_Back { get; private set; }
    [field: SerializeField] public AnimationClip IdleAnim_Left { get; private set; }
    [field: SerializeField] public AnimationClip IdleAnim_Right { get; private set; }
    [field: SerializeField] public AnimationClip MoveAnim_Front { get; private set; }
    [field: SerializeField] public AnimationClip MoveAnim_Back { get; private set; }
    [field: SerializeField] public AnimationClip MoveAnim_Left { get; private set; }
    [field: SerializeField] public AnimationClip MoveAnim_Right { get; private set; }
    [field: SerializeField] public AnimationClip Die { get; private set; }
}
