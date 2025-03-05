using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text message;
    [SerializeField] private TMP_Text buttonText;
    
    [SerializeField] private Button messageButton;
    [SerializeField] private Button closeButton;
    
    public delegate void Delegate();
    private Delegate _buttonAction;

    private void Start()
    {
        closeButton.onClick.AddListener(CloseButtonAction);
    }
    public void Show( string getMessage, string getButtonText, Delegate buttonAction)
    {
        message.text = getMessage;
        buttonText.text = getButtonText;
        _buttonAction = buttonAction;
        
        messageButton.onClick.AddListener(()=>_buttonAction());
        
        gameObject.SetActive(true);
    }

    private void CloseButtonAction()
    {
        Debug.Log("버튼 누름, 내부에서 호출됨");
        gameObject.SetActive(false);
    }
}
