using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private float _fadeTime;

    private CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        DontDestroyOnLoad(this.gameObject);
    }

    public async UniTask FadeOut()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.DOFade(1f, _fadeTime);
        await UniTask.WaitForSeconds(_fadeTime);
    }
    public async UniTask FadeIn()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.DOFade(0f, _fadeTime);
        await UniTask.WaitForSeconds(_fadeTime);
    }

}
