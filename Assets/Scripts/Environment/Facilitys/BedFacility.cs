using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BedFacility : Facilitybase
{
    [Header("¥≤Ã˘Õº")]
    [SerializeField] private Sprite emptyBedSprite;
    [SerializeField] private Sprite occupiedBedSprite;

    [Header("“Ù–ß")]
    [SerializeField] private AudioClip BedInteractDay;
    [SerializeField] private AudioClip BedInteractNight;

    [SerializeField] private bool isPlayerInBed = false;

    protected override void ObjectAwake()
    {
        base.ObjectAwake();
        isDisabled = false;
        isInteractable = true;

        if (spriteRenderer != null && emptyBedSprite != null)
        {
            spriteRenderer.sprite = emptyBedSprite;
        }
    }


    protected override void OnInteract()
    {
        if (currentPlayer == null) return;

        if (GameManager.Instance.IsDay)
        {
            HandleDayInteract();
        }else if (GameManager.Instance.IsNight)
        {
            HandleNightInteract();
        }
    }

    private void HandleDayInteract()
    {
        AudioMgr.Instance.PlayNormalSFX(BedInteractDay, this.transform.position,false,null,0.3f);
        GameManager.Instance.SwitchDayToNight();
    }

    private void HandleNightInteract()
    {
        AudioMgr.Instance.PlayNormalSFX(BedInteractDay, this.transform.position, false, null, 0.3f);
        if (!isPlayerInBed)
        {
            PlayerGoToBed();
        }
        else
        {
            PlayerLeaveBed();
        }
    }

    private void PlayerGoToBed()
    {
        isPlayerInBed = true;

        NightManager.Instance.SetPlayerOnBed(true);

        currentPlayer.SetVisible(false);
        currentPlayer.SetControlEnabled(false);

        if (spriteRenderer != null && occupiedBedSprite != null)
        {
            spriteRenderer.sprite = occupiedBedSprite;
        }
    }

    private void PlayerLeaveBed()
    {
        isPlayerInBed = false;

        NightManager.Instance.SetPlayerOnBed(false);

        currentPlayer.SetVisible(true);
        currentPlayer.SetControlEnabled(true);

        if (spriteRenderer != null && emptyBedSprite != null)
        {
            spriteRenderer.sprite = emptyBedSprite;
        }
    }

    protected override void PossessedRoutine()
    {
        base.PossessedRoutine();
        if(isPlayerInBed && currentPlayer != null) PlayerLeaveBed() ;
    }

    public override void OnNightEnd()
    {
        base.OnNightEnd();

        if (currentPlayer != null && isPlayerInBed)
        {
            PlayerLeaveBed();
        }
    }

    public override void ResetFacility()
    {
        base.ResetFacility();

        if(isPlayerInBed)PlayerLeaveBed();

    }
}
