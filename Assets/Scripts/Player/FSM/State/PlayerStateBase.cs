using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;

public abstract class PlayerStateBase : IState
{
    protected PlayerStateMachine _stateMachine;
    protected PlayerReusableData _reusableData;

    protected InputMgr _inputService;
    protected PlayableAnimService _animCtrl;
    protected PlayerConfig _cfg;
    protected PlayerAnimConfig _animCfg;
    protected PlayerAudioConfig _audioCfg;
    public PlayerStateBase(PlayerStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _reusableData = stateMachine.ReusableData;
        _animCtrl = stateMachine.Player.AnimCtrl;
        _cfg = stateMachine.Player.Config;

        _animCfg = _cfg.AnimConfig;
        _audioCfg = _cfg.AudioConfig;

        _inputService = InputMgr.Instance;
    }

    public virtual void OnEnter()
    {
        Debug.Log($"状态切换：{this.GetType().Name}");
    }

    public virtual void OnExit()
    {
    }

    public virtual void OnUpdate(float deltaTime)
    {
        UpdatePlayerFace();
        Move();
    }

    #region Reusable Methods

    #region Callback
    public virtual void AddCallbacks()
    {
    }
    public virtual void RemoveCallbacks()
    {
    }

    #region Input Callbacks
    public virtual void OnMovementStarted(InputAction.CallbackContext _)
    {
        _stateMachine.ChangeState(_stateMachine.MovingState);
    }
    #endregion
    #endregion

    protected virtual void Move()
    {
        Vector2 currentVelocity = _stateMachine.Player.Rb.velocity;
        Vector2 moveDirection = _reusableData.InputDir;
        float moveSpeed = _cfg.BaseSpeed * _reusableData.SpeedMult;

        _stateMachine.Player.Rb.AddForce(moveSpeed * moveDirection - currentVelocity, ForceMode2D.Force);
    }
    private void UpdatePlayerFace()
    {
        if (_inputService.MoveDir == Vector2.zero)
        {
            _reusableData.InputDir = Vector2.zero;
            return;
        }

        if (_inputService.MoveDir == _reusableData.InputDir) return;

        float deltaAngle = Vector2.Angle(_reusableData.InputDir, _inputService.MoveDir);
        if (deltaAngle >= 90f)
        {
            if (_inputService.MoveDir.y != 0)
            {
                if (_inputService.MoveDir.y > 0)
                    _reusableData.PlayerFace = ECharacterFace.Back;
                else if (_inputService.MoveDir.y < 0)
                    _reusableData.PlayerFace = ECharacterFace.Front;
            }
            else
            {
                if (_inputService.MoveDir.x > 0)
                    _reusableData.PlayerFace = ECharacterFace.Right;
                else if (_inputService.MoveDir.x < 0)
                    _reusableData.PlayerFace = ECharacterFace.Left;
            }
        }
        else
        {
            if (_reusableData.InputDir.x == 0 || _inputService.MoveDir.x == 0)
            {
                if (_inputService.MoveDir.x == 0)
                    _reusableData.PlayerFace = _inputService.MoveDir.y > 0 ? ECharacterFace.Back : ECharacterFace.Front;
                else
                    _reusableData.PlayerFace = _inputService.MoveDir.x > 0 ? ECharacterFace.Right : ECharacterFace.Left;
            }
            else if (_reusableData.InputDir.y == 0 || _inputService.MoveDir.y == 0)
            {
                if (_inputService.MoveDir.y == 0)
                    _reusableData.PlayerFace = _inputService.MoveDir.x > 0 ? ECharacterFace.Right : ECharacterFace.Left;
                else
                    _reusableData.PlayerFace = _inputService.MoveDir.y > 0 ? ECharacterFace.Back : ECharacterFace.Front;
            }
        }
        _reusableData.InputDir = _inputService.MoveDir;
    }


    #endregion
}
