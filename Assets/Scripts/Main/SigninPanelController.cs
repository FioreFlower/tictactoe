using TMPro;
using UnityEngine;

public class SigninPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _usernamedInputField;
    [SerializeField] private TMP_InputField _passwordInputField;

    public void OnClickSigninButton()
    {
        string username = _usernamedInputField.text;
        string password = _passwordInputField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            // TODO : 누락된 값 입력 요청 팝업 표시
            return;
        }

        SigninRequest signinRequest = new()
        {
            username = username,
            password = password
        };

        NetworkManager.Instance.Signin(signinRequest, () =>
            {
                Destroy(gameObject);
                GameManager.Instance.GetScore();
            },
            (result) =>
            {
                if (result == (int)Constants.SigninResultType.UserNameNotFound)
                {
                    _usernamedInputField.text = "";
                } else if (result == (int)Constants.SigninResultType.IncorrectPassword)
                {
                    _passwordInputField.text = "";
                }
            });
    }

    public void OnClickSignupButton()
    {
        GameManager.Instance.OpenSignupPanel();
    }
}
