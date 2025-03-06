using System;

public struct UpdateScoreRequest
{
    public int score;
}

public struct UpdateScoreResult
{
    public string message;
}

public struct SigninRequest
{
    public string username;
    public string password;
}

public struct SigninResponse
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

[Serializable]
public struct LeaderboardData
{
    public string username;
    public string nickname;
    public int score;
}

[Serializable]
public struct LeaderboardDataList
{
    public LeaderboardData[] leaderboardDatas;
}

public struct SignupRequest
{
    public string username;
    public string nickname;
    public string password;
}