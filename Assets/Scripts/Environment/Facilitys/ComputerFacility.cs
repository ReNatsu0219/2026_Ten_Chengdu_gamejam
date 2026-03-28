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

    [Header("参数")]
    [SerializeField] private float maxEnergy = 1f;
    [SerializeField] private float energyChargingRate = 0.1f;
    [SerializeField] private float energyLosingRate = 0.01f;
    [SerializeField] private float deadlineEnergy = -3f;
                                  
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

    private void TickInNight()
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
    }

    private float GetChargingRate()
    {
        return energyChargingRate + (float)allocatedPower * 0.01f;  //充能计算公式（待改）
    }

    private float GetLosingRate()
    {
        return energyLosingRate + GameManager.Instance.CurrentDay * 0.02f - (float)allocatedPower * 0.001f;    //能量损失计算公式（待改）
    }

    protected override void OnInteract()
    {

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
    }

}
