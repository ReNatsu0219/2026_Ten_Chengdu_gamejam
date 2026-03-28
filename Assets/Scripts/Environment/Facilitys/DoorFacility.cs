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
    [Header("参数")]
    [SerializeField] private int konckerCooldownTimer = 0;  //敲门鬼冷却计时器
    [SerializeField] private int konckerBaseCooldown = 40; //敲门鬼基础冷却
    [SerializeField] private int konckerCooldownRange = 5;   //冷却浮动范围
    [SerializeField] private int currentCooldown = 0;   //当前进行的冷却时间
    [SerializeField] private float knockDuration = 3f; // 敲门持续时间
    [SerializeField] private float knockerStayDuration = 3f;    //敲门鬼停留时间

    [Header("状态")]
    [SerializeField] private DoorStatus currentStatus = DoorStatus.Disabled;

    [Header("门的贴图")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite openedSprite;

    public DoorStatus CurrentStatus => currentStatus;

    protected override void ObjectAwake()
    {
        base.ObjectAwake();
        spriteRenderer.sprite = idleSprite;
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
        if(currentStatus == DoorStatus.Disabled) return;

        currentStatus = DoorStatus.Disabled;
        konckerCooldownTimer = 0;
        currentCooldown = konckerBaseCooldown;
        spriteRenderer.sprite = idleSprite;
    }

    private void ChangeToKoncking()
    {
        if (currentStatus != DoorStatus.Idle) return;

        currentStatus = DoorStatus.Knocking;
        StartCoroutine(KnockRoutine());
    }

    private IEnumerator KnockRoutine()
    {
        yield return new WaitForSeconds(knockDuration);

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
        if(currentStatus != DoorStatus.Knocking) return;

        currentStatus = DoorStatus.Opened;
        spriteRenderer.sprite = openedSprite;
        if (!NightManager.Instance.IsPlayerOnBed)
        {
            GameManager.Instance.PlayerDead("Killed by Knocker");
            return;
        }
        StartCoroutine(CloseRoutine()); 
        
    }

    private int GetFristCooldown()
    {
        int baseValue = konckerBaseCooldown - GameManager.Instance.CurrentDay * 2;

        int randomOffset = Random.Range(-konckerCooldownRange, konckerCooldownRange + 1);

        int result = baseValue + randomOffset;

        return Mathf.Max(5, result);
    }

    private int GetNewCooldown()
    {
        int baseValue = konckerBaseCooldown - GameManager.Instance.CurrentDay * 2 - (int)(NightManager.Instance.CurrentNightTime * 0.5f);

        int randomOffset = Random.Range(-konckerCooldownRange, konckerCooldownRange + 1);

        int result = baseValue + randomOffset;

        return Mathf.Max(5, result);
    }

    private void TickInNight()
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

    protected override void OnInteract() { }

    
}
