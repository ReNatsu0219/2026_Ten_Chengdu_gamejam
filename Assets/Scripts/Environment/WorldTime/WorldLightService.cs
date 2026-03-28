using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WorldLightService
{
    private Light2D _light;
    private List<Color> _colors = new List<Color>();//定义一个泛型集合，存储线性插值经过的端点的值
    private int _currentIdx;//此时颜色序号
    private float _time;
    public WorldLightService()
    {
        _light = WorldTimeMgr.Instance.WorldLight;
        _colors = WorldTimeMgr.Instance.ColorsCfg;

        WorldTimeMgr.Instance.TimeEvent += ColorFlow;
    }

    public void ColorFlow(float time)
    {
        if (time < _time) _currentIdx = 0;
        //这里判断i是因为：在i超过长度后，要使插值停止工作
        else if (time <= (float)((_currentIdx + 1f) / (_colors.Count - 1f)) && _currentIdx < _colors.Count - 1)
        {
            _light.color = Color.Lerp(_colors[_currentIdx], _colors[_currentIdx + 1], time * (_colors.Count - 1) - _currentIdx);
            _light.intensity = 1f - time;
        }
        else if (_currentIdx < _colors.Count - 1)//如果还有得变
        {
            _currentIdx++;
        }
        _time = time;
    }
    public void Destroy() => WorldTimeMgr.Instance.TimeEvent -= ColorFlow;
}
