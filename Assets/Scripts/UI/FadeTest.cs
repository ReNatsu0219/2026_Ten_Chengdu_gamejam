using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeTest : MonoBehaviour
{
    public CanvasGroup CanvasGroup;
    public RectTransform RectTransform;
    public Button Button;

    void Awake()
    {
        Button.onClick.AddListener(PanelFadeIn);
    }

    private void PanelFadeIn()
    {
        UIMgr.Instance.OperationPanelFadeIn(CanvasGroup, RectTransform, RectTransform.localPosition.x);
    }
}
