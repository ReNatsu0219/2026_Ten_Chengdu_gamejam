using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ChargeBar : MonoBehaviour
{
    [SerializeField] private Image _fill;
    [SerializeField] private TextMeshProUGUI _percentText;
    private float _targetAmount;

    public void Initialize(float currentValue, float maxValue)
    {
        _targetAmount = currentValue / maxValue;
        _fill.fillAmount = _targetAmount;
    }
    public void UpdateStats(float targetValue, float maxValue)
    {
        _targetAmount = targetValue / maxValue;
        _fill.fillAmount = _targetAmount;
        SetPercentText();
    }
    private void SetPercentText()
    {
        _percentText.text = Mathf.RoundToInt(_targetAmount * 100f) + "%";
    }
}
