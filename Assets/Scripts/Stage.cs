using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Stage : MonoBehaviour
{
    public StageData data;
    public UnityEvent OnClear;

    private SocketAnswerChecker[] checkers;

    private class SocketAnswerChecker
    {
        public bool IsCorrect { get; private set; }

        public event UnityAction OnChecked;

        public int id = -1;
        public int axis = -1;
        public int state = -1;

        public void CheckAnswer(ArticleSocket.SelectedChangedEventArgs args)
        {
            IsCorrect = (id < 0 || id == args.articleId) &&
                (axis < 0 || axis == args.selectedAxisIndex) &&
                (state < 0 || state == args.articleState);

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

            checkers[i] = new();
            if (data.socketDatas[i].id.check)
                checkers[i].id = data.socketDatas[i].id.value;
            if (data.socketDatas[i].axis.check)
                checkers[i].axis = data.socketDatas[i].axis.value;
            if (data.socketDatas[i].state.check)
                checkers[i].state = data.socketDatas[i].state.value;

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
