using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPanelController : MonoBehaviour
{
    [Header("User Infos")]
    [SerializeField] private TMP_Text userNickName;
    [SerializeField] private TMP_Text userScore;

    [Header("Scroll View")]
    [SerializeField] private ScrollRect scrollRect;

    public void Start()
    {
        userNickName.text = GameManager.Instance.UserNickName;
        userScore.text = GameManager.Instance.PlayerScore + " Points";
    }
}
