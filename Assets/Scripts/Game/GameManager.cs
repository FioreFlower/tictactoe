using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;
    [SerializeField] private GameObject leaderboardPanel;

    private BlockController _blockController;
    private GameUIController _gameUIController;
    private Canvas _canvas;
    
    public enum PlayerType { None, PlayerA, PlayerB }
    private PlayerType[,] _board;

    private GameType currentGameTypeState;

    private enum TurnType { PlayerA, PlayerB }

    private enum GameResult
    {
        None,   // 게임 진행 중
        Win,    // 플레이어 승
        Lose,   // 플레이어 패
        Draw    // 비김
    }

    // 서버에서 받은 데이터 캐싱
    public int PlayerScore { get; private set; }
    public string UserNickName{ get; private set;}
    
    public enum GameType { SinglePlayer, CoOpPlayer }

    private void Start()
    {
        NetworkManager.Instance.GetScore((userInfo) =>
        {
            Debug.Log("Already has Session ID : " + userInfo.username);
            PlayerScore = userInfo.score; // 서버 점수 불러와 캐싱
            UserNickName = userInfo.nickname; // 로그인한 유저 닉네임 캐싱
        }, OpenSigninPanel);
    }

    public void ChangeToGameScene(GameType gameType)
    {
        currentGameTypeState = gameType;
        SceneManager.LoadScene("Game");
    }

    public void ChangeToMainScene()
    {
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

    public void OpenLeaderboardPanel()
    {
        if (_canvas != null)
        {
            NetworkManager.Instance.GetScore((userInfo) =>
            {
                Debug.Log("Already has Session ID : " + userInfo.username);
                PlayerScore = userInfo.score; // 서버 점수 불러와 캐싱
                UserNickName = userInfo.nickname; // 로그인한 유저 닉네임 캐싱
                
                var leaderboardPanelObject = Instantiate(leaderboardPanel, _canvas.transform);
            }, OpenSigninPanel);

        }
    }

    /// <summary>
    /// 게임 시작
    /// </summary>
    private void StartGame()
    {
        // _board 초기화
        _board = new PlayerType[3, 3];
        
        // 블록 초기화
        _blockController.InitBlocks();
        
        // 게임 초기화
        _blockController.OnBlockClickedDelegate = null;
        
        // Game UI 초기화
        _gameUIController.SetGameUIMode(GameUIController.GameUIMode.Init);
        
        // 턴 시작
        SetTurn(TurnType.PlayerA);
    }

    /// <summary>
    /// 게임 오버시 호출되는 함수
    /// gameResult에 따라 결과 출력
    /// </summary>
    /// <param name="gameResult">win, lose, draw</param>
    private void EndGame(GameResult gameResult)
    {
        // 게임오버 표시
        _gameUIController.SetGameUIMode(GameUIController.GameUIMode.GameOver);

        // 더이상 블록을 할당하지 못하도록 
        _blockController.OnBlockClickedDelegate = null;
        
        // TODO: 나중에 구현!!
        switch (gameResult)
        {
            case GameResult.Win:
                Debug.Log("Player A win");
                PlayerScore += 10;
                //  여기서 게임 스코어 업데이트?
                UpdateScoreRequest updateScore = new()
                {
                    score = PlayerScore
                };

                // 업데이트 요청
                NetworkManager.Instance.UpdateScore(updateScore, () => { }, () => { });

                break;
            case GameResult.Lose:
                Debug.Log("Player B win");
                break;
            case GameResult.Draw:
                Debug.Log("Draw");
                break;
        }
    }

    /// <summary>
    /// _board에 새로운 값을 할당하는 함수
    /// </summary>
    /// <param name="playerType">할당하고자 하는 플레이어 타입</param>
    /// <param name="row">Row</param>
    /// <param name="col">Col</param>
    /// <returns>False가 반환되면 할당할 수 없음, True는 할당이 완료됨</returns>
    private bool SetNewBoardValue(PlayerType playerType, int row, int col)
    {
        if (_board[row, col] != PlayerType.None) return false;

        var newMarker = playerType == PlayerType.PlayerA 
            ? Block.MarkerType.O 
            : Block.MarkerType.X;
        _board[row, col] = playerType;
        
        _blockController.PlaceMarker(newMarker, row, col);
        return true;
    }

    private void SetTurn(TurnType turnType)
    {
        switch (turnType)
        {
            case TurnType.PlayerA:
                _gameUIController.SetGameUIMode(GameUIController.GameUIMode.TurnA);
                SetupPlayerMove(PlayerType.PlayerA, TurnType.PlayerB);
                break;
            case TurnType.PlayerB:
                _gameUIController.SetGameUIMode(GameUIController.GameUIMode.TurnB);
                if (currentGameTypeState == GameType.CoOpPlayer)
                {
                    SetupPlayerMove(PlayerType.PlayerB, TurnType.PlayerA);
                }
                else // 싱글플레이 인 경우
                {
                    _blockController.OnBlockClickedDelegate = null; // 블럭 클릭 델리게이트 제거
                    // AI 턴: AI로부터 최적의 수를 받아 바로 처리
                    var result = MiniMaxAIController.GetBestMove(_board);
                    if (result.HasValue)
                        ProcessMove(PlayerType.PlayerB, result.Value.row, result.Value.col, TurnType.PlayerA);
                    else
                    {
                        // TODO: AI가 유효한 수를 찾지 못했을 때 처리? 필요할수도??
                    }
                }
                break;
        }
    }
    
    /// <summary>
    /// 플레이어가 직접 클릭하여 이동할 때 사용하는 이벤트 핸들러를 등록하는 함수
    /// </summary>
    private void SetupPlayerMove(PlayerType player, TurnType nextTurn)
    {
        _blockController.OnBlockClickedDelegate = (row, col) =>
        {
            ProcessMove(player, row, col, nextTurn);
        };
    }
    
    /// <summary>
    /// 주어진 좌표에 플레이어의 말을 두고, 게임 상태를 확인한 후 턴 전환 혹은 게임 종료 처리를 수행하는 함수
    /// </summary>
    private void ProcessMove(PlayerType player, int row, int col, TurnType nextTurn)
    {
        if (SetNewBoardValue(player, row, col))
        {
            var gameResult = CheckGameResult();
            if (gameResult == GameResult.None)
            {
                SetTurn(nextTurn);
            }
            else
            {
                EndGame(gameResult);
            }
        }
        else
        {
            // TODO: 이미 값이 있는 곳을 클릭했을 때 처리
        }
    }

    /// <summary>
    /// 게임 결과 확인 함수
    /// </summary>
    /// <returns>플레이어 기준 게임 결과</returns>
    private GameResult CheckGameResult()
    {
        if (CheckGameWin(PlayerType.PlayerA)) { return GameResult.Win; }
        if (CheckGameWin(PlayerType.PlayerB)) { return GameResult.Lose; }
        if (MiniMaxAIController.IsAllBlocksPlaced(_board)) { return GameResult.Draw; }
        
        return GameResult.None;
    }
    
    
    //게임의 승패를 판단하는 함수
    private bool CheckGameWin(PlayerType playerType)
    {
        int length = _board.GetLength(0);
        
        // 가로로 마커가 일치하는지 확인
        for (var row = 0; row < length; row++)
        {
            if (_board[row, 0] == playerType && _board[row, 1] == playerType && _board[row, 2] == playerType)
            {
                (int, int)[] blocks = { ( row, 0 ), ( row, 1 ), ( row, 2 ) };
                _blockController.SetBlockColor(playerType, blocks);
                return true;
            }
        }
        
        // 세로로 마커가 일치하는지 확인
        for (var col = 0; col < length; col++)
        {
            if (_board[0, col] == playerType && _board[1, col] == playerType && _board[2, col] == playerType)
            {
                (int, int)[] blocks = { ( 0, col ), ( 1, col ), ( 2, col ) };
                _blockController.SetBlockColor(playerType, blocks);
                return true;
            }
        }
        
        // 대각선 마커 일치하는지 확인
        if (_board[0, 0] == playerType && _board[1, 1] == playerType && _board[2, 2] == playerType)
        {
            (int, int)[] blocks = { ( 0, 0 ), ( 1, 1 ), ( 2, 2 ) };
            _blockController.SetBlockColor(playerType, blocks);
            return true;
        }
        if (_board[0, 2] == playerType && _board[1, 1] == playerType && _board[2, 0] == playerType)
        {
            (int, int)[] blocks = { ( 0, 2 ), ( 1, 1 ), ( 2, 0 ) };
            _blockController.SetBlockColor(playerType, blocks);
            return true;
        }

        return false;
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            _blockController = GameObject.FindObjectOfType<BlockController>();
            _gameUIController = GameObject.FindObjectOfType<GameUIController>();

            // 게임 시작
            StartGame();
        }
        
        _canvas = GameObject.FindObjectOfType<Canvas>();
    }
}