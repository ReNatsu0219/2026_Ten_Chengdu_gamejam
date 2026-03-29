using System;
using UnityEngine;

public class PlayerIdlingState : PlayerStateBase
{
    private AnimationClip _animFront;
    private AnimationClip _animBack;
    private AnimationClip _animLeft;
    private AnimationClip _animRight;
    public PlayerIdlingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        RegisterAnimCtrl();
    }

    public override void OnEnter()
    {
        base.OnEnter();

        AddCallbacks();
        PlayAnimOnFace();
    }

    public override void OnExit()
    {
        RemoveCallbacks();
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
    }

    private void RegisterAnimCtrl()
    {
        _animFront = _animCfg.IdleAnim_Front;
        _animBack = _animCfg.IdleAnim_Back;
        _animLeft = _animCfg.IdleAnim_Left;
        _animRight = _animCfg.IdleAnim_Right;
        _animCtrl.AddAnimationClip(_animFront);
        _animCtrl.AddAnimationClip(_animBack);
        _animCtrl.AddAnimationClip(_animLeft);
        _animCtrl.AddAnimationClip(_animRight);
    }
    private void PlayAnimOnFace()
    {
        switch (_reusableData.PlayerFace)
        {
            case ECharacterFace.Front:
                _animCtrl.PlayAnimation(_animFront);
                break;
            case ECharacterFace.Back:
                _animCtrl.PlayAnimation(_animBack);
                break;
            case ECharacterFace.Left:
                _animCtrl.PlayAnimation(_animLeft);
                break;
            case ECharacterFace.Right:
                _animCtrl.PlayAnimation(_animRight);
                break;
        }
    }

    #region Reusable Methods
    public override void AddCallbacks()
    {
        base.AddCallbacks();

        _inputService.InputMap.Gameplay.Movement.started += OnMovementStarted;
    }
    public override void RemoveCallbacks()
    {
        base.RemoveCallbacks();

        _inputService.InputMap.Gameplay.Movement.started -= OnMovementStarted;
    }
    #endregion
}
