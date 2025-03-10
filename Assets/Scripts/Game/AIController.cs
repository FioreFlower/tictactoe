// using System.Collections.Generic;
// using UnityEngine;
// using Random = UnityEngine.Random;

/*
public static class AIController
{
    private const float randomMoveProbability = 0.3f; // 30% AI가 실수를 유발
    
    /// <summary>
    /// 보드 상태를 받아 AI가 둘 다음 수를 결정
    /// </summary>
    /// <param name="board">현재 보드 상태 (예: 3x3 배열)</param>
    /// <returns></returns>
    public static (int row, int col) FindNextMove2(GameManager.PlayerType[,] board)
    {
        int lastRow = board.GetLength(0); // 3
        int lastCol = board.GetLength(1); // 3
        const int startPoint = 0;
        
        // 상, 하, 좌, 우 방향을 정의
        var directions = new (int dRow, int dCol)[]
        {
            ( 1,  0), // 아래
            (-1,  0), // 위
            ( 0,  1), // 오른쪽
            ( 0, -1)  // 왼쪽
        };
        
        for (int row = startPoint; row < lastRow; row++)
        {
            for (int col = startPoint; col < lastCol; col++)
            {
                if (board[row, col] != GameManager.PlayerType.PlayerA) continue;
                
                foreach (var ( dRow, dCol) in directions)
                {
                    var newRow = row + dRow;
                    var newCol = col + dCol;

                    if (newRow >= lastRow || newRow < startPoint || newCol >= lastCol || newCol < startPoint) continue; // 보드 범위를 벗어난 경우
                    
                    if (board[newRow, newCol] == GameManager.PlayerType.None)
                    {
                        return (newRow, newCol);
                    }
                }
            }
        }
        
        // 이 경우의 수에 도달해선 안됨.
        return (-1, -1);
    }

    public static (int row, int col) FindNextMove(GameManager.PlayerType[,] board)
    {
        int lastRow = board.GetLength(0); // 3
        int lastCol = board.GetLength(1); // 3
        int startPoint = 0;
        
        List<(int row, int col)> emtpyBlocks = new();
        
        for (int row = startPoint; row < lastRow; row++)
        {
            for (int col = startPoint; col < lastCol; col++)
            {
                if (board[row, col] == GameManager.PlayerType.None)
                {
                    emtpyBlocks.Add((row, col));
                }
            }
        }

        // 가능한 수가 없다면
        // 일반적인 경우 이 조건에 도달할 수 없음
        if (emtpyBlocks.Count == 0)
        {
            Debug.Log("No emtpy blocks found : WHYYYYYYYYYYYYYYY?!!!");
            return (-1, -1);
        }
        
        // 일정 확률로 실수를 유도.
        if (Random.value < randomMoveProbability)
        {
            Debug.Log("AI가 실수함");
            int randomIndex = Random.Range(0, emtpyBlocks.Count);
            return emtpyBlocks[randomIndex];
        }
        
        // 1. AI(PlayerB)가 승리할 수 있는 수가 있는지 검사
        foreach (var move in emtpyBlocks)
        {
            if (IsWinningMove(board, move.row, move.col, GameManager.PlayerType.PlayerB))
            {
                return move;
            }
        }

        // 2. 상대(PlayerA)의 승리 수를 막는 수가 있는지 검사
        foreach (var move in emtpyBlocks)
        {
            if (IsWinningMove(board, move.row, move.col, GameManager.PlayerType.PlayerA))
            {
                return move;
            }
        }
        
        // 3. 중앙을 우선적으로 선택합니다.
        int centerRow = lastRow / 2;
        int centerCol = lastCol / 2;
        if (board[centerRow, centerCol] == GameManager.PlayerType.None)
        {
            return (centerRow, centerCol);
        }

        // 4. 코너를 선택합니다.
        var corners = new List<(int row, int col)>
        {
            (0, 0),
            (0, lastCol - 1),
            (lastRow - 1, 0),
            (lastRow - 1, lastCol - 1)
        };
        
        foreach (var corner in corners)
        {
            if (board[corner.row, corner.col] == GameManager.PlayerType.None)
                return corner;
        }

        // 5. 그 외에는 가능한 수 중 하나를 무작위로 선택합니다.
        int index = Random.Range(0, emtpyBlocks.Count);
        return emtpyBlocks[index];
    }
    
    /// <summary>
    /// 주어진 위치에 플레이어의 말을 두었을 때 승리할 수 있는지 확인합니다.
    /// </summary>
    private static bool IsWinningMove(GameManager.PlayerType[,] board, int row, int col, GameManager.PlayerType player)
    {
        // 임시로 말을 놓고 승리 여부를 체크한 후 원상복구합니다.
        board[row, col] = player;
        bool win = CheckWin(board, player);
        board[row, col] = GameManager.PlayerType.None;
        return win;
    }

    /// <summary>
    /// 보드에서 해당 플레이어가 승리 상태인지 확인합니다.
    /// </summary>
    private static bool CheckWin(GameManager.PlayerType[,] board, GameManager.PlayerType player)
    {
        int n = board.GetLength(0);

        // 행 체크
        for (int row = 0; row < n; row++)
        {
            bool win = true;
            for (int col = 0; col < n; col++)
            {
                if (board[row, col] != player)
                {
                    win = false;
                    break;
                }
            }
            if (win) return true;
        }

        // 열 체크
        for (int col = 0; col < n; col++)
        {
            bool win = true;
            for (int row = 0; row < n; row++)
            {
                if (board[row, col] != player)
                {
                    win = false;
                    break;
                }
            }
            if (win) return true;
        }

        // 대각선 체크 (좌상단 ~ 우하단)
        bool diagWin = true;
        for (int i = 0; i < n; i++)
        {
            if (board[i, i] != player)
            {
                diagWin = false;
                break;
            }
        }
        if (diagWin) return true;

        // 역대각선 체크 (우상단 ~ 좌하단)
        diagWin = true;
        for (int i = 0; i < n; i++)
        {
            if (board[i, n - 1 - i] != player)
            {
                diagWin = false;
                break;
            }
        }
        if (diagWin) return true;

        return false;
    }
}
*/