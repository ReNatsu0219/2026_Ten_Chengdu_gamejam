using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactablebase : MonoBehaviour
{
    [Header("交互设置")]
    public string interactPrompt = "按E交互";
    public bool isInteractable = false;

    [Header("状态设置")]
    public bool isPlayerInRange = false;
    public bool isDisabled = false;

    [Header("碰撞器")]
    public Collider2D interactionCollider;

    [Header("渲染器")]
    public SpriteRenderer spriteRenderer;

    private bool hasSubscribed = false;

    protected virtual void ObjectAwake() { }

    public virtual void OnPlayerEnterRange()
    {
        isPlayerInRange = true;
    }

    public virtual void OnPlayerExitRange()
    {
        isPlayerInRange = false;
    }

    public void TryInteract()
    {
        if (!isInteractable || isDisabled || !isPlayerInRange) return;
        OnInteract();
    }

    protected abstract void OnInteract();

    public virtual void OnNightStart() { }

    public virtual void OnNightEnd()
    {
        Debug.Log("收到夜晚结束的事件 " + this.name);
    }

    public virtual void OnDayStart() { }

    public virtual void OnDayEnd() { }

    public virtual void OnNightClear() { }

    public virtual void OnPlayerDead() {
        isInteractable = false;
    }

    public virtual void SetDisabled(bool value)
    {
        isDisabled = value;
    }

    public virtual string GetPromptText()
    {
        if (isDisabled) return "现在无法使用";
        return interactPrompt;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnterRange();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerExitRange();
        }
    }

    protected virtual void Awake()
    {
        if (interactionCollider == null)
        {
            interactionCollider = GetComponent<Collider2D>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        isInteractable = true;
        isDisabled = false;

        ObjectAwake();
    }

    protected virtual IEnumerator Start()
    {
        while (GameManager.Instance == null)
        {
            yield return null;
        }

        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (hasSubscribed) return;
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnNightStarted += OnNightStart;
        GameManager.Instance.OnNightEnded += OnNightEnd;
        GameManager.Instance.OnDayStarted += OnDayStart;
        GameManager.Instance.OnDayEnded += OnDayEnd;
        GameManager.Instance.OnNightClear += OnNightClear;
        GameManager.Instance.OnPlayerDead += OnPlayerDead;

        hasSubscribed = true;
        Debug.Log($"{name} 已订阅昼夜切换事件");
    }

    private void Update()
    {
        if (InputMgr.Instance != null && InputMgr.Instance.InteractPressed)
        {
            TryInteract();
        }
    }

    protected virtual void OnDisable()
    {
        UnsubscribeEvents();
    }

    protected virtual void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void UnsubscribeEvents()
    {
        if (!hasSubscribed) return;
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnNightStarted -= OnNightStart;
        GameManager.Instance.OnNightEnded -= OnNightEnd;
        GameManager.Instance.OnDayStarted -= OnDayStart;
        GameManager.Instance.OnDayEnded -= OnDayEnd;
        GameManager.Instance.OnNightClear -= OnNightClear;
        GameManager.Instance.OnPlayerDead -= OnPlayerDead;

        hasSubscribed = false;
    }
}