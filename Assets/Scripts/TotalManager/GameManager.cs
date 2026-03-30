using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum GamePhase
{
    Menu,
    Day,
    Night,
    Transition,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("游戏流程")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Day;
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int targetDay = 5;

    [Header("游戏资源")]
    [SerializeField] private int currentPower = 0;
    [SerializeField] private int dailyPower = 8;
    [SerializeField] private int maxPower = 12;
    [SerializeField] private int allocatedPower = 0;

    [Header("过渡参数")]
    [SerializeField] private float phaseTransitionDuration = 2f;

    private bool isTransitioning = false;

    public GamePhase CurrentPhase => currentPhase;
    public int CurrentDay => currentDay;
    public int TargetDay => targetDay;
    public int CurrentPower => currentPower;
    public int DailyPower => dailyPower;
    public int MaxPower => maxPower;
    public int AllocatedPower => allocatedPower;

    public bool IsDay => currentPhase == GamePhase.Day;
    public bool IsNight => currentPhase == GamePhase.Night;
    public bool IsTransition => currentPhase == GamePhase.Transition;
    public bool IsGameOver => currentPhase == GamePhase.GameOver;

    public event Action OnDayStarted;
    public event Action OnDayEnded;
    public event Action OnNightStarted;
    public event Action OnNightEnded;
    public event Action OnPlayerDead;
    public event Action OnNightClear;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentPhase = GamePhase.Menu;
    }

    private void Start()
    {
        EnterDayImmediate();
        currentPower = dailyPower;
    }


    public void SwitchDayToNight()
    {
        if (currentPhase != GamePhase.Day || isTransitioning) return;
        StartCoroutine(DayToNightRoutine());
    }

    public void SwitchNightToDay()
    {
        if (currentPhase != GamePhase.Night || isTransitioning) return;
        StartCoroutine(NightToDayRoutine());
    }


    private IEnumerator DayToNightRoutine()
    {
        PlayerCtrl player = FindObjectOfType<PlayerCtrl>();
        player?.SetControlEnabled(false);

        isTransitioning = true;

        currentPhase = GamePhase.Transition;
        OnDayEnded?.Invoke();

        LightManager.Instance?.FadeToBlack(phaseTransitionDuration);
        yield return new WaitForSeconds(phaseTransitionDuration);

        currentPhase = GamePhase.Night;
        LightManager.Instance?.ApplyNightLightNow();
        OnNightStarted?.Invoke();

        LightManager.Instance?.FadeFromBlack(phaseTransitionDuration);
        yield return new WaitForSeconds(phaseTransitionDuration);

        player?.SetControlEnabled(true);
        isTransitioning = false;
    }

    private IEnumerator NightToDayRoutine()
    {
        isTransitioning = true;

        PlayerCtrl player = FindObjectOfType<PlayerCtrl>();
        player?.SetControlEnabled(false);


        currentPhase = GamePhase.Transition;
        OnNightEnded?.Invoke();

        LightManager.Instance?.FadeToBlack(phaseTransitionDuration);
        yield return new WaitForSeconds(phaseTransitionDuration);

        currentPhase = GamePhase.Day;
        LightManager.Instance?.ApplyDayLightNow();
        OnDayStarted?.Invoke();

        LightManager.Instance?.FadeFromBlack(phaseTransitionDuration);
        yield return new WaitForSeconds(phaseTransitionDuration);

        player?.SetControlEnabled(true);
        isTransitioning = false;
    }


    private void EnterDayImmediate()
    {
        currentPhase = GamePhase.Day;
        LightManager.Instance?.ApplyDayLightNow();
        OnDayStarted?.Invoke();
    }

    private void EnterNightImmediate()
    {
        currentPhase = GamePhase.Night;
        LightManager.Instance?.ApplyNightLightNow();
        OnNightStarted?.Invoke();
    }


    public void EndDay()
    {
        SwitchDayToNight();
    }

    public void NightClear()
    {
        OnNightClear?.Invoke();

        if (currentDay < targetDay)
        {
            currentDay++;
            SwitchNightToDay();
        }
        else
        {
            currentPhase = GamePhase.Victory;
            // 这里放通关逻辑
        }
    }

    public void PlayerDead(string deathtype)
    {
        OnPlayerDead?.Invoke();
        Debug.Log("玩家死亡：" + deathtype);

        UIMgr.Instance.Dead();
    }

    public void ReStart()
    {
        SwitchNightToDay();
        UIMgr.Instance.DeadOver();
    }

    public void PowerSet(int value)
    {
        if (value > maxPower || value < 0)
        {
            currentPower = maxPower;
            return;
        }
        currentPower = value;
    }

    public void PowerDown()
    {
        if (currentPower <= 0) return;
        currentPower--;
    }

    public void PowerUp()
    {
        if (currentPower >= maxPower) return;
        currentPower++;
    }
}