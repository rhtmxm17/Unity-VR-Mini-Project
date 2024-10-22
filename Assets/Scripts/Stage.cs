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
        //Everything = ~0
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
    private List<GameObject> stageElements;

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

    public void SetupStage()
    {
        if (data == null)
        {
            Debug.LogWarning("[Stage] 스테이지 정보가 비어있습니다.");
            return;
        }

        if (stageElements != null)
        {
            Debug.LogWarning("[Stage] 스테이지가 중복 생성된 것으로 예상됩니다.");
        }

        stageElements = new List<GameObject>(data.articleDatas.Length + data.socketDatas.Length + data.clueDatas.Length);
        float directSpendingTime = 1.5f;

        // 기물 생성
        for (int i = 0; i < data.articleDatas.Length; i++)
        {
            var article = Instantiate(data.articleDatas[i].prefab, this.transform);
            article.transform.SetLocalPositionAndRotation(data.articleDatas[i].pose.position, data.articleDatas[i].pose.rotation);
            article.Id = i;

            stageElements.Add(article.gameObject);

            // 랜덤 딜레이 후 활성화(출현 연출)
            article.gameObject.SetActive(false);
            float directStartTime = Random.value * directSpendingTime;
            StartCoroutine(DelayedActivation(article.gameObject, new WaitForSeconds(directStartTime)));
            StartCoroutine(DelayedDelegate(article.OnStageInit, new WaitForSeconds(directStartTime + 1f)));
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

            stageElements.Add(socket.gameObject);

            socket.gameObject.SetActive(false);
            float directStartTime = Random.value * directSpendingTime;
            StartCoroutine(DelayedActivation(socket.gameObject, new WaitForSeconds(directStartTime)));
        }

        // 힌트 등 맵 요소 생성
        for (int i = 0; i < data.clueDatas.Length; i++)
        {
            GameObject clue = Instantiate(data.clueDatas[i].prefab, this.transform);
            clue.transform.SetLocalPositionAndRotation(data.clueDatas[i].pose.position, data.clueDatas[i].pose.rotation);

            stageElements.Add(clue);

            // 맵 요소의 경우 Animator가 존재하는지 검사
            if (clue.TryGetComponent(out Animator _))
            {
                clue.SetActive(false);
                StartCoroutine(DelayedActivation(clue, new WaitForSeconds(Random.value * directSpendingTime)));
            }
        }
    }

    public void ClearStageElements()
    {
        int hash = Animator.StringToHash("Disappear");
        foreach (GameObject element in stageElements)
        {
            if (element.TryGetComponent(out Animator animator))
            {
                animator.SetTrigger(hash);
                Destroy(element, 1.5f);
            }
            else
            {
                Destroy(element);
            }
        }

        stageElements = null;
    }

    private IEnumerator DelayedActivation(GameObject target, YieldInstruction delay)
    {
        yield return delay;
        target.SetActive(true);
    }

    private IEnumerator DelayedDelegate(UnityAction action, YieldInstruction delay)
    {
        yield return delay;
        action?.Invoke();
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
        ClearStageElements();
        OnClear?.Invoke();
    }
}
