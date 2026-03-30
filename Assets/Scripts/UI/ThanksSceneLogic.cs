using UnityEngine;
using UnityEngine.UI;

public class ThanksSceneLogic : MonoBehaviour
{
    [SerializeField] private Button _door;
    [SerializeField] private Sprite _closed;
    [SerializeField] private Sprite _open;
    [SerializeField] private Image _background;
    private bool _isDoorOpen;
    void Awake()
    {
        _door.onClick.AddListener(ChangeBackground);
    }

    private void ChangeBackground()
    {
        if (!_isDoorOpen)
        {
            _isDoorOpen = true;
            _background.sprite = _open;
        }
        else
        {
            _isDoorOpen = false;
            _background.sprite = _closed;
        }
    }
}
