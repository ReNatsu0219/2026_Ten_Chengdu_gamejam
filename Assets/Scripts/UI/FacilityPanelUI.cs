using TMPro;
using UnityEngine;

public class FacilityPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text powerText;
    private Facilitybase currentFacility;

    [Header("“Ù–ß")]
    [SerializeField] private AudioClip PowerAllocate;

    public void BindFacility(Facilitybase facility)
    {
        currentFacility = facility;
        RefreshUI();
    }

    public void OnClickAddPower()
    {
        if (currentFacility == null) return;
        Debug.Log("AddButton pressed");
        currentFacility.PowerAllocate();
        AudioMgr.Instance.PlayNormalSFX(PowerAllocate,this.transform.position);
        RefreshUI();
    }

    public void OnClickReducePower()
    {
        if (currentFacility == null) return;
        Debug.Log("ReduceButton pressed");
        currentFacility.PowerDeAllocate();
        AudioMgr.Instance.PlayNormalSFX(PowerAllocate, this.transform.position);
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (currentFacility == null || powerText == null) return;
        powerText.text = $"{currentFacility.AllocatedPower}";
    }
}