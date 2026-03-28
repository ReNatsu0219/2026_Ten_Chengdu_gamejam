using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WorldTimeMgr : MonoSingleton<WorldTimeMgr>
{
    private WorldLightService _lightService;
    public List<Color> ColorsCfg = new List<Color>();

    public Light2D WorldLight;
    public Action<float> TimeEvent;
    private float _timer;
    [SerializeField] private float _period;
    void Start()
    {
        _lightService = new WorldLightService();
    }
    void Update()
    {
        TimeFlow();
        SendTimeEvent();
    }
    void OnDestroy()
    {
        _lightService.Destroy();
    }
    private void TimeFlow()
    {
        _timer += Time.deltaTime;
        if (_timer > _period) _timer = 0f;
    }
    private void SendTimeEvent()
    {
        //归一化时间
        TimeEvent.Invoke(_timer / _period);
    }
}
