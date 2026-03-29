using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PCStatus
{
    Disabled,
    PowerAllocationMode,
    ChargingMode,
    Possessed
} 

public class ComputerFacility : Facilitybase
{
    [Header("能量")]
    [SerializeField] private float facilityEnergy=1f;  //夜晚间充能能量

    [Header("状态")]
    [SerializeField] private PCStatus currentStatus=PCStatus.Disabled;
    [SerializeField] private bool isCharging=false;
    [SerializeField] private bool isEnemySpawned = false;

    [Header("参数")]
    [SerializeField] private float maxEnergy = 1f;
    [SerializeField] private float energyChargingRate = 0.1f;
    [SerializeField] private float energyLosingRate = 0.01f;
    [SerializeField] private float deadlineEnergy = -3f;
    [SerializeField] private float baseEnemySpawnRate=0.05f;
    [SerializeField] private int enemyCooldown = 10;

    private int enemtCooldownTimer = 0;
                                  
    public float FacilityEnergy=>facilityEnergy;
    public PCStatus CurrentStatus=>currentStatus;
    private float MaxEnergy => maxEnergy;
    private bool IsCharging => isCharging;

    public void SetEnergy(int value)
    {
        facilityEnergy = value;
    }

    private void EnergyCharging(float rate)
    {
        facilityEnergy += rate;
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
        return energyChargingRate + (float)allocatedPower * 0.01f;  //充能计算公式（待改）
    }

    private float GetLosingRate()
    {
        return energyLosingRate + GameManager.Instance.CurrentDay * 0.02f - (float)allocatedPower * 0.001f;    //能量损失计算公式（待改）
    }

    private float GetSpawnRate()
    {
        return baseEnemySpawnRate + (float)(GameManager.Instance.CurrentDay * 0.001 + NightManager.Instance.CurrentNightTime * 0.001);
    }

    private bool ShouldSpawnEnemyOnInteract()
    {
        return Random.value < GetSpawnRate();
    
    }


    protected override void OnInteract()
    {
        if (currentStatus == PCStatus.ChargingMode)
        {
            if (enemtCooldownTimer == 0)
            {
                isEnemySpawned = true ? ShouldSpawnEnemyOnInteract() : false;
                enemtCooldownTimer = enemyCooldown;
            }

            if (isEnemySpawned)
            {
                //此处写夜晚打开电脑鬼出现UI的代码
            }
            else
            {
                //此处写夜晚打开正常UI的代码
            }
        }else if(currentStatus == PCStatus.PowerAllocationMode)
        {
            //此处写白天打开UI的代码
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
    }

    public override void OnNightEnd()
    {
        base.OnNightEnd();
        ExitChargingMode();
    }

    public override void ResetFacility()
    {
        base.ResetFacility();
        facilityEnergy = maxEnergy;
        isEnemySpawned = false;
    }

}
