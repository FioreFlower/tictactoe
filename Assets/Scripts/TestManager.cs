using UnityEngine;

public class TestManager : MonoBehaviour
{
    [SerializeField] private MessagePopup messagePopup;
    
    void Start()
    {
        messagePopup.Show("Do you like react?", "No, I like Unity", _buttonAction);
    }

    void _buttonAction()
    {
        Debug.Log("버튼이 눌렸습니다! (GameManager에서 호출됨)");
        messagePopup.gameObject.SetActive(false);
    }
}
