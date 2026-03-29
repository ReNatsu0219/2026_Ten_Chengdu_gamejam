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
    [SerializeField] private int enemyCooldown = 10;

    [Header("成长参数 - 充能")]
    [SerializeField] private float powerChargeFactor = 0.001f;        // 电力对充能加成

    [Header("成长参数 - 能量消耗")]
    [SerializeField] private float dayLoseFactor = 0.0003f;            // 天数影响
    [SerializeField] private float powerLoseFactor = 0.0001f;         // 电力减缓消耗

    [Header("成长参数 - 敌人生成")]
    [SerializeField] private float daySpawnFactor = 0.001f;          // 天数影响
    [SerializeField] private float nightSpawnFactor = 0.001f;        // 夜晚时间影响

    [Header("限制参数")]
    [SerializeField] private float minEnergy = 0f;
    [SerializeField] private float maxSpawnRate = 1f;

    [Header("交互面板")]
    [SerializeField] private CanvasGroup DaycanvasGroup;
    [SerializeField] private RectTransform DayrectTransform;

    [Header("音效")]
    [SerializeField] private AudioClip ComputerNoise;
    [SerializeField] private AudioClip ChargingNoise;

    private int enemtCooldownTimer = 0;

    public float FacilityEnergy=>facilityEnergy;
    public PCStatus CurrentStatus=>currentStatus;
    private float MaxEnergy => maxEnergy;
    private bool IsCharging => isCharging;

    protected override void ObjectAwake()
    {
        base.ObjectAwake();
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnDayStarted += OnDayStart;
            GameManager.Instance.OnDayEnded += OnDayEnd;

            GameManager.Instance.OnNightStarted += OnNightStart;
            GameManager.Instance.OnNightEnded += OnNightEnd;
           
        }
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

        if(enemtCooldownTimer>0)enemtCooldownTimer--;
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
            + NightManager.Instance.CurrentNightTime * nightSpawnFactor;

        return Mathf.Clamp(value, 0f, maxSpawnRate);
    }

    private bool ShouldSpawnEnemyOnInteract()
    {
        return Random.value < GetSpawnRate();
    
    }


    protected override void OnInteract()
    {
        if (currentStatus == PCStatus.ChargingMode)
        {
            /*
            if (enemtCooldownTimer == 0)
            {
                isEnemySpawned = true ? ShouldSpawnEnemyOnInteract() : false;
                enemtCooldownTimer = enemyCooldown;
            }

            if (isEnemySpawned)
            {
                //OpenPanel(dangerPanelCanvasGroup, dangerPanelRectTransform);
            }
            else
            {
                //OpenPanel(nightPanelCanvasGroup, nightPanelRectTransform);
            }
            */

            EnergyCharging(GetChargingRate());
            AudioMgr.Instance.PlayDimensionalSFX(ChargingNoise,this.transform.position);

        }else if(currentStatus == PCStatus.PowerAllocationMode)
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

        AudioMgr.Instance.StopLoopSFX("ComputerNoise");
    }

    public override void OnNightClear()
    {
        base.OnNightClear();
        int RestEnergy = (int)((facilityEnergy / maxEnergy) * GameManager.Instance.DailyPower);
        GameManager.Instance.PowerSet(GameManager.Instance.CurrentPower + RestEnergy);
    }

    public override void ResetFacility()
    {
        base.ResetFacility();
    }

}
