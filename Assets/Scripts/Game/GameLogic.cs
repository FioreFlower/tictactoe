using System;

public abstract class BasePlayerState
{
    public abstract void OnEnter(GameLogic gameLogic);

    public abstract void OnExit(GameLogic gameLogic);

    public abstract void HandleMove(GameLogic gameLogic, int row, int col);

    protected abstract void HandleNextTurn(GameLogic gameLogic);

    protected void ProcessMove(GameLogic gameLogic, Constants.PlayerType playerType, int row, int col)
    {
        if (gameLogic.SetNewBoardValue(playerType, row, col))
        {
            var gameResult = gameLogic.CheckGameResult();

            if (gameResult == GameLogic.GameResult.None)
            {
                HandleNextTurn(gameLogic);
            }
            else
            {
                gameLogic.EndGame(gameResult);
            }
        }
    }
}

// 직접 플레이 (싱글, 네트워크)
public class PlayerState : BasePlayerState
{
    private Constants.PlayerType _playerType;
    private bool _isFirstPlayer;

    private MultiPlayManager _multiPlayManager;
    private string _roomId;

    private bool _isMultiPlay;

    public PlayerState(bool isFirstPlayer)
    {
        _isFirstPlayer = isFirstPlayer;
        _playerType =  _isFirstPlayer ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
        _isMultiPlay = false;
    }

    public PlayerState(bool isFirstPlayer, MultiPlayManager multiPlayManager, string roomId) : this(isFirstPlayer)
    {
        _multiPlayManager = multiPlayManager;
        _roomId = roomId;
        _isMultiPlay = true;
    }

    public override void OnEnter(GameLogic gameLogic)
    {
        gameLogic.blockController.OnBlockClickedDelegate = (row, col) =>
        {
            HandleMove(gameLogic, row, col);
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.blockController.OnBlockClickedDelegate = null;
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, _playerType, row, col);

        if (_isMultiPlay)
        {
            _multiPlayManager.SendPlayerMove(_roomId, row * 3 + col);
        }
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        gameLogic.SetState(_isFirstPlayer ? gameLogic.secondPlayerState : gameLogic.firstPlayerState);
    }
}

// AI 플레이
public class AIState : BasePlayerState
{
    public override void OnEnter(GameLogic gameLogic)
    {
        // AI 연산
        var result = MiniMaxAIController.GetBestMove(gameLogic.GetBoard());
        if (result.HasValue)
        {
            HandleMove(gameLogic, result.Value.row, result.Value.col);
        }
        else
        {
            gameLogic.EndGame(GameLogic.GameResult.Draw);
        }

    }

    public override void OnExit(GameLogic gameLogic) { }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, Constants.PlayerType.PlayerB, row, col);
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        gameLogic.SetState(gameLogic.firstPlayerState);
    }
}

// 네트워크 플레이
public class MultiPlayState : BasePlayerState
{
    private Constants.PlayerType _playerType;
    private bool _isFirstPlayer;

    private MultiPlayManager _multiPlayManager;

    public MultiPlayState(bool isFirstPlayer, MultiPlayManager multiPlayManager)
    {
        _isFirstPlayer = isFirstPlayer;
        _playerType =  _isFirstPlayer ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
        _multiPlayManager = multiPlayManager;
    }
    public override void OnEnter(GameLogic gameLogic)
    {
        _multiPlayManager.OnOpponentMove = moveData =>
        {
            var row = moveData.position / 3;
            var col = moveData.position % 3;
            UnityThread.executeInUpdate(() =>
            {
                HandleMove(gameLogic, row, col);
            });
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        _multiPlayManager.OnOpponentMove = null;
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, _playerType, row, col);
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        gameLogic.SetState(_isFirstPlayer ? gameLogic.secondPlayerState : gameLogic.firstPlayerState);
    }
}

public class GameLogic : IDisposable
{
    public BlockController blockController;
    private Constants.PlayerType[,] _board;

    public BasePlayerState firstPlayerState;      // 첫 번째 턴  상태 객체
    public BasePlayerState secondPlayerState;     // 두번째 턴 상태 객체
    private BasePlayerState _currentPlayerState;    // 현재 턴 상태 객체

    private MultiPlayManager _multiPlayManager;

    private string _roomId;

    public enum GameResult
    {
        None,   // 게임 진행 중
        Win,    // 플레이어 승
        Lose,   // 플레이어 패
        Draw    // 비김
    }

    public GameLogic( BlockController blockController, Constants.GameType gameType,
        MultiPlayManager multiPlayManager = null, string roomId = null, bool isFirstPlayer = true )
    {
        this.blockController = blockController;

        // _board 초기화
        _board = new Constants.PlayerType[3, 3];

        switch (gameType)
        {
            case Constants.GameType.SinglePlayer:
                firstPlayerState = new PlayerState(true);
                secondPlayerState = new AIState();

                // 게임 시작
                SetState(firstPlayerState);
                break;
            case Constants.GameType.CoOpPlayer:
                firstPlayerState = new PlayerState(true);
                secondPlayerState = new PlayerState(false);

                // 게임 시작
                SetState(firstPlayerState);
                break;
            case Constants.GameType.MultiPlayer:
                // MultiPlayManager 생성
                _multiPlayManager = new MultiPlayManager((state, roomId) =>
                {
                    _roomId = roomId;
                    switch (state)
                    {
                        case Constants.MultiplayManagerState.CreateRoom:
                            // TODO: 대기화면 표시
                            // GameManager에게 대기 화면 표시 요청
                            break;
                        case Constants.MultiplayManagerState.JoinRoom:
                            firstPlayerState = new MultiPlayState(true, _multiPlayManager);
                            secondPlayerState = new PlayerState(false , _multiPlayManager, roomId);

                            // 게임 시작
                            SetState(firstPlayerState);
                            break;
                        case Constants.MultiplayManagerState.StartGame:
                            firstPlayerState = new PlayerState(true, _multiPlayManager, roomId );
                            secondPlayerState = new MultiPlayState(false, _multiPlayManager);

                            // 게임 시작
                            SetState(firstPlayerState);
                            break;
                        case Constants.MultiplayManagerState.ExitGame:
                            // TODO: Exit Room 처리
                            break;
                        case Constants.MultiplayManagerState.EndGame:
                            // TODO: End Room 처리
                            break;

                    }
                });
                break;
        }


    }

    public void SetState(BasePlayerState state)
    {
        _currentPlayerState?.OnExit(this);
        _currentPlayerState = state;
        _currentPlayerState?.OnEnter(this);
    }

    /// <summary>
    /// _board에 새로운 값을 할당하는 함수
    /// </summary>
    /// <param name="playerType">할당하고자 하는 플레이어 타입</param>
    /// <param name="row">Row</param>
    /// <param name="col">Col</param>
    /// <returns>False가 반환되면 할당할 수 없음, True는 할당이 완료됨</returns>
    public bool SetNewBoardValue(Constants.PlayerType playerType, int row, int col)
    {
        if (_board[row, col] != Constants.PlayerType.None) return false;

        var newMarker = playerType == Constants.PlayerType.PlayerA
            ? Block.MarkerType.O
            : Block.MarkerType.X;
        _board[row, col] = playerType;

        blockController.PlaceMarker(newMarker, row, col);
        return true;
    }

    /// <summary>
    /// 게임 결과 확인 함수
    /// </summary>
    /// <returns>플레이어 기준 게임 결과</returns>
    public GameResult CheckGameResult()
    {
        if (CheckGameWin(Constants.PlayerType.PlayerA)) { return GameResult.Win; }
        if (CheckGameWin(Constants.PlayerType.PlayerB)) { return GameResult.Lose; }
        if (MiniMaxAIController.IsAllBlocksPlaced(_board)) { return GameResult.Draw; }

        return GameResult.None;
    }

    //게임의 승패를 판단하는 함수
    private bool CheckGameWin(Constants.PlayerType playerType)
    {
        int length = _board.GetLength(0);

        // 가로로 마커가 일치하는지 확인
        for (var row = 0; row < length; row++)
        {
            if (_board[row, 0] == playerType && _board[row, 1] == playerType && _board[row, 2] == playerType)
            {
                (int, int)[] blocks = { ( row, 0 ), ( row, 1 ), ( row, 2 ) };
                blockController.SetBlockColor(playerType, blocks);
                return true;
            }
        }

        // 세로로 마커가 일치하는지 확인
        for (var col = 0; col < length; col++)
        {
            if (_board[0, col] == playerType && _board[1, col] == playerType && _board[2, col] == playerType)
            {
                (int, int)[] blocks = { ( 0, col ), ( 1, col ), ( 2, col ) };
                blockController.SetBlockColor(playerType, blocks);
                return true;
            }
        }

        // 대각선 마커 일치하는지 확인
        if (_board[0, 0] == playerType && _board[1, 1] == playerType && _board[2, 2] == playerType)
        {
            (int, int)[] blocks = { ( 0, 0 ), ( 1, 1 ), ( 2, 2 ) };
            blockController.SetBlockColor(playerType, blocks);
            return true;
        }
        if (_board[0, 2] == playerType && _board[1, 1] == playerType && _board[2, 0] == playerType)
        {
            (int, int)[] blocks = { ( 0, 2 ), ( 1, 1 ), ( 2, 0 ) };
            blockController.SetBlockColor(playerType, blocks);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 게임 오버시 호출되는 함수
    /// gameResult에 따라 결과 출력
    /// </summary>
    /// <param name="gameResult">win, lose, draw</param>
    public void EndGame(GameResult gameResult)
    {
        SetState(null);
        firstPlayerState = null;
        secondPlayerState = null;

        GameManager.Instance.OpenGameOverPanel();
    }

    public Constants.PlayerType[,] GetBoard(){return _board;}

    public void Dispose()
    {
        _multiPlayManager?.LeaveRoom(_roomId);
        _multiPlayManager?.Dispose();
    }
}