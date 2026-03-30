using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IconSwitch : MonoBehaviour
{
    [SerializeField] private List<Sprite> _icons;
    [SerializeField] private float _interval;
    public bool IsTriggered { get; set; }
    private float _timer;
    private int _currentIdx;
    private Image _image;
    void Awake()
    {
        _image = GetComponent<Image>();
        _image.sprite = _icons[0];
    }

    void Update()
    {
        if (IsTriggered)
        {
            _timer += Time.deltaTime;
            if (_timer >= _interval)
            {
                _currentIdx = _currentIdx + 1 >= _icons.Count ? 0 : _currentIdx + 1;
                _image.sprite = _icons[_currentIdx];
                _timer = 0f;
            }
        }
    }
}
