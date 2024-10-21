using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static partial class Extension
{
    public static Stage.Solution GetAnswerFlag(this int i)
    {
        return (Stage.Solution)(1 << i);
    }
}

public class Stage : MonoBehaviour
{
    public StageData data;
    public UnityEvent OnClear;

    [System.Flags]
    public enum Solution : int
    {
        _0 = 1 << 0,
        _1 = 1 << 1,
        _2 = 1 << 2,
        _3 = 1 << 3,
        _4 = 1 << 4,
        _5 = 1 << 5,
        _6 = 1 << 6,
        _7 = 1 << 7,
        _8 = 1 << 8,
        _9 = 1 << 9,
        _10 = 1 << 10,
        _11 = 1 << 11,
        _12 = 1 << 12,
        _13 = 1 << 13,
        _14 = 1 << 14,
        _15 = 1 << 15,

        Undef = 1 << -1, // 1 << 31, 실제 사용은 고려하지 않음, Everything 및 디버깅용
    }

    [System.Serializable]
    public class SolutionSet
    {
        // 기본값 Everything으로 따로 설정하지 않은 경우(Undef를 포함한 아무 값) 항상 통과할 수 있도록 함
        public Solution id = (Solution)~0;
        public Solution axis = (Solution)~0;
        public Solution state = (Solution)~0;
    }

    private SocketAnswerChecker[] checkers;

    private class SocketAnswerChecker
    {
        public bool IsCorrect { get; private set; }

        public event UnityAction OnChecked;

        public SolutionSet solution;

        public void CheckAnswer(ArticleSocket.SelectedChangedEventArgs args)
        {
            IsCorrect = solution.id.HasFlag(args.articleId.GetAnswerFlag()) &&
                solution.axis.HasFlag(args.selectedAxisIndex.GetAnswerFlag()) &&
                solution.state.HasFlag(args.articleState.GetAnswerFlag());

            OnChecked?.Invoke();
        }
    }

    private void Start()
    {
        SetupStage();
    }

    public void SetupStage()
    {
        if (data == null)
        {
            Debug.LogWarning("[Stage] 스테이지 정보가 비어있습니다.");
            return;
        }

        // 기물 생성
        for (int i = 0; i < data.articleDatas.Length; i++)
        {
            var article = Instantiate(data.articleDatas[i].prefab, this.transform);
            article.transform.SetLocalPositionAndRotation(data.articleDatas[i].pose.position, data.articleDatas[i].pose.rotation);
            article.Id = i;
        }

        // 소켓 생성 및 정답 정보 입력
        checkers = new SocketAnswerChecker[data.socketDatas.Length];
        for (int i = 0; i < data.socketDatas.Length; i++)
        {
            var socket = Instantiate(data.socketDatas[i].prefab, this.transform);
            socket.transform.SetLocalPositionAndRotation(data.socketDatas[i].pose.position, data.socketDatas[i].pose.rotation);

            checkers[i] = new()
            {
                solution = data.socketDatas[i].solution,
            };

            socket.OnSelectedChanged += checkers[i].CheckAnswer;
            checkers[i].OnChecked += CheckClear;
        }
    }

    private void CheckClear()
    {
        foreach (var checker in checkers)
        {
            // 오답이 있을 경우 종료
            if (!checker.IsCorrect)
                return;
        }

        Debug.Log("클리어 확인");
        OnClear?.Invoke();
    }
}
