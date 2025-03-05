using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkManager : Singleton<NetworkManager>
{
    public IEnumerator Signin(SigninData signinData, Action successAction, Action<int> failAction)
    {
        string jsonString = JsonUtility.ToJson(signinData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using ( UnityWebRequest www =
                new UnityWebRequest(Constants.ServerURL + "/users/signin", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                // TODO : 연결 오류 프로토콜 오류시 처리
            }
            else
            {
                string cookie = www.GetResponseHeader("Set-Cookie");
                if (!string.IsNullOrEmpty(cookie))
                {
                    int lastIndex = cookie.IndexOf(';');
                    string sid = cookie.Substring(0, lastIndex);
                    Debug.Log(sid);
                    PlayerPrefs.SetString("sid", sid);
                }

                var resultString = www.downloadHandler.text;
                var result = JsonUtility.FromJson<SigninResult>(resultString);

                if (result.result == (int)Constants.NetworkResultType.UserNameFail)
                {
                    // 유저 이름이 유효하지 않음
                    GameManager.Instance.OpenConfirmPanel("유저네임이 유효하지 않습니다.", () =>
                    {
                        // _usernamedInputField.text = "";
                        failAction?.Invoke((int)Constants.NetworkResultType.UserNameFail);
                    });
                } else if (result.result == (int)Constants.NetworkResultType.PasswordFail)
                {
                    // 패스워드가 유효하지 않음
                    GameManager.Instance.OpenConfirmPanel("패스워드가 유효하지 않습니다.", () =>
                    {
                        // _passwordInputField.text = "";
                        failAction?.Invoke((int)Constants.NetworkResultType.PasswordFail);
                    });
                }else if (result.result == (int)Constants.NetworkResultType.Success)
                {
                    // 성공
                    GameManager.Instance.OpenConfirmPanel("로그인에 성공했습니다.", () =>
                    {
                        // Destroy(gameObject);
                        successAction?.Invoke();
                    });
                }
            }
        }
    }

    public IEnumerator Signup(SignupData signupData , Action successAction, Action failAction)
    {
        string jsonString = JsonUtility.ToJson(signupData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using ( UnityWebRequest www =
                new UnityWebRequest(Constants.ServerURL + "/users/signup", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error : " + www.error);

                if (www.responseCode == 409)
                {
                    // TODO: 중복 사용자 생성 표시 팝업
                    Debug.Log("중복 사용자");
                    GameManager.Instance.OpenConfirmPanel("이미 존재하는 사용자입니다.", () =>
                    {
                        failAction?.Invoke();
                    });
                }
            }
            else
            {
                var result = www.downloadHandler.text;
                Debug.Log("Result :" + result);

                //회원가입 성공 팝업 표시
                GameManager.Instance.OpenConfirmPanel("회원가입이 완료되었습니다.", () =>
                {
                    successAction?.Invoke();
                });
            }
        }
    }

    public IEnumerator GetScore(Action<ScoreResult> successAction, Action failAction)
    {
        using (UnityWebRequest www = new UnityWebRequest(Constants.ServerURL + "/users/score", UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (www.responseCode == 403)
                {
                    Debug.Log("로그인이 필요합니다.");
                }
                failAction?.Invoke();
            }
            else
            {
                var result = www.downloadHandler.text;
                var userScore = JsonUtility.FromJson<ScoreResult>(result);

                Debug.Log("user Score : " + userScore.score);

                successAction?.Invoke(userScore);
            }
        }
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
}