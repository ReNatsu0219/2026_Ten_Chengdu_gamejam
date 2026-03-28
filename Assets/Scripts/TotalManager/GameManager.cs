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


    public GamePhase CurrentPhase => currentPhase;
    public int CurrentDay => currentDay;
    public int TargetDay => targetDay;

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


    //调试用
    private void Start()
    {
        StartNight();
    }

    private void Update()
    {
        
    }
}
