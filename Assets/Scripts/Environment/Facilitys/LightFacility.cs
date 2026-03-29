using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFacility : Facilitybase
{
    [Header("交互面板")]
    [SerializeField] private CanvasGroup LightcanvasGroup;
    [SerializeField] private RectTransform LightrectTransform;

    protected override void ObjectAwake()
    {
        base.ObjectAwake();
    }

    public override void OnDayStart()
    {
        base.OnDayStart();
        Debug.Log("白天开始，灯已启用");
        isInteractable = true;
        isDisabled = false;
    }

    public override void OnDayEnd()
    {
        base.OnDayEnd();
        isInteractable = false;
        isDisabled = true;
    }

    protected override void OnInteract()
    {
        HandlePanel(LightcanvasGroup, LightrectTransform);
    }

    protected override void PossessedRoutine()
    {
        base.PossessedRoutine();

        if (LightManager.Instance != null)
        {
            // 不再直接改基础亮度，改为进入“和闪烁暗态相同”的暗状态
            LightManager.Instance.SetPossessedDark(true);
        }
    }

    protected override void OnPossessCleared()
    {
        base.OnPossessCleared();

        if (LightManager.Instance != null)
        {
            LightManager.Instance.SetPossessedDark(false);
        }
    }

    public override void ResetFacility()
    {
        base.ResetFacility();

        if (GameManager.Instance != null && GameManager.Instance.IsDay)
        {
            LightManager.Instance?.ApplyDayLightNow();
        }
        else if (GameManager.Instance != null && GameManager.Instance.IsNight)
        {
            LightManager.Instance?.ApplyNightLightNow();
        }
    }
}