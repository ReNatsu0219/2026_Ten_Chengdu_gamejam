using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactablebase : MonoBehaviour
{
    [Header("交互设置")]
    public string interactPrompt = "按E交互";   //物品可交互时显示的文字(可能会用到)
    public bool isInteractable = false; //物品是否可交互

    [Header("状态设置")]
    public bool isPlayerInRange = false;    //玩家是否处于范围内
    public bool isDisabled = false; //物品是否被禁用

    [Header("碰撞器")]
    public Collider2D interactionCollider;

    [Header("渲染器")]
    public SpriteRenderer spriteRenderer;
     
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

    public virtual void OnNightEnd() { }

    public virtual void OnDayStart() { }

    public virtual void OnDayEnd() { }

    public virtual void SetDisabled(bool value)
    {
        isDisabled = value;
    }

    public virtual string GetPromptText()
    {
        if (isDisabled) return"现在无法使用";
        return interactPrompt;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerInteract"))
        {
            OnPlayerEnterRange();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Playernteract"))
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
        if (spriteRenderer==null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        ObjectAwake();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNightStarted += OnNightStart;
            GameManager.Instance.OnNightEnded += OnNightEnd;
            GameManager.Instance.OnDayStarted += OnDayStart;
            GameManager.Instance.OnDayEnded += OnDayEnd;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNightStarted -= OnNightStart;
            GameManager.Instance.OnNightEnded -= OnNightEnd;
            GameManager.Instance.OnDayStarted -= OnDayStart;
            GameManager.Instance.OnDayEnded -= OnDayEnd;
        }
    }
}
