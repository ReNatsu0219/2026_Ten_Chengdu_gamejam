using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private Button _startBtn;
    [SerializeField] private Button _exitBtn;
    [SerializeField] private Image _background;
    [SerializeField] private Sprite _startedBackground;

    void Awake()
    {
        _startBtn.onClick.AddListener(OnGameStart);
        _exitBtn.onClick.AddListener(() => Application.Quit());
    }
    private void OnGameStart()
    {
        _background.sprite = _startedBackground;
        SceneMgr.Instance.TransitionToLevelScene();
    }
}
