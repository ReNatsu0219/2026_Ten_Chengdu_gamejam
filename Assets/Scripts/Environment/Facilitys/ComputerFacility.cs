using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PCStatus
{
    Disabled,
    PowerAllocationMode,
    ChargingMode,
} 

public class ComputerFacility : Facilitybase
{
    [Header("引用")]
    [SerializeField] private ComputerGhostController Ghost;
    [SerializeField] private ChargeBar chargeBar;

    [Header("能量")]
    [SerializeField] private float facilityEnergy=1f;  //夜晚间充能能量

    [Header("状态")]
    [SerializeField] private PCStatus currentStatus=PCStatus.Disabled;
    [SerializeField] private bool isCharging=false;
    [SerializeField] private bool isEnemySpawned = false;

    [Header("基础参数")]
    [SerializeField] private float maxEnergy = 1f;
    [SerializeField] private float energyChargingRate = 0.01f;
    [SerializeField] private float energyLosingRate = 0.002f;
    [SerializeField] private float deadlineEnergy = -2f;
    [SerializeField] private float baseEnemySpawnRate = 0.05f;
    [SerializeField] private int enemyCooldown = 2;

    [Header("成长参数 - 充能")]
    [SerializeField] private float powerChargeFactor = 0.001f;        // 电力对充能加成

    [Header("成长参数 - 能量消耗")]
    [SerializeField] private float dayLoseFactor = 0.0003f;            // 天数影响
    [SerializeField] private float powerLoseFactor = 0.0001f;         // 电力减缓消耗

    [Header("成长参数 - 敌人生成")]
    [SerializeField] private float daySpawnFactor = 0.02f;          // 天数影响
    [SerializeField] private float nightSpawnFactor = 0.01f;        // 夜晚时间影响
    [SerializeField] private float powerSpawnFactor;


    [Header("限制参数")]
    [SerializeField] private float minEnergy = 0f;
    [SerializeField] private float maxSpawnRate = 1f;

    [Header("交互面板")]
    [SerializeField] private CanvasGroup DaycanvasGroup;
    [SerializeField] private RectTransform DayrectTransform;

    [Header("音效")]
    [SerializeField] private AudioClip ComputerNoise;
    [SerializeField] private AudioClip ChargingNoise;

    private int enemyCooldownTimer = 0;

    public float FacilityEnergy=>facilityEnergy;
    public PCStatus CurrentStatus=>currentStatus;
    public float MaxEnergy => maxEnergy;
    public bool IsCharging => isCharging;

    protected override void ObjectAwake()
    {
        base.ObjectAwake();
        if(GameManager.Instance != null)
        {
           
        }
        isDisabled = false;
        isInteractable = true;
    }

    public void SetEnergy(int value)
    {
        facilityEnergy = value;
    }

    private void EnergyCharging(float rate)
    {
        facilityEnergy += rate;
        if(facilityEnergy>maxEnergy)facilityEnergy = maxEnergy;
    }

    private void EnergyLosing(float rate)
    {
        facilityEnergy -= rate;
    }

    private void ChangeToAllocationMode()
    {
        isInteractable = true;
        isDisabled = false;
        isCharging = false;
        currentStatus = PCStatus.PowerAllocationMode;
    }


    private void ChangeToChargingMode()
    {
        isInteractable = true;
        isDisabled = false;
        isCharging = false;
        currentStatus = PCStatus.ChargingMode;

        NightManager.Instance.OnPcEnergyChange += TickInNight;
    }

    private void ExitChargingMode()
    {
        NightManager.Instance.OnPcEnergyChange -= TickInNight;
    }

    protected override void TickInNight()
    {
        if (facilityEnergy < deadlineEnergy)
        {
            GameManager.Instance.PlayerDead("Energy ran out");
        }

        if (isCharging)
        {
            if(facilityEnergy<0f)facilityEnergy = 0f;
            EnergyCharging(GetChargingRate());
        }
        else
        {
            EnergyLosing(GetLosingRate());
        }

        if(enemyCooldownTimer>0 && !UIMgr.Instance.IsScreenPanelOpen())enemyCooldownTimer--;
    }

    private float GetChargingRate()
    {
        return energyChargingRate + allocatedPower * powerChargeFactor;
    }

    private float GetLosingRate()
    {
        float value = energyLosingRate
            + GameManager.Instance.CurrentDay * dayLoseFactor
            - allocatedPower * powerLoseFactor;

        return Mathf.Max(0f, value);
    }

    private float GetSpawnRate()
    {
        float value = baseEnemySpawnRate
            + GameManager.Instance.CurrentDay * daySpawnFactor
            + NightManager.Instance.CurrentNightTime * nightSpawnFactor
            - allocatedPower * powerSpawnFactor;

        return Mathf.Clamp(value, 0f, maxSpawnRate);
    }

    private bool ShouldSpawnEnemyOnInteract()
    {
        return Random.value < GetSpawnRate();
    
    }

    public void SetCharging(bool value)
    {
        isCharging = value;
    }

    protected override void OnInteract()
    {
        if (currentStatus == PCStatus.ChargingMode)
        {

            if (UIMgr.Instance.IsScreenPanelOpen())
            {
                SetCharging(false);
                UIMgr.Instance.HideScreenPanel();
                Ghost?.DeactivateGhost();
                isEnemySpawned = false;
                return;
            }

            if (enemyCooldownTimer == 0)
            {
                isEnemySpawned = ShouldSpawnEnemyOnInteract();
                enemyCooldownTimer = enemyCooldown;
            }

            if (isEnemySpawned)
                Ghost?.ActivateGhost();

            UIMgr.Instance.ShowScreenPanel();
            chargeBar.Initialize(facilityEnergy, maxEnergy);
        }
        else
        {
            Debug.Log("Interact with computer in day");
            HandlePanel(DaycanvasGroup, DayrectTransform);
            
        }
    }

    public override void OnDayStart()
    {
        base.OnDayStart();
        ChangeToAllocationMode();
    }

    public override void OnNightStart()
    {
        base.OnNightStart();
        ChangeToChargingMode();
        facilityEnergy = maxEnergy;

        AudioMgr.Instance.PlayDimensionalSFX(ComputerNoise, this.transform.position, true,"ComputerNoise");
    }

    public override void OnNightEnd()
    {
        base.OnNightEnd();
        ExitChargingMode();

        SetCharging(false);
        UIMgr.Instance.HideScreenPanel();
        Ghost?.DeactivateGhost();
        isEnemySpawned = false;

        AudioMgr.Instance.StopLoopSFX("ComputerNoise");
    }

    public override void OnNightClear()
    {
        base.OnNightClear();
        int RestEnergy = (int)((facilityEnergy / maxEnergy) * GameManager.Instance.DailyPower);
        GameManager.Instance.PowerSet(GameManager.Instance.CurrentPower + RestEnergy);
    }

    protected override void PossessedRoutine()
    {
        base.PossessedRoutine();

        SetCharging(false);

        if (UIMgr.Instance != null && UIMgr.Instance.IsScreenPanelOpen())
        {
            UIMgr.Instance.HideScreenPanel();
        }

        Ghost?.DeactivateGhost();
        isEnemySpawned = false;
    }

    public override void ResetFacility()
    {
        base.ResetFacility();
    }

}
