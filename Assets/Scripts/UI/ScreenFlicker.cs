using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFlicker : MonoBehaviour
{
    [SerializeField] private float _interval;
    [SerializeField] private float _radius;
    [SerializeField] private RectTransform _screen;
    private float _timer;
    private Vector3 _originPos;
    void Awake()
    {
        _originPos = _screen.localPosition;
    }
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _interval)
        {
            float xOff = Random.Range(-Mathf.Abs(_radius), Mathf.Abs(_radius));
            float yOff = Random.Range(-Mathf.Abs(_radius), Mathf.Abs(_radius));

            _screen.localPosition = new Vector3(_originPos.x + xOff, _originPos.y + yOff, _originPos.z);
            _timer = 0f;
        }
    }
}
