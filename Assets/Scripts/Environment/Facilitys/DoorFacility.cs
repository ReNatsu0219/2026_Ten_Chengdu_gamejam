using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorStatus
{
    Disabled,
    Idle,
    Knocking,
    Opened
}

public class DoorFacility : Facilitybase
{
    [Header("冷却相关")]
    [SerializeField] private int konckerCooldownTimer = 0;
    [SerializeField] private int konckerBaseCooldown = 45;
    [SerializeField] private int konckerCooldownRange = 20;
    [SerializeField] private int currentCooldown = 0;

    [Header("敲门时长")]
    [SerializeField] private float knockBaseDuration = 6f;
    [SerializeField] private float currentKnockDuration = 6f;

    [Header("停留时间")]
    [SerializeField] private float knockerStayDuration = 3f;

    [Header("成长参数 - 冷却")]
    [SerializeField] private float dayCooldownFactor = 4f;          // 天数影响
    [SerializeField] private float nightCooldownFactor = 10f;      // 夜晚进度影响
    [SerializeField] private float powerCooldownFactor = 1f;        // 电力影响

    [Header("成长参数 - 敲门时长")]
    [SerializeField] private float dayDurationFactor = 0.3f;        // 天数影响
    [SerializeField] private float nightDurationFactor = 0.6f;     // 夜晚进度影响
    [SerializeField] private float powerDurationFactor = 0.1f;      // 电力影响

    [SerializeField] private int minCooldown = 10;
    [SerializeField] private float minKnockDuration = 4f;

    [Header("状态")]
    [SerializeField] private DoorStatus currentStatus = DoorStatus.Disabled;

    [Header("门的贴图")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite openedSprite;

    [Header("交互面板")]
    [SerializeField] private CanvasGroup DoorcanvasGroup;
    [SerializeField] private RectTransform DoorrectTransform;

    [Header("音效")]
    [SerializeField] private AudioClip KnockSFX;
    [SerializeField] private AudioClip OpenSFX;

    public DoorStatus CurrentStatus => currentStatus;

    protected override void ObjectAwake()
    {
        base.ObjectAwake();
        spriteRenderer.sprite = idleSprite;
    }

    public override void OnDayStart()
    {
        base.OnDayStart();
        isInteractable = true;
        isDisabled = false;
    }

    public override void OnDayEnd()
    {
        base.OnDayEnd();
        isInteractable = false;
    }

    protected override void OnInteract()
    {
        HandlePanel(DoorcanvasGroup, DoorrectTransform);
    }

    public override void OnNightStart()
    {
        base.OnNightStart();
        Activate();
    }

    public override void OnNightEnd()
    {
        base.OnNightEnd();
        DeActivate();
    }

    private void Activate()
    {
        NightManager.Instance.OnEnemySpawnsTick += TickInNight;
        konckerCooldownTimer = 0;
        currentCooldown = GetFristCooldown();
        currentKnockDuration = GetCurrentKnockDuration();

        ChangeToIdle();
    }

    private void DeActivate()
    {
        NightManager.Instance.OnEnemySpawnsTick -= TickInNight;
        StopAllCoroutines();
        ChangeToDisabled();
        spriteRenderer.sprite = idleSprite;
    }

    private void ChangeToIdle()
    {
        if (currentStatus == DoorStatus.Idle) return;

        currentStatus = DoorStatus.Idle;
        spriteRenderer.sprite = idleSprite;
    }

    private void ChangeToDisabled()
    {
        if (currentStatus == DoorStatus.Disabled) return;

        currentStatus = DoorStatus.Disabled;
        konckerCooldownTimer = 0;
        currentCooldown = konckerBaseCooldown;
        currentKnockDuration = knockBaseDuration;
        spriteRenderer.sprite = idleSprite;
    }

    private void ChangeToKoncking()
    {
        if (currentStatus != DoorStatus.Idle) return;

        currentStatus = DoorStatus.Knocking;
        currentKnockDuration = GetCurrentKnockDuration();
        AudioMgr.Instance.PlayDimensionalSFX(KnockSFX, this.transform.position, false, null, 1, currentKnockDuration);
        StartCoroutine(KnockRoutine());
    }

    private IEnumerator KnockRoutine()
    {
        yield return new WaitForSeconds(currentKnockDuration);
        ChangeToOpened();
    }

    private IEnumerator CloseRoutine()
    {
        float timer = 0f;

        while (timer < knockerStayDuration)
        {
            if (!NightManager.Instance.IsPlayerOnBed)
            {
                GameManager.Instance.PlayerDead("Killed by Knocker");
                yield break;
            }

            yield return null;
            timer += Time.deltaTime;
        }

        ChangeToIdle();
    }

    private void ChangeToOpened()
    {
        if (currentStatus != DoorStatus.Knocking) return;

        currentStatus = DoorStatus.Opened;
        spriteRenderer.sprite = openedSprite;

        AudioMgr.Instance.PlayDimensionalSFX(OpenSFX, this.transform.position);

        if (!NightManager.Instance.IsPlayerOnBed)
        {
            GameManager.Instance.PlayerDead("Killed by Knocker");
            return;
        }

        StartCoroutine(CloseRoutine());
    }

    private int GetFristCooldown()
    {
        int dayValue = (int)(GameManager.Instance.CurrentDay * dayCooldownFactor);
        int powerValue = (int)(allocatedPower * powerCooldownFactor);

        int baseValue = konckerBaseCooldown - dayValue + powerValue;

        int randomOffset = Random.Range(-konckerCooldownRange, konckerCooldownRange + 1);

        int result = baseValue + randomOffset;

        return Mathf.Max(minCooldown, result);
    }

    private int GetNewCooldown()
    {
        int dayValue = (int)(GameManager.Instance.CurrentDay * dayCooldownFactor);
        float nightValue = NightManager.Instance.CurrentNightTime / NightManager.Instance.NightDuration;
        int powerValue = (int)(allocatedPower * powerCooldownFactor);

        int baseValue = konckerBaseCooldown - dayValue - (int)(nightValue * nightCooldownFactor) + powerValue;

        int randomOffset = Random.Range(-konckerCooldownRange, konckerCooldownRange + 1);

        int result = baseValue + randomOffset;

        return Mathf.Max(minCooldown, result);
    }

    private float GetCurrentKnockDuration()
    {
        float dayValue = GameManager.Instance.CurrentDay * dayDurationFactor;
        float nightValue = NightManager.Instance.CurrentNightTime / NightManager.Instance.NightDuration;
        float powerValue = allocatedPower * powerDurationFactor;

        float result = knockBaseDuration - dayValue - nightValue * nightDurationFactor + powerValue;

        return Mathf.Max(minKnockDuration, result);
    }
    protected override void TickInNight()
    {
        if (currentStatus != DoorStatus.Idle) return;

        konckerCooldownTimer++;

        if (konckerCooldownTimer >= currentCooldown)
        {
            konckerCooldownTimer = 0;
            currentCooldown = GetNewCooldown();
            ChangeToKoncking();
        }
    }
}