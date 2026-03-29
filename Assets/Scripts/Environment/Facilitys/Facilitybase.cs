using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public abstract class Facilitybase : Interactablebase
{
    [Header("设施通用数据")]
    [SerializeField] public string facilityName; //设施名称
    [SerializeField] protected int allocatedPower;  //设施分配的电力

    [Header("附身效果")]
    [SerializeField] private Color possessedColor = Color.black;
    [SerializeField] private int possessFadeTicks = 3;   // 渐黑/恢复各需要几个tick

    [SerializeField] protected PlayerCtrl currentPlayer;
    [SerializeField] protected RectTransform panelRectTransform;
    [SerializeField] protected FacilityPanelUI panelUI;

    [SerializeField] private AudioClip PanelInteract;

    private bool isPossessed = false;
    private bool _isPanelOpened = false;
    private int possessTotalTicks = 0;
    private int possessRemainTicks = 0;
    private Color originalColor;

    public int AllocatedPower => allocatedPower;
    public bool IsPossessed => isPossessed;

    protected override void ObjectAwake()
    {
        base.ObjectAwake();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public override void OnPlayerEnterRange()
    {
        base.OnPlayerEnterRange();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            currentPlayer = playerObj.GetComponent<PlayerCtrl>();
        }
    }

    public virtual void SetPower(int power)
    {
        allocatedPower = power;
        OnPowerChanged();
    }

    public void PowerAllocate()
    {
        if (GameManager.Instance.CurrentPower <= 0) return;

        GameManager.Instance.PowerDown();
        allocatedPower++;
        OnPowerChanged();
    }

    public void PowerDeAllocate()
    {
        if (GameManager.Instance.CurrentPower >= GameManager.Instance.MaxPower)
        {
            return;
        }

        if (allocatedPower <= 0) return;

        GameManager.Instance.PowerUp();
        allocatedPower--;
        OnPowerChanged();
    }

    protected virtual void HandlePanel(CanvasGroup canvasGroup, RectTransform rectTransform)
    {
        panelUI?.BindFacility(this);

        AudioMgr.Instance.PlayNormalSFX(PanelInteract,this.transform.position);

        if (!_isPanelOpened)
        {
            OpenPanel(canvasGroup, rectTransform);
        }
        else
        {
            ClosePanel(canvasGroup, rectTransform);
        }
    }

    private void OpenPanel(CanvasGroup canvasGroup, RectTransform rectTransform)
    {
        UIMgr.Instance.OperationPanelFadeIn(
            canvasGroup,
            rectTransform,
            rectTransform.localPosition.x
        );
        panelUI.RefreshUI();
        UIMgr.Instance?.RefreshPowerWindow();
        currentPlayer.SetControlEnabled(false);
        _isPanelOpened = true;
    }

    private void ClosePanel(CanvasGroup canvasGroup, RectTransform rectTransform)
    {
        UIMgr.Instance.OperationPanelFadeOut(
            canvasGroup,
            rectTransform,
            rectTransform.localPosition.x
        );
        currentPlayer.SetControlEnabled(true);
        _isPanelOpened = false;
    }

    public virtual void ResetFacility()
    {
        allocatedPower = 0;
        ClearPossessState();
    }

    public override void OnNightStart()
    {
        base.OnNightStart();
    }

    public override void OnNightClear()
    {
        base.OnNightClear();
        ResetFacility();
    }

    public override void OnPlayerDead()
    {
        base.OnPlayerDead();
        ClearPossessState();
    }

    protected virtual void TickInNight()
    {
    }

    protected virtual void OnPowerChanged()
    {
        panelUI?.RefreshUI();
        UIMgr.Instance?.RefreshPowerWindow();
    }

    public void GetPossessed(int possessTicks)
    {
        if (NightManager.Instance == null) return;
        if (possessTicks <= 0) return;
        if (isPossessed) return;

        isPossessed = true;
        SetDisabled(true);
        isInteractable = false;

        possessTotalTicks = possessTicks;
        possessRemainTicks = possessTicks;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            RefreshPossessColor();
        }

        PossessedRoutine();

        NightManager.Instance.OnEnemySpawnsTick += TickPossess;
    }

    protected virtual void PossessedRoutine() { }

    protected virtual void OnPossessCleared() { }

    private void TickPossess()
    {
        if (!isPossessed) return;

        possessRemainTicks--;

        RefreshPossessColor();

        if (possessRemainTicks <= 0)
        {
            ClearPossessState();
        }
    }

    private void RefreshPossessColor()
    {
        if (spriteRenderer == null) return;

        int fadeTicks = Mathf.Max(1, possessFadeTicks);
        int elapsedTicks = possessTotalTicks - possessRemainTicks;

        if (elapsedTicks <= fadeTicks)
        {
            float t = Mathf.Clamp01((float)elapsedTicks / fadeTicks);
            spriteRenderer.color = Color.Lerp(originalColor, possessedColor, t);
            return;
        }

        if (possessRemainTicks <= fadeTicks)
        {
            float t = Mathf.Clamp01((float)possessRemainTicks / fadeTicks);
            spriteRenderer.color = Color.Lerp(originalColor, possessedColor, t);
            return;
        }

        spriteRenderer.color = possessedColor;
    }

    private void ClearPossessState()
    {
        if (NightManager.Instance != null)
        {
            NightManager.Instance.OnEnemySpawnsTick -= TickPossess;
        }

        bool wasPossessed = isPossessed;

        isPossessed = false;
        possessTotalTicks = 0;
        possessRemainTicks = 0;

        SetDisabled(false);
        isInteractable = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        if (wasPossessed)
        {
            OnPossessCleared();
        }
    }

    private void OnPostRender()
    {
    }
}