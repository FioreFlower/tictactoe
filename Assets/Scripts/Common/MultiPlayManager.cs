using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SocketIOClient;
using UnityEngine;

public class RoomData
{
    [JsonProperty("roomId")]
    public string roomId { get; set; }
}

public class UserData
{
    [JsonProperty("userId")]
    public string userId { get; set; }
}

public class MoveData
{
    [JsonProperty("position")]
    public int position { get; set; }
}

public class MessageData
{
    [JsonProperty("nickName")]
    public string nickName { get; set; }
    [JsonProperty("message")]
    public string message { get; set; }
}

public class MultiPlayManager : IDisposable
{
    private SocketIOUnity _socket;

    private event Action<Constants.MultiplayManagerState, string> _onMultiPlayStateChanged;
    public Action<MessageData> OnReceiveMessage;
    public Action<MoveData> OnOpponentMove;

    public MultiPlayManager(Action<Constants.MultiplayManagerState, string> onMultiPlayStateChanged)
    {
        _onMultiPlayStateChanged = onMultiPlayStateChanged;
        var uri = new Uri(Constants.GameServerURL);
        _socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        _socket.OnUnityThread("createRoom", CreateRoom);
        _socket.OnUnityThread("joinRoom", JoinRoom);
        _socket.OnUnityThread("startGame", StartGame);
        _socket.OnUnityThread("exitRoom", ExitRoom);
        _socket.OnUnityThread("endGame", EndGame);
        _socket.OnUnityThread("doOpponent", DoOpponent);
        _socket.OnUnityThread("receiveMessage", ReceiveMessage);

        _socket.Connect();
    }

    // 서버로부터 상대방의 마커 정보를 받기 위한 메서드
    private void DoOpponent(SocketIOResponse response)
    {
        var data = response.GetValue<MoveData>();
        OnOpponentMove?.Invoke(data);
    }

    public void SendPlayerMove(string roomId, int position)
    {
        _socket.Emit("doPlayer", new {roomId, position});
    }

    private void CreateRoom(SocketIOResponse response)
    {
        Debug.Log("createRoom");
        var data = response.GetValue<RoomData>();

        _onMultiPlayStateChanged?.Invoke(Constants.MultiplayManagerState.CreateRoom, data.roomId);
    }

    private void JoinRoom(SocketIOResponse response)
    {
        Debug.Log("joinRoom");
        var data = response.GetValue<RoomData>();

        _onMultiPlayStateChanged?.Invoke(Constants.MultiplayManagerState.JoinRoom, data.roomId);
    }

    private void StartGame(SocketIOResponse response)
    {
        Debug.Log("StartGame");
        var data = response.GetValue<RoomData>();

        _onMultiPlayStateChanged?.Invoke(Constants.MultiplayManagerState.StartGame, data.roomId);
    }

    private void ExitRoom(SocketIOResponse response)
    {
        _onMultiPlayStateChanged?.Invoke(Constants.MultiplayManagerState.ExitGame, null);
    }

    private void EndGame(SocketIOResponse response)
    {
        Debug.Log("gameEnded");
        var data = response.GetValue<UserData>();

        _onMultiPlayStateChanged?.Invoke(Constants.MultiplayManagerState.EndGame, null);
    }

    private void ReceiveMessage(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        OnReceiveMessage?.Invoke(data);
    }

    public void SendMessage(string roomId, string nickName, string message)
    {
        _socket.Emit("sendMessage", new { roomId, nickName, message });
    }

    public void LeaveRoom(string roomId)
    {
        _socket.Emit("leaveRoom", new { roomId });
    }

    public void Dispose()
    {
        if (_socket != null)
        {
            _socket.Disconnect();
            _socket.Dispose();
            _socket = null;
        }
    }
}
