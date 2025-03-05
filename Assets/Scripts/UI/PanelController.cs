using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class PanelController : MonoBehaviour
{
    [SerializeField] protected RectTransform panelRectTransform;      // 팝업창

    private CanvasGroup _backgroundCanvasGroup;                     // 뒤에 시커먼 배경

    protected delegate void PanelControllerHideDelegate();
    
    private void Awake()
    {
        _backgroundCanvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Panel 표시 함수
    /// </summary>
    public void Show()
    {
        _backgroundCanvasGroup.alpha = 0;
        panelRectTransform.localScale = Vector3.zero;
        
        _backgroundCanvasGroup.DOFade(1, 0.3f).SetEase(Ease.Linear);
        panelRectTransform.DOScale(1, 0.3f).SetEase(Ease.InOutBack);
    }

    /// <summary>
    /// Panel 숨기기 함수
    /// </summary>
    protected void Hide(PanelControllerHideDelegate hideDelegate = null)
    {
        _backgroundCanvasGroup.alpha = 1;
        panelRectTransform.localScale = Vector3.one;
        
        _backgroundCanvasGroup.DOFade(0, 0.3f).SetEase(Ease.Linear);
        panelRectTransform.DOScale(1, 0.3f).SetEase(Ease.InOutBack).OnComplete(() =>
        {
            hideDelegate?.Invoke();
            Destroy(gameObject);
        });
    }
}
