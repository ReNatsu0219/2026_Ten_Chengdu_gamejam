using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerCtrl : MonoBehaviour
{
    private PlayerStateMachine _stateMachine;

    [field: SerializeField] public PlayerConfig Config { get; private set; }

    public PlayableAnimService AnimCtrl { get; set; }
    public Rigidbody2D Rb { get; private set; }
    public Animator Animator { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }

    public Light2D Light2D { get; private set; }

    private bool canControl = true;


    #region LifeCycle Methods
    void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        this.Animator = GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Light2D = GetComponentInChildren<Light2D>();

        AnimCtrl = new PlayableAnimService(this.Animator);

        _stateMachine = new PlayerStateMachine(this);
    }
    void Update()
    {
        if (!canControl) return;

        _stateMachine?.OnUpdate(Time.deltaTime);
    }
    void OnDestroy()
    {
        AnimCtrl.Destroy();
    }
    #endregion

    public void SetControlEnabled(bool value)
    {
        canControl = value;

        if (!value && Rb != null)
        {
            Rb.velocity = Vector2.zero;
        }
    }

    public void SetVisible(bool value)
    {
        if (SpriteRenderer != null)
        {
            Color color = SpriteRenderer.color;
            color.a = value ? 1f : 0f;
            SpriteRenderer.color = color;
        }

        if (Light2D != null)
        {
            Light2D.enabled = value;
        }
    }
}
