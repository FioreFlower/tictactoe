using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(RectTransform))]
public class ScrollViewController : MonoBehaviour
{
    [SerializeField] private float cellHeight;

    private ScrollRect _scrollRect;
    private RectTransform _rectTransform;

    private List<LeaderboardData> _items;                                          // Cell에 표시할 Item 정보
    private LinkedList<LeaderboardItem> _visibleCells = new LinkedList<LeaderboardItem>();    // 화면에 표시되고 있는 Cell 정보

    private float _lastYValue = 1f;

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        LoadData();
    }

    /// <summary>
    /// 현재 보여질 Cell 인덱스를 반환하는 메서드
    /// </summary>
    /// <returns>startIndex: 가장 위에 표시될 Cell 인덱스, endIndex: 가장 아래에 표시될 Cell 인덱스</returns>
    private (int startIndex, int endIndex) GetVisibleIndexRange()
    {
        var visibleRect = new Rect(
            _scrollRect.content.anchoredPosition.x,
            _scrollRect.content.anchoredPosition.y,
            _rectTransform.rect.width,
            _rectTransform.rect.height
        );

        // 스크롤 위치에 따른 시작 인덱스 계산
        var startIndex = Mathf.FloorToInt(visibleRect.y / cellHeight);

        // 화면에 보이게 될 Cell 개수 계산
        int visibleCount = Mathf.CeilToInt(visibleRect.height / cellHeight);


        // 버퍼 추가
        startIndex = Mathf.Max(0, startIndex - 1); // startIndex가 0보다 크다면 startIndex -1, 아니면 0
        visibleCount += 2;

        return (startIndex, startIndex + visibleCount - 1);
    }

    /// <summary>
    /// 특정 인덱스가 화면에 보여야하는지 여부를 판단하는 메서드
    /// </summary>
    /// <param name="index">특정 인덱스</param>
    /// <returns>true: 보여야함, false: 보이지 않아야함</returns>
    private bool IsVisibleIndex(int index)
    {
        var (startIndex, endIndex) = GetVisibleIndexRange();
        return startIndex <= index && index <= endIndex;
    }

    /// <summary>
    /// _items에 있는 값을 Scroll View에 표시하는 함수
    /// _items에 새로운 값이 추가되거나 기존 값이 삭제되면 호출됨
    /// </summary>
    private void ReloadData()
    {
        var contentSizeDelta = _scrollRect.content.sizeDelta;
        contentSizeDelta.y = _items.Count * cellHeight;
        _scrollRect.content.sizeDelta = contentSizeDelta;

        // 화면에 보이는 영역에 Cell 추가
        var (startIndex, endIndex) = GetVisibleIndexRange();
        var maxEndIndex = Mathf.Min(endIndex, _items.Count -1);
        for (int i = startIndex; i < maxEndIndex; i++)
        {
            // Cell 만들기
            var cellObject = ObjectPool.Instance.GetObject();
            var cell = cellObject.GetComponent<LeaderboardItem>();
            cell.SetItem(_items[i], i);
            cell.transform.localPosition = new Vector3(0, -i * cellHeight, 0);

            _visibleCells.AddLast(cell);
        }
    }

    private void LoadData()
    {
        NetworkManager.Instance.LoadLeaderboard(list =>
        {
            _items = list.ToList();
            ReloadData();
        }, () =>
        {
            Debug.Log("리더보드 불러오기 실패");
        });
    }

    #region Scroll Rect Event

    public void OnValueChanged(Vector2 value)
    {
        if (_lastYValue < value.y)
        {
            /////////////////////////////////////////////////////
            // 위로 스크롤
            var firstCell = _visibleCells.First.Value;
            var newFirstIndex = firstCell.Index - 1;

            // 1. 상단에 새로운 셀이 필요핝지 확인 후 필요하면 추가
            if (IsVisibleIndex(newFirstIndex))
            {
                var cell = ObjectPool.Instance.GetObject().GetComponent<LeaderboardItem>();
                cell.SetItem(_items[newFirstIndex], newFirstIndex);
                cell.transform.localPosition = new Vector3(0, -newFirstIndex * cellHeight, 0);
                _visibleCells.AddFirst(cell);
            }

            // 2. 하단에 있는 셀이 화면에서 벗어나면 제거
            var lastCell = _visibleCells.Last.Value;
            if (!IsVisibleIndex(lastCell.Index))
            {
                ObjectPool.Instance.ReturnObject(lastCell.gameObject);
                _visibleCells.RemoveLast();
            }
        }
        else
        {
            /////////////////////////////////////////////////////
            // 아래로 스크롤
            var lastCell = _visibleCells.Last.Value;
            var newLastIndex = lastCell.Index + 1;

            // 1. 하단에 새로운 셀이 필요한지 확인 후 필요하면 추가
            if (IsVisibleIndex(newLastIndex))
            {
                if (newLastIndex >= _items.Count) return; // index가 _items 범위를 벗어날 경우

                var cell = ObjectPool.Instance.GetObject().GetComponent<LeaderboardItem>();
                cell.SetItem(_items[newLastIndex], newLastIndex);
                cell.transform.localPosition = new Vector3(0, -newLastIndex * cellHeight, 0);
                _visibleCells.AddLast(cell);
            }

            var firstCell = _visibleCells.First.Value;

            // 2. 상단에 있는 셀이 화면에서 벗어나면 제거
            if (!IsVisibleIndex(firstCell.Index))
            {
                ObjectPool.Instance.ReturnObject(firstCell.gameObject);
                _visibleCells.RemoveFirst();
            }

        }

        _lastYValue = value.y;
    }

    #endregion
}
