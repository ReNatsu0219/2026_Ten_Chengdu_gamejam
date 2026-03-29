using UnityEngine;
using TMPro;

public class DayBoardText : MonoBehaviour
{
    [SerializeField] private TMP_Text dayText;

    private void Awake()
    {
        if (dayText == null)
        {
            dayText = GetComponent<TMP_Text>();
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayStarted += RefreshText;
        }

        RefreshText();
    }

    private void OnDisable()
    {
        if (NightManager.Instance != null)
        {
            GameManager.Instance.OnDayStarted -= RefreshText;
        }
    }

    private void RefreshText()
    {
        if (dayText == null || GameManager.Instance == null)
            return;

        dayText.text = $"Day {GameManager.Instance.CurrentDay}";
    }
}