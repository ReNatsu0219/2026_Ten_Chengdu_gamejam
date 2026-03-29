using TMPro;
using UnityEngine;

public class CurrentPowerWindowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text currentPowerText;

    public void RefreshUI()
    {
        if (currentPowerText == null || GameManager.Instance == null) return;

        currentPowerText.text = $"{GameManager.Instance.CurrentPower}";
    }
}