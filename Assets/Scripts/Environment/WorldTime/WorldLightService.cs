using System.Collections.Generic;
using UnityEngine;

public class LightService : MonoBehaviour
{
    [Header("夜晚颜色关键帧")]
    [SerializeField] private List<Color> nightColors = new List<Color>();

    [Header("夜晚亮度曲线")]
    [SerializeField] private float nightStartIntensity = 0.8f;
    [SerializeField] private float nightMidIntensity = 0.45f;
    [SerializeField] private float nightEndIntensity = 0.8f;

    private int currentColorIndex = 0;
    private float lastProgress = 0f;
    private bool isNightRunning = false;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayStarted += HandleDayStarted;
            GameManager.Instance.OnNightStarted += HandleNightStarted;
            GameManager.Instance.OnNightEnded += HandleNightEnded;
        }

        if (NightManager.Instance != null)
        {
            NightManager.Instance.OnQuickTick += TickLight;
        }

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.IsNight)
            {
                HandleNightStarted();
            }
            else
            {
                HandleDayStarted();
            }
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayStarted -= HandleDayStarted;
            GameManager.Instance.OnNightStarted -= HandleNightStarted;
            GameManager.Instance.OnNightEnded -= HandleNightEnded;
        }

        if (NightManager.Instance != null)
        {
            NightManager.Instance.OnQuickTick -= TickLight;
        }
    }

    private void HandleDayStarted()
    {
        isNightRunning = false;
        currentColorIndex = 0;
        lastProgress = 0f;

        if (LightManager.Instance != null)
        {
            LightManager.Instance.ApplyDayLightNow();
        }
    }

    private void HandleNightStarted()
    {
        isNightRunning = true;
        currentColorIndex = 0;
        lastProgress = 0f;

        if (LightManager.Instance != null)
        {
            Color startColor = GetNightColor(0f);
            LightManager.Instance.ApplyNightLightNow();
            LightManager.Instance.SetBaseLight(nightStartIntensity, startColor);
        }
    }

    private void HandleNightEnded()
    {
        isNightRunning = false;
        currentColorIndex = 0;
        lastProgress = 0f;

        if (LightManager.Instance != null)
        {
            LightManager.Instance.ApplyDayLightNow();
        }
    }

    private void TickLight()
    {
        if (!isNightRunning) return;
        if (NightManager.Instance == null) return;
        if (LightManager.Instance == null) return;
        if (!NightManager.Instance.IsActive) return;

        float progress = 0f;
        if (NightManager.Instance.NightDuration > 0f)
        {
            progress = NightManager.Instance.CurrentNightTime / NightManager.Instance.NightDuration;
        }

        progress = Mathf.Clamp01(progress);

        // 新一轮夜晚重置
        if (progress < lastProgress)
        {
            currentColorIndex = 0;
        }

        Color baseColor = GetNightColor(progress);
        float baseIntensity = EvaluateNightIntensity(progress);

        LightManager.Instance.SetBaseLight(baseIntensity, baseColor);

        lastProgress = progress;
    }

    private Color GetNightColor(float progress)
    {
        if (nightColors == null || nightColors.Count == 0)
        {
            return Color.white;
        }

        if (nightColors.Count == 1)
        {
            return nightColors[0];
        }

        while (currentColorIndex < nightColors.Count - 2 &&
               progress > (float)(currentColorIndex + 1) / (nightColors.Count - 1))
        {
            currentColorIndex++;
        }

        float sectionStart = (float)currentColorIndex / (nightColors.Count - 1);
        float sectionEnd = (float)(currentColorIndex + 1) / (nightColors.Count - 1);

        float localT = 0f;
        if (sectionEnd > sectionStart)
        {
            localT = (progress - sectionStart) / (sectionEnd - sectionStart);
        }

        return Color.Lerp(nightColors[currentColorIndex], nightColors[currentColorIndex + 1], localT);
    }

    private float EvaluateNightIntensity(float progress)
    {
        progress = Mathf.Clamp01(progress);

        // 亮 -> 暗 -> 亮
        if (progress <= 0.5f)
        {
            float t = progress / 0.5f;
            return Mathf.Lerp(nightStartIntensity, nightMidIntensity, t);
        }
        else
        {
            float t = (progress - 0.5f) / 0.5f;
            return Mathf.Lerp(nightMidIntensity, nightEndIntensity, t);
        }
    }
}