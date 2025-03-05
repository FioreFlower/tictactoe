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
}
