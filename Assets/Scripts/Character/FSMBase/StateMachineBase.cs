public abstract class StateMachineBase
{
    private IState _currentState;

    public void ChangeState(IState newState)
    {
        _currentState?.OnExit();
        _currentState = newState;
        _currentState?.OnEnter();
    }
    public void OnUpdate(float deltaTime)
    {
        _currentState?.OnUpdate(deltaTime);
    }
}
