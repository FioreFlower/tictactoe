using TMPro;
using UnityEngine;

public struct SigninData
{
    public string username;
    public string password;
}

public struct SigninResult
{
    public int result;
}

public struct UserInfo
{
    public string id;
    public string username;
    public string nickname;
    public int score;
}

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

        SigninData signinData = new()
        {
            username = username,
            password = password
        };

        StartCoroutine(NetworkManager.Instance.Signin(signinData, () =>
            {
                Destroy(gameObject);
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
            }));
    }

    public void OnClickSignupButton()
    {
        GameManager.Instance.OpenSignupPanel();
    }
}
