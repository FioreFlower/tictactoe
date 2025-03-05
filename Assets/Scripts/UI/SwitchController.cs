using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(AudioSource))]
public class SwitchController : MonoBehaviour
{
    [SerializeField] private Image handleImage;
    [SerializeField] private AudioClip clickSound;
    
    public delegate void OnSwitchChangedDelegate(bool isOn);
    public OnSwitchChangedDelegate OnSwitchChanged;
    
    private static readonly Color32 _onColor = new Color32(117, 70, 93, 231);
    private static readonly Color32 _offColor = new Color32(70, 93, 117, 231);
    
    private Image _backgroundImage;
    private RectTransform _handleRectTransform;
    private bool _isOn;
    private AudioSource _audioSource;
    
    
    private void Awake()
    {
        _backgroundImage = GetComponent<Image>();
        _handleRectTransform = handleImage.GetComponent<RectTransform>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _handleRectTransform.anchoredPosition = new Vector2(-16, 0);
        _backgroundImage.color = _offColor;
        _isOn = false;
    }

    private void SetOn(bool isOn)
    {
        if (isOn)
        {
            _handleRectTransform.DOAnchorPosX(16f, 0.2f);
        }
        else
        {
            _handleRectTransform.DOAnchorPosX(-16f, 0.2f);
        }

        // 효과음 재생
        if (clickSound != null)
            _audioSource.PlayOneShot(clickSound);
        
        _backgroundImage.DOBlendableColor(isOn ? _onColor : _offColor, 0.2f);
        OnSwitchChanged?.Invoke(isOn);
        _isOn = isOn;
    }

    public void OnClickSwitch()
    {
        
        SetOn(!_isOn);
    }
    
}
