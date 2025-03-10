using UnityEngine;

public class MainPanelController : MonoBehaviour
{
    public void OnClickSinglePlayButton()
    {
        GameManager.Instance.ChangeToGameScene(Constants.GameType.SinglePlayer);
    }
    
    public void OnClickDualPlayButton()
    {
        GameManager.Instance.ChangeToGameScene(Constants.GameType.CoOpPlayer);
    }

    public void OnClickMultiPlayButton()
    {
        GameManager.Instance.ChangeToGameScene(Constants.GameType.MultiPlayer);
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
            // TODO: 유저 정보 캐싱 진행
            GameManager.Instance.OpenLeaderboardPanel(userInfo);
        }, () =>
        {
            // 로그인 화면 띄우기
            GameManager.Instance.OpenSigninPanel();
        });
    }
}
