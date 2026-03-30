using UnityEngine;
using UnityEngine.EventSystems;

public class ChargeButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("ÒýÓÃ")]
    [SerializeField] private ComputerFacility computer;
    [SerializeField] private ChargeBar chargeBar;

    private bool isHolding = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        computer?.SetCharging(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopHolding();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopHolding();
    }

    private void OnDisable()
    {
        StopHolding();
    }

    private void Update()
    {
        chargeBar.UpdateStats(computer.FacilityEnergy, computer.MaxEnergy);

        if (!isHolding) return;
        if (computer == null || chargeBar == null) return;
    }

    private void StopHolding()
    {
        isHolding = false;
        computer?.SetCharging(false);
    }
}