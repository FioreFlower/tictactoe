using TMPro;
using UnityEngine;

public struct SignupData
{
    public string username;
    public string nickname;
    public string password;
}
public class SignupPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _usernameInputField;
    [SerializeField] private TMP_InputField _nicknameInputField;
    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private TMP_InputField _confirmPasswordInputField;

    public void OnClickConfirmButton()
    {
        var username = _usernameInputField.text;
        var nickname = _nicknameInputField.text;
        var password = _passwordInputField.text;
        var confirmPassword = _confirmPasswordInputField.text;

        if ( string.IsNullOrEmpty(username)
                || string.IsNullOrEmpty(nickname)
                || string.IsNullOrEmpty(password)
                || string.IsNullOrEmpty(confirmPassword)
            )
        {
            // TODO : 입력값이 비어있음을 알리는 팝업창 표시
            return;
        }

        if (password.Equals(confirmPassword))
        {
            SignupData signupData = new()
            {
                username = username,
                nickname = nickname,
                password = password
            };

            // 서버로 SignupData 전달하면서 회원가입 진행
            StartCoroutine(NetworkManager.Instance.Signup(signupData, () => {
                Destroy(gameObject);
            }, () =>
            {
                _usernameInputField.text = "";
                _nicknameInputField.text = "";
                _passwordInputField.text = "";
                _confirmPasswordInputField.text = "";
            }));
        }
    }

    public void OnClickCancelButton()
    {
        Debug.Log("OnClickCancelButton");
    }
}
