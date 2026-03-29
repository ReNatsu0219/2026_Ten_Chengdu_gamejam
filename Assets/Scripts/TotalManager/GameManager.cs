using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private int currentPower = 0;    //当前的可分配电量
    [SerializeField] private int dailyPower = 8;    //每天额外获得的电量
    [SerializeField] private int maxPower = 12;     //最多拥有的电量
    [SerializeField] private int allocatedPower = 0;    //当天已经分配的电量


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

    public event Action OnDayStarted;   //白天开始事件
    public event Action OnDayEnded; //白天结束事件
    public event Action OnNightStarted; //夜晚开始事件
    public event Action OnNightEnded;   //夜晚结束事件
    public event Action OnPlayerDead;   //玩家死亡事件
    public event Action OnNightClear;  //通关一天的事件

    private void Awake()
    {
        if(Instance !=null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartDay()
    {
        if (currentPhase == GamePhase.Day) return;

        currentPhase = GamePhase.Day;
        OnDayStarted?.Invoke();

        currentPower += 8;
        if (currentPower > maxPower) currentPower = maxPower;
    }

    public void EndDay()
    {
        if(currentPhase != GamePhase.Day) return;

        currentPhase = GamePhase.Transition;
        OnDayEnded?.Invoke();
    }

    public void StartNight()
    {
        if(currentPhase == GamePhase.Night) return;

        currentPhase = GamePhase.Night;
        OnNightStarted?.Invoke();
    }

    public void EndNight()
    {
        if (currentPhase != GamePhase.Night) return;

        currentPhase = GamePhase.Transition;
        OnNightEnded?.Invoke();
    }

    public void NightClear()
    {
        OnNightClear?.Invoke();
    }

    public void PlayerDead(string deathtype)
    {
        OnPlayerDead?.Invoke();
        Debug.Log("玩家死亡："+deathtype);
    }

    public void PowerSet(int value)
    {
        if (value > maxPower || value < 0) return;
        currentPower = value;
    }

    public void PowerDown()
    {
        if(currentPower<=0) return;
        currentPower--;
    }

    public void PowerUp()
    {
        if (currentPower >= maxPower) return;
        currentPower++;
    }

    //调试用
    private void Start()
    {
        //StartNight();
    }

    private void Update()
    {
        
    }
}
