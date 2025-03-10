using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChattingPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private GameObject messageTextPrefab;
    [SerializeField] private Transform messageParent;

    private MultiPlayManager multiPlayManager;
    private string _roomId;

    public void OnEndEditInputField(string messageText)
    {
        var messageTextObject = Instantiate(messageTextPrefab, messageParent);
        messageTextObject.GetComponent<TMP_Text>().text = messageText;
        messageInputField.text = "";

        if (_roomId != null && multiPlayManager != null)
        {
            multiPlayManager.SendMessage(_roomId, "홍길동", messageText);
        }
    }

    private void Start()
    {
        messageInputField.interactable = false;
        multiPlayManager = new MultiPlayManager((state, id) =>
        {
            switch (state)
            {
                case Constants.MultiplayManagerState.CreateRoom:
                    Debug.Log("## Create room");
                    _roomId = id;
                    break;
                case Constants.MultiplayManagerState.JoinRoom:
                    Debug.Log("## Join room");
                    _roomId = id;
                    messageInputField.interactable = true;
                    break;
                case Constants.MultiplayManagerState.StartGame:
                    Debug.Log("## Start game");
                    messageInputField.interactable = true;
                    break;
                case Constants.MultiplayManagerState.EndGame:
                    Debug.Log("## End game");
                    break;
            }
        });
        multiPlayManager.OnReceiveMessage = OnReceiveMessage;
    }

    private void OnReceiveMessage(MessageData messageData)
    {
        UnityThread.executeInUpdate(() =>
        {
            var messageTextObject = Instantiate(messageTextPrefab, messageParent);
            messageTextObject.GetComponent<TMP_Text>().text = messageData.nickName + " : " + messageData.message;
        });
    }

    private void OnApplicationQuit()
    {
        multiPlayManager.Dispose();
    }
}
