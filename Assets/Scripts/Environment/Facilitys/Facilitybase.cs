using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class Facilitybase : Interactablebase
{
    [Header("设施通用数据")]
    [SerializeField] public string facilityName; //设施名称
    [SerializeField] protected int allocatedPower;  //设施分配的电力

    [Header("附身效果")]
    [SerializeField] private Color possessedColor = Color.black;
    [SerializeField] private int possessFadeTicks = 3;   // 渐黑/恢复各需要几个tick

    private bool isPossessed = false;
    private int possessTotalTicks = 0;
    private int possessRemainTicks = 0;
    private Color originalColor;


    public int AllocatedPower => allocatedPower;

    protected override void ObjectAwake()
    {
        base.ObjectAwake();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
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
    }

    public void PowerDeAllocate()
    {
        if(GameManager.Instance.CurrentPower >=GameManager.Instance.MaxPower)
        {
            return;
        }

        GameManager.Instance.PowerUp();
        allocatedPower--;
    }

    public virtual void ResetFacility()
    {
    }

    public override void OnNightStart()
    {
        base.OnNightStart();
        ResetFacility();
    }

    protected virtual void TickInNight()
    {

    }

    protected virtual void OnPowerChanged() { }

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

        isPossessed = false;
        possessTotalTicks = 0;
        possessRemainTicks = 0;

        SetDisabled(false);
        isInteractable = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void OnPostRender()
    {
        
    }

}
