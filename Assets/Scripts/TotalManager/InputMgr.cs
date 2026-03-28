using UnityEngine;

public class InputMgr : MonoSingleton<InputMgr>
{
    public InputMap InputMap { get; private set; }

    public Vector2 MoveDir
    {
        get => InputMap.Gameplay.Movement.ReadValue<Vector2>();
    }

    protected override void Awake()
    {
        base.Awake();

        InputMap = new InputMap();
        InputMap.Enable();
    }
    void OnDestroy()
    {
        InputMap.Disable();
    }
}
