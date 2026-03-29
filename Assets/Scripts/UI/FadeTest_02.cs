using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeTest_02 : MonoBehaviour
{
    public CanvasGroup CanvasGroup;
    public RectTransform RectTransform;
    public Button Button;

    void Awake()
    {
        Button.onClick.AddListener(PanelFadeOut);
    }

    private void PanelFadeOut()
    {
        UIMgr.Instance.OperationPanelFadeOut(CanvasGroup, RectTransform, RectTransform.localPosition.x);
    }
}
