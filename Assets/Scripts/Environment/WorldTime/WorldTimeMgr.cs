using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WorldTimeMgr : MonoSingleton<WorldTimeMgr>
{
    //private WorldLightService _lightService;

    public List<Color> ColorsCfg = new List<Color>();
    public Light2D WorldLight;

    private void Start()
    {
        //_lightService = new WorldLightService();
    }

    private void OnDestroy()
    {
        //if (_lightService != null)
        {
            //_lightService.Destroy();
        }
    }
}