using UnityEngine;

public class MainPanelController : MonoBehaviour
{
    public void OnClickSinglePlayButton()
    {
        GameManager.Instance.ChangeToGameScene(GameManager.GameType.SinglePlayer);
    }
    
    public void OnClickDualPlayButton()
    {
        GameManager.Instance.ChangeToGameScene(GameManager.GameType.CoOpPlayer);
    }
    
    public void OnClickSettingsButton()
    {
        GameManager.Instance.OpenSettingsPanel();
    }

    public void OnClickScoreButton()
    {
        NetworkManager.Instance.GetScore((userInfo) =>
        {
            Debug.Log(userInfo);
            GameManager.Instance.OpenLeaderboardPanel();
        }, () =>
        {
            // 로그인 화면 띄우기
            GameManager.Instance.OpenSigninPanel();
        });
    }
}
