using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }

    [Header("全局灯光")]
    [SerializeField] private Light2D worldLight;

    [Header("白天灯光")]
    [SerializeField] private float dayIntensity = 1f;
    [SerializeField] private Color dayColor = Color.white;

    [Header("夜晚起始灯光")]
    [SerializeField] private float nightStartIntensity = 0.8f;
    [SerializeField] private Color nightStartColor = Color.white;

    [Header("闪烁亮度")]
    [SerializeField] private float flickerDarkMultiplier = 0.2f;

    [Header("默认闪烁参数")]
    [SerializeField] private int flickerTotalTicks = 20;
    [SerializeField] private int switchIntervalTicks = 1;

    [Header("可调闪烁范围")]
    [SerializeField] private int minFlickerTotalTicks = 8;
    [SerializeField] private int maxFlickerTotalTicks = 40;
    [SerializeField] private int minSwitchIntervalTicks = 1;
    [SerializeField] private int maxSwitchIntervalTicks = 4;

    [Header("随机闪烁")]
    [SerializeField] private float darkChance = 1f;    // 每次切到黑的概率
    [SerializeField] private float brightChance = 1f;  // 每次切到亮的概率

    [Header("附加颜色")]
    [SerializeField] private Color redOverlayColor = new Color(1f, 0.75f, 0.75f, 1f);

    private float baseIntensity = 1f;
    private Color baseColor = Color.white;

    private float intensityMultiplier = 1f;

    private bool hasOverlayColor = false;
    private Color overlayColor = Color.white;

    private bool isFlickering = false;
    private bool isDark = false;
    private int flickerRemainTicks = 0;
    private int switchTickCounter = 0;

    public bool IsFlickering => isFlickering;
    public bool IsDark => isDark;

    public event Action OnLightTurnDark;
    public event Action OnLightTurnBright;
    public event Action OnFlickerFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (NightManager.Instance != null)
        {
            NightManager.Instance.OnQuickTick += TickFlicker;
        }

        ApplyDayLightNow();
    }

    private void OnDisable()
    {
        if (NightManager.Instance != null)
        {
            NightManager.Instance.OnQuickTick -= TickFlicker;
        }
    }

    public void SetBaseLight(float intensity, Color color)
    {
        baseIntensity = Mathf.Max(0f, intensity);
        baseColor = color;
        ApplyLight();
    }

    public void ApplyDayLightNow()
    {
        SetBaseLight(dayIntensity, dayColor);
        ResetLight();
    }

    public void ApplyNightLightNow()
    {
        SetBaseLight(nightStartIntensity, nightStartColor);
        ResetLight();
    }

    public void SetOverlayColor(Color color)
    {
        hasOverlayColor = true;
        overlayColor = color;
        ApplyLight();
    }

    public void SetRed()
    {
        SetOverlayColor(redOverlayColor);
    }

    public void ClearOverlayColor()
    {
        hasOverlayColor = false;
        ApplyLight();
    }

    public void ResetLight()
    {
        intensityMultiplier = 1f;
        hasOverlayColor = false;
        ApplyLight();
    }

    public void SetDarkChance(float value)
    {
        darkChance = Mathf.Clamp01(value);
    }

    public void SetBrightChance(float value)
    {
        brightChance = Mathf.Clamp01(value);
    }

    public void StartFlicker(int totalTicks = -1, int intervalTicks = -1)
    {
        int finalTicks = totalTicks > 0 ? totalTicks : flickerTotalTicks;
        int finalInterval = intervalTicks > 0 ? intervalTicks : switchIntervalTicks;

        flickerTotalTicks = Mathf.Clamp(finalTicks, minFlickerTotalTicks, maxFlickerTotalTicks);
        switchIntervalTicks = Mathf.Clamp(finalInterval, minSwitchIntervalTicks, maxSwitchIntervalTicks);

        isFlickering = true;
        isDark = false;
        flickerRemainTicks = flickerTotalTicks;
        switchTickCounter = 0;

        ApplyBright();
    }

    public void StopFlicker()
    {
        isFlickering = false;
        isDark = false;
        flickerRemainTicks = 0;
        switchTickCounter = 0;

        ApplyBright();
    }

    private void TickFlicker()
    {
        if (!isFlickering) return;

        flickerRemainTicks--;
        switchTickCounter++;

        if (switchTickCounter >= Mathf.Max(1, switchIntervalTicks))
        {
            switchTickCounter = 0;

            if (isDark)
            {
                if (UnityEngine.Random.value <= brightChance)
                {
                    ApplyBright();
                    OnLightTurnBright?.Invoke();
                    isDark = false;
                }
            }
            else
            {
                if (UnityEngine.Random.value <= darkChance)
                {
                    ApplyDark();
                    OnLightTurnDark?.Invoke();
                    isDark = true;
                }
            }
        }

        if (flickerRemainTicks <= 0)
        {
            StopFlicker();
            OnFlickerFinished?.Invoke();
        }
    }

    private void ApplyBright()
    {
        intensityMultiplier = 1f;
        ApplyLight();
    }

    private void ApplyDark()
    {
        intensityMultiplier = flickerDarkMultiplier;
        ApplyLight();
    }

    private void ApplyLight()
    {
        if (worldLight == null) return;

        worldLight.intensity = baseIntensity * intensityMultiplier;
        worldLight.color = hasOverlayColor ? overlayColor : baseColor;
    }
}