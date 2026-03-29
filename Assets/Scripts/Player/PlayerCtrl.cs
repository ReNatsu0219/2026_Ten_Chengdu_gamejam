using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    private PlayerStateMachine _stateMachine;

    [field: SerializeField] public PlayerConfig Config { get; private set; }

    public PlayableAnimService AnimCtrl { get; set; }
    public Rigidbody2D Rb { get; private set; }
    public Animator Animator { get; private set; }

    #region LifeCycle Methods
    void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        this.Animator = GetComponent<Animator>();
        AnimCtrl = new PlayableAnimService(this.Animator);

        _stateMachine = new PlayerStateMachine(this);
    }
    void Update()
    {
        _stateMachine?.OnUpdate(Time.deltaTime);
    }
    void OnDestroy()
    {
        AnimCtrl.Destroy();
    }
    #endregion
}
