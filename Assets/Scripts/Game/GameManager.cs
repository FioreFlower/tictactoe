using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;
    [SerializeField] private GameObject leaderboardPanel;

    private GameUIController _gameUIController;
    private GameLogic _gameLogic;
    private Canvas _canvas;

    private Constants.GameType currentGameTypeState;

    // 서버에서 받은 유저 데이터 캐싱
    public int PlayerScore { get; private set; }
    public string UserNickName{ get; private set;}


    private void Start()
    {
        GetScore();
    }

    public void GetScore()
    {
        NetworkManager.Instance.GetScore((userInfo) =>
        {
            PlayerScore = userInfo.score; // 서버 점수 불러와 캐싱
            UserNickName = userInfo.nickname; // 로그인한 유저 닉네임 캐싱
        }, OpenSigninPanel);
    }

    public void ChangeToGameScene(Constants.GameType gameType)
    {
        currentGameTypeState = gameType;
        SceneManager.LoadScene("Game");
    }

    public void ChangeToMainScene()
    {
        _gameLogic?.Dispose();
        _gameLogic = null;
        SceneManager.LoadScene("Main");
    }

    public void OpenSettingsPanel()
    {
        if (_canvas != null)
        {
            var settingsPanelObject = Instantiate(settingsPanel, _canvas.transform);
            settingsPanelObject.GetComponent<PanelController>().Show();
        }
    }

    public void OpenConfirmPanel(string message, ConfirmPanelController.OnConfirmButtonClick onConfirmButtonClick)
    {
        if (_canvas != null)
        {
            var confirmPanelObject = Instantiate(confirmPanel, _canvas.transform);
            confirmPanelObject.GetComponent<ConfirmPanelController>().Show(message, onConfirmButtonClick);
        }
    }

    public void OpenSigninPanel()
    {
        if (_canvas != null)
        {
            var signinPanelObject = Instantiate(signinPanel, _canvas.transform);
        }
    }

    public void OpenSignupPanel()
    {
        if (_canvas != null)
        {
            var signupPanelObject = Instantiate(signupPanel, _canvas.transform);
        }
    }

    public void OpenLeaderboardPanel(UserInfo userInfo)
    {
        if (_canvas != null)
        {
            UserNickName = userInfo.nickname; // 캐싱
            PlayerScore = userInfo.score; // 캐싱
            var leaderboardPanelObject = Instantiate(leaderboardPanel, _canvas.transform);
        }
    }

    public void OpenGameOverPanel()
    {
        _gameUIController.SetGameUIMode(GameUIController.GameUIMode.GameOver);
    }


    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            // 씬에 배치된 오브젝트 찾기 (BlockController, GameUIController)
            var blockController = GameObject.FindObjectOfType<BlockController>();
            _gameUIController = GameObject.FindObjectOfType<GameUIController>();

            //BlockController 초기화
            blockController.InitBlocks();

            // Game UI 초기화
            _gameUIController.SetGameUIMode(GameUIController.GameUIMode.Init);

            // Game Logic 객체 생성
            var gameLogic = new GameLogic(blockController, currentGameTypeState);
        }
        
        _canvas = GameObject.FindObjectOfType<Canvas>();
    }

    private void OnApplicationQuit()
    {
        _gameLogic?.Dispose();
        _gameLogic = null;
    }
}