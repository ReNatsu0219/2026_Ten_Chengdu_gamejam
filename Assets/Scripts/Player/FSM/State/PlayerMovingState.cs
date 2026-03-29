using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovingState : PlayerStateBase
{
    private AnimationClip _animFront;
    private AnimationClip _animBack;
    private AnimationClip _animLeft;
    private AnimationClip _animRight;
    public PlayerMovingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        RegisterAnimCtrl();
    }

    public override void OnEnter()
    {
        base.OnEnter();

        AddCallbacks();
    }

    public override void OnExit()
    {
        RemoveCallbacks();
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);

        PlayAnimOnFace();
        PlayStepSFX(deltaTime);
    }

    private void RegisterAnimCtrl()
    {
        _animFront = _animCfg.MoveAnim_Front;
        _animBack = _animCfg.MoveAnim_Back;
        _animLeft = _animCfg.MoveAnim_Left;
        _animRight = _animCfg.MoveAnim_Right;
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
    private void PlayStepSFX(float deltaTime)
    {
        _reusableData.StepTimer += deltaTime;
        if (_reusableData.StepTimer >= _audioCfg.StepInterval)
        {
            AudioMgr.Instance.PlayNormalSFX(_audioCfg.Step, _stateMachine.Player.transform.position);
            _reusableData.StepTimer = 0f;
        }
    }

    #region Reusable Methods
    public override void AddCallbacks()
    {
        base.AddCallbacks();

        _inputService.InputMap.Gameplay.Movement.canceled += OnMovementCanceled;
    }
    public override void RemoveCallbacks()
    {
        base.RemoveCallbacks();

        _inputService.InputMap.Gameplay.Movement.canceled -= OnMovementCanceled;
    }

    private void OnMovementCanceled(InputAction.CallbackContext _)
    {
        _stateMachine.ChangeState(_stateMachine.IdlingState);
    }
    #endregion
}
