using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightManager : MonoBehaviour
{
    public static NightManager Instance { get; private set; }

    [Header("夜晚时间")]
    [SerializeField] private float nightDuration = 60f; //夜晚持续的时间
    [SerializeField] private float currentNightTime = 0f;   //当前的时间 

    [Header("刷新间隔")]
    [SerializeField] private float enemySpawnsInterval = 1f;    //敌人刷新间隔
    [SerializeField] private float pcEnergyChangeInterval = 0.5f;  //电脑掉电的时间间隔

    [Header("计时器")]
    [SerializeField] private float pcEnergyTimer = 0f;
    [SerializeField] private float enemySpawnTimer = 0f;

    [Header("夜晚状态")]
    [SerializeField] private bool isActive=false;
    [SerializeField] private bool isPlayerOnBed = false;

    public bool IsPlayerOnBed => isPlayerOnBed;
    public float NightDuration => nightDuration;
    public float EnemySpawnTimer => enemySpawnTimer;
    public float CurrentNightTime => currentNightTime;
    public float EnemySpawnsInterval=> enemySpawnsInterval;
    public float PcEnergyChangeInterval => pcEnergyChangeInterval;
    public bool IsActive => isActive;

    public event Action OnEnemySpawnsTick; //敌人刷新事件
    public event Action OnPcEnergyChange;  //电脑掉电事件

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNightStarted += InitNight;
            GameManager.Instance.OnNightClear += EndNight;
            GameManager.Instance.OnPlayerDead += EndNight;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNightStarted -= InitNight;
            GameManager.Instance.OnNightClear -= EndNight;
            GameManager.Instance.OnPlayerDead -= EndNight;
        }
    }

    private void InitNight() {
        enemySpawnTimer = 0f;
        currentNightTime = 0f;
        pcEnergyTimer = 0f;
        ActivateNight();
    }

    private void EndNight()
    {
        DeActivateNight();
    }

    public void ActivateNight()
    {
        isActive = true;
    }

    public void DeActivateNight()
    {
        isActive = false;
    }

    private void TimeTick()
    {
        enemySpawnTimer += Time.deltaTime;
        currentNightTime += Time.deltaTime;
        pcEnergyTimer += Time.deltaTime;
    }

    private void DetectEnemySpawns()
    {
        while (enemySpawnTimer >= enemySpawnsInterval)
        {
            enemySpawnTimer -= enemySpawnsInterval;
            OnEnemySpawnsTick?.Invoke();
        }
    }

    private void DetectPCEnergy()
    {
        while (pcEnergyTimer >= pcEnergyChangeInterval)
        {
            pcEnergyTimer -= pcEnergyChangeInterval;
            OnPcEnergyChange?.Invoke();
        }
    }

    private void DetectNightClear()
    {
        if (currentNightTime >= nightDuration)
        {
            GameManager.Instance.NightClear();
        }
    }

    public void PlayerOnBed()
    {
        isPlayerOnBed = true;
    }

    private void Update()
    {
        if (!isActive) return;

        TimeTick();
        DetectPCEnergy();
        DetectEnemySpawns();
        DetectNightClear();
    }
}
