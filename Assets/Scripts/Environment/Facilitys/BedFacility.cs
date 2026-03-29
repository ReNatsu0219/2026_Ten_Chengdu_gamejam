using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedFacility : Facilitybase
{
    [Header("´²ÌùÍ¼")]
    [SerializeField] private Sprite emptyBedSprite;
    [SerializeField] private Sprite occupiedBedSprite;

    [SerializeField] private bool isPlayerInBed = false;
    [SerializeField] private PlayerCtrl currentPlayer;

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

    public override void OnPlayerEnterRange()
    {
        base.OnPlayerEnterRange();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            currentPlayer = playerObj.GetComponent<PlayerCtrl>();
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
        GameManager.Instance.EndDay();
        GameManager.Instance.StartNight();
    }

    private void HandleNightInteract()
    {
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

        isPlayerInBed = false;

        if (spriteRenderer != null && emptyBedSprite != null)
        {
            spriteRenderer.sprite = emptyBedSprite;
        }
    }
}
