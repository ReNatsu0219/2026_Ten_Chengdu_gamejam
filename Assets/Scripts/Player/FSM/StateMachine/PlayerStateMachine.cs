public class PlayerStateMachine : StateMachineBase
{
    public PlayerCtrl Player { get; private set; }
    public PlayerReusableData ReusableData { get; private set; }

    public PlayerIdlingState IdlingState { get; private set; }
    public PlayerMovingState MovingState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }
    public PlayerStateMachine(PlayerCtrl player)
    {
        Player = player;
        ReusableData = new PlayerReusableData();

        IdlingState = new PlayerIdlingState(this);
        MovingState = new PlayerMovingState(this);
        DeadState = new PlayerDeadState(this);

        Init();
    }

    private void Init()
    {
        ChangeState(IdlingState);
    }
}
