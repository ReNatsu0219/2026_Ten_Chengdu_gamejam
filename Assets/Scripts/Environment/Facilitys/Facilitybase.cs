using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Facilitybase : Interactablebase
{
    [Header("设施通用数据")]
    [SerializeField] public string facilityName; //设施名称
    [SerializeField] protected int allocatedPower;  //设施分配的电力

    public int AllocatedPower => allocatedPower;

    public virtual void SetPower(int power)
    {
        allocatedPower = power;
        OnPowerChanged();
    }

    public virtual void ResetFacility()
    {
    }

    public override void OnNightStart()
    {
        base.OnNightStart();
        ResetFacility();
    }

    protected virtual void OnPowerChanged() { }

}
