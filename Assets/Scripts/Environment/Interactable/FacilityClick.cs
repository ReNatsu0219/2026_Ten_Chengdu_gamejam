using UnityEngine;
using UnityEngine.UI;

public class FacilityClick : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _rectTransform;

    public void OpenFacilityPanel()
    {
        UIMgr.Instance.OperationPanelFadeIn(_canvasGroup, _rectTransform, _rectTransform.localPosition.x);
    }
}
