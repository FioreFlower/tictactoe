using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardItem : MonoBehaviour
{
    [SerializeField] private TMP_Text rankingNumberText;
    [SerializeField] private TMP_Text userNickNameText;
    [SerializeField] private TMP_Text userScoreText;


    public int Index { get; private set; }

    public void SetItem( LeaderboardData item, int index)
    {
        Index = index;

        // image.sprite = Resources.Load<Sprite>(item.imageFileName);
        userNickNameText.text = item.nickname;
        userScoreText.text = item.score.ToString();
        rankingNumberText.text = (index + 1).ToString();
    }
}