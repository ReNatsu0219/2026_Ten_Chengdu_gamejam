using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UIMgr : MonoSingleton<UIMgr>
{
    [field: SerializeField] public Image OperationPanel;
    [field: SerializeField] public Image OperationPanelFrame;
    [field: SerializeField] public RectTransform ThinkingIcon;
    [field: SerializeField] public RectTransform CurrentPowerWindow { get; private set; }
    [SerializeField] private CurrentPowerWindowUI currentPowerWindowUI;

    private Vector3 _currentPowerWindowPos;
    private Vector3 _thinkIconPos;

    void Start()
    {
        _thinkIconPos = ThinkingIcon.transform.localPosition;
        _currentPowerWindowPos = CurrentPowerWindow.transform.localPosition;
    }

    public void OperationPanelFadeIn(CanvasGroup canvasGroup, RectTransform rectTransform, float xPos, float yPos = 0f, float duration = 1f)
    {
        if (OperationPanel.gameObject.activeSelf) return;

        OperationPanel.gameObject.SetActive(true);
        canvasGroup.gameObject.SetActive(true);
        CurrentPowerWindow.gameObject.SetActive(true);

        OperationPanel.DOBlendableColor(new Color(0f, 0f, 0f, 0.9f), duration * 0.5f);
        OperationPanelFrame.DOFade(1f, 0.5f * duration);

        ThinkingIcon.transform.localPosition = new Vector3(_thinkIconPos.x, -1000f, 0f);
        ThinkingIcon.DOAnchorPos(
            new Vector2(_thinkIconPos.x, _thinkIconPos.y),
            duration
        ).SetEase(GetRandomEase());

        PanelJumpFadeIn(canvasGroup, rectTransform, xPos, yPos, duration);

        CurrentPowerWindow.transform.localPosition = new Vector3(2000f, _currentPowerWindowPos.y, 0f);
        CurrentPowerWindow.DOAnchorPos(
            new Vector2(_currentPowerWindowPos.x, _currentPowerWindowPos.y),
            duration
        ).SetEase(GetRandomEase());
    }

    public void OperationPanelFadeOut(CanvasGroup canvasGroup, RectTransform rectTransform, float xPos, float yPos = 0f, float duration = 0.75f)
    {
        if (!OperationPanel.gameObject.activeSelf) return;

        OperationPanel.DOBlendableColor(new Color(0f, 0f, 0f, 0f), duration * 0.5f);
        OperationPanelFrame.DOFade(0f, 0.5f * duration);

        ThinkingIcon.transform.localPosition = new Vector3(_thinkIconPos.x, _thinkIconPos.y, 0f);
        ThinkingIcon.DOAnchorPos(
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
            OperationPanel.gameObject,
            CurrentPowerWindow.gameObject
        };
        WaitFadeOutThenInavtivate(gos, duration).Forget();
    }

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