using UnityEngine;

public class PlayerDeadState : PlayerStateBase
{
    private AnimationClip _die;
    public PlayerDeadState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        RegisterAnimCtrl();
    }
    public override void OnEnter()
    {
        base.OnEnter();

        _animCtrl.PlayAnimation(_die);
    }
    private void RegisterAnimCtrl()
    {
        _die = _animCfg.Die;
        _animCtrl.AddAnimationClip(_die);
    }
}
