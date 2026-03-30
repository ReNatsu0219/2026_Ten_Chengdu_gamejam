using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UIMgr : MonoSingleton<UIMgr>
{
    [SerializeField] private Image _operationPanel;
    [SerializeField] private Image _operationPanelFrame;
    [SerializeField] private RectTransform _thinkingIcon;
    [SerializeField] public RectTransform CurrentPowerWindow { get; private set; }
    [SerializeField] private CurrentPowerWindowUI currentPowerWindowUI;

    [SerializeField] private CanvasGroup screenPanelCanvasGroup;
    [SerializeField] private GameObject screenPanel;

    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _settingsBtn;
    [SerializeField] private Button _backBtn;
    [SerializeField] private Button _exitBtn;

    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _gameOverMask;
    [SerializeField] private RectTransform _restartBtn;
    [SerializeField] private RectTransform _gameOverExitBtn;
    [SerializeField] private RectTransform _gameOverTitle;

    private Vector3 _currentPowerWindowPos;
    private Vector3 _thinkIconPos;

    void Start()
    {
        _thinkIconPos = _thinkingIcon.transform.localPosition;
        _settingsBtn.onClick.AddListener(TogglePausePanel);
        _backBtn.onClick.AddListener(TogglePausePanel);

        if (CurrentPowerWindow == null && currentPowerWindowUI != null)
            CurrentPowerWindow = currentPowerWindowUI.GetComponent<RectTransform>();

        if (CurrentPowerWindow != null)
            _currentPowerWindowPos = CurrentPowerWindow.transform.localPosition;
        else
            Debug.LogError("CurrentPowerWindow û����");
    }

    #region Operation Panel
    public async UniTaskVoid OperationPanelFadeIn(CanvasGroup canvasGroup, RectTransform rectTransform, float xPos, float yPos = 0f, float duration = 1f)
    {
        if (_operationPanel.gameObject.activeSelf) return;

        _operationPanel.gameObject.SetActive(true);
        canvasGroup.gameObject.SetActive(true);
        CurrentPowerWindow.gameObject.SetActive(true);

        _operationPanel.DOBlendableColor(new Color(0f, 0f, 0f, 0.9f), duration * 0.5f);
        _operationPanelFrame.DOFade(1f, 0.5f * duration);

        _thinkingIcon.transform.localPosition = new Vector3(_thinkIconPos.x, -1000f, 0f);
        _thinkingIcon.DOAnchorPos(
            new Vector2(_thinkIconPos.x, _thinkIconPos.y),
            duration
        ).SetEase(GetRandomEase());

        PanelJumpFadeIn(canvasGroup, rectTransform, xPos, yPos, duration);

        CurrentPowerWindow.transform.localPosition = new Vector3(2000f, _currentPowerWindowPos.y, 0f);
        CurrentPowerWindow.DOAnchorPos(
            new Vector2(_currentPowerWindowPos.x, _currentPowerWindowPos.y),
            duration
        ).SetEase(GetRandomEase());

        await UniTask.WaitForSeconds(duration);
        _thinkingIcon.GetComponent<IconSwitch>().IsTriggered = true;
    }

    public void OperationPanelFadeOut(CanvasGroup canvasGroup, RectTransform rectTransform, float xPos, float yPos = 0f, float duration = 0.75f)
    {
        if (!_operationPanel.gameObject.activeSelf) return;

        _thinkingIcon.GetComponent<IconSwitch>().IsTriggered = false;
        _operationPanel.DOBlendableColor(new Color(0f, 0f, 0f, 0f), duration * 0.5f);
        _operationPanelFrame.DOFade(0f, 0.5f * duration);

        _thinkingIcon.transform.localPosition = new Vector3(_thinkIconPos.x, _thinkIconPos.y, 0f);
        _thinkingIcon.DOAnchorPos(
            new Vector2(_thinkIconPos.x, -1000f),
            duration
        ).SetEase(GetRandomEase());

        PanelJumpFadeOut(canvasGroup, rectTransform, xPos, yPos, duration);

        CurrentPowerWindow.transform.localPosition = new Vector3(_currentPowerWindowPos.x, _currentPowerWindowPos.y, 0f);
        CurrentPowerWindow.DOAnchorPos(
            new Vector2(2000f, _currentPowerWindowPos.y),
            duration
        ).SetEase(GetRandomEase());

        GameObject[] gos = {
            canvasGroup.gameObject,
            _operationPanel.gameObject,
            CurrentPowerWindow.gameObject
        };
        WaitFadeOutThenInavtivate(gos, duration).Forget();
    }
    #endregion

    #region Pause Panel
    private void TogglePausePanel()
    {
        if (_pausePanel.activeInHierarchy)
        {
            _pausePanel.SetActive(false);
            InputMgr.Instance.EnableGameplayInput();
            Time.timeScale = 1.0f;
        }
        else
        {
            _pausePanel.SetActive(true);
            Time.timeScale = 0.00f;
            InputMgr.Instance.DisableGameplayInput();
        }
    }
    #endregion

    #region GameOver Panel
    public void Dead()
    {
        FadeInGameOverPanel().Forget();
    }

    public void DeadOver()
    {
        FadeOutGameOverPanel().Forget();
    }
    public async UniTaskVoid FadeInGameOverPanel(float duration = 1f)
    {
        _gameOverPanel.SetActive(true);
        _gameOverMask.SetActive(true);

        Image panelImage = _gameOverPanel.GetComponent<Image>();
        Image maskImage = _gameOverMask.GetComponent<Image>();

        panelImage.color = new Color(1f, 0f, 0f, 0f);
        maskImage.color = new Color(1f, 0f, 0f, 0f);

        panelImage.DOColor(new Color(1f, 0f, 0f, 1f), duration);
        maskImage.DOColor(new Color(1f, 0f, 0f, 1f), duration);

        await UniTask.WaitForSeconds(duration);

        PanelJumpFadeIn(_gameOverTitle.GetComponent<CanvasGroup>(), _gameOverTitle, 0f, _gameOverTitle.localPosition.y, duration * 0.5f);
        await UniTask.WaitForSeconds(duration * 0.5f);
        PanelJumpFadeIn(_restartBtn.GetComponent<CanvasGroup>(), _restartBtn, 0f, _restartBtn.localPosition.y, duration * 0.5f);
        await UniTask.WaitForSeconds(duration * 0.5f);
        PanelJumpFadeIn(_gameOverExitBtn.GetComponent<CanvasGroup>(), _gameOverExitBtn, 0f, _gameOverExitBtn.localPosition.y, duration * 0.5f);
    }

    public async UniTaskVoid FadeOutGameOverPanel(float duration = 0.5f)
    {
        Image panelImage = _gameOverPanel.GetComponent<Image>();
        Image maskImage = _gameOverMask.GetComponent<Image>();

        panelImage.DOColor(new Color(1f, 0f, 0f, 0f), duration);
        maskImage.DOColor(new Color(1f, 0f, 0f, 0f), duration);

        await UniTask.WaitForSeconds(duration);

        _gameOverPanel.SetActive(false);
        _gameOverMask.SetActive(false);
    }
    #endregion

    public void PanelJumpFadeIn(CanvasGroup canvasGroup, RectTransform rectTransform, float xPos, float yPos = 0f, float duration = 1f)
    {
        canvasGroup.alpha = 0f;
        rectTransform.transform.localPosition = new Vector3(xPos, 1000f, 0f);
        rectTransform.DOAnchorPos(new Vector2(xPos, yPos), duration).SetEase(Ease.OutElastic);
        canvasGroup.DOFade(1f, duration);
    }

    public void PanelJumpFadeOut(CanvasGroup canvasGroup, RectTransform rectTransform, float xPos, float yPos = 0f, float duration = 1f)
    {
        canvasGroup.alpha = 1f;
        rectTransform.transform.localPosition = new Vector3(xPos, yPos, 0f);
        rectTransform.DOAnchorPos(new Vector2(xPos, 1000f), duration).SetEase(Ease.InOutQuint);
        canvasGroup.DOFade(0f, duration);
    }

    public void ShowScreenPanel(float duration = 0.2f)
    {
        if (screenPanel == null || screenPanelCanvasGroup == null) return;

        screenPanel.SetActive(true);

        _settingsBtn.interactable = false;

        screenPanelCanvasGroup.alpha = 0f;

        screenPanelCanvasGroup.DOKill();
        screenPanelCanvasGroup.DOFade(1f, duration).SetEase(Ease.OutQuad);
    }

    public void HideScreenPanel()
    {
        if (screenPanel == null) return;

        screenPanel.SetActive(false);

        // �ָ���ͣ��ť
        _settingsBtn.interactable = true;
    }

    public bool IsScreenPanelOpen()
    {
        return screenPanel != null && screenPanel.activeSelf;
    }

    private Ease GetRandomEase()
    {
        int easeMode = Random.Range(0, 5);
        Ease curve = Ease.Linear;
        switch (easeMode)
        {
            case 0: curve = Ease.InSine; break;
            case 1: curve = Ease.OutSine; break;
            case 2: curve = Ease.InOutQuint; break;
            case 3: curve = Ease.InOutBack; break;
            case 4: curve = Ease.OutBounce; break;
        }
        return curve;
    }

    private async UniTask WaitFadeOutThenInavtivate(GameObject[] gameObjects, float duration)
    {
        await UniTask.WaitForSeconds(duration);
        foreach (var go in gameObjects)
            go.SetActive(false);
    }

    public void RefreshPowerWindow()
    {
        currentPowerWindowUI?.RefreshUI();
    }
}