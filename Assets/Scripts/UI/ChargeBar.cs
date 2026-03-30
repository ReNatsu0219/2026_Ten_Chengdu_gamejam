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
        _targetAmount = maxValue > 0f ? currentValue / maxValue : 0f;
        _targetAmount = Mathf.Clamp01(_targetAmount);

        _fill.fillAmount = _targetAmount;
        SetPercentText();
    }

    public void UpdateStats(float targetValue, float maxValue)
    {
        _targetAmount = maxValue > 0f ? targetValue / maxValue : 0f;
        _targetAmount = Mathf.Clamp01(_targetAmount);

        _fill.fillAmount = _targetAmount;
        SetPercentText();
    }

    private void SetPercentText()
    {
        _percentText.text = Mathf.RoundToInt(_targetAmount * 100f) + "%";
    }
}