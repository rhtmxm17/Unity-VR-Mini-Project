#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode] // 에디터 모드에서 라이프사이클 호출
public class StageEditor : MonoBehaviour
{
    public StageData stageData;
    public Transform cluesParent;

    [ContextMenu("Save Stage Data")]
    public void SaveStageData()
    {
        // 등록된 StageData가 없다면 생성
        if (stageData == null)
        {
            string fullPath = EditorUtility.SaveFilePanel("스테이지 데이터를 생성", "Assets", "New Stage Data.asset", "asset");
            if (string.IsNullOrEmpty(fullPath))
                return;

            stageData = ScriptableObject.CreateInstance<StageData>();
            var relativePath = "Assets/" + fullPath.Substring(Application.dataPath.Length + 1);
            AssetDatabase.CreateAsset(stageData, relativePath);
            Debug.Log($"{relativePath} 으로 저장됨");
        }

        var sockets = (ArticleSocket[])FindObjectsByType(typeof(ArticleSocket), FindObjectsSortMode.None);
        var articles = (InteractableArticle[])FindObjectsByType(typeof(InteractableArticle), FindObjectsSortMode.None);

        stageData.articleDatas = new StageData.ArticleData[articles.Length];
        stageData.socketDatas = new StageData.SocketData[sockets.Length];
        stageData.clueDatas = new StageData.ClueData[cluesParent.childCount];

        // 기물 저장
        for (int i = 0; i < articles.Length; i++)
        {
            // 배열에 저장된 순서(인덱스)는 읽어올 때 id로 사용된다
            stageData.articleDatas[i].prefab = PrefabUtility.GetCorrespondingObjectFromSource(articles[i]);
            stageData.articleDatas[i].pose = new Pose(articles[i].transform.position, articles[i].transform.rotation);
        }

        // 소켓 저장
        for (int i = 0; i < sockets.Length; i++)
        {
            stageData.socketDatas[i].prefab = PrefabUtility.GetCorrespondingObjectFromSource(sockets[i]);
            stageData.socketDatas[i].pose = new Pose(sockets[i].transform.position, sockets[i].transform.rotation);

            SocketDataSetter dataSetter = sockets[i].GetComponent<SocketDataSetter>();
            Stage.Solution solutionIDs = 0;

            if (dataSetter.solutionArticles.Count == 0)
            {
                // 정답 기물 목록이 비어있다면 모든 정답을 허용으로 처리
                solutionIDs = (Stage.Solution)~0;
            }
            else
            {
                // id(== 저장된 순서)를 찾아 정답 목록에 추가
                foreach (var article in dataSetter.solutionArticles)
                {
                    for (int id = 0; id < articles.Length; id++)
                    {
                        if (articles[id] == article)
                        {
                            solutionIDs |= id.GetAnswerFlag();
                            break;
                        }
                    }
                }
            }

            stageData.socketDatas[i].solution = new Stage.SolutionSet()
            {
                id = solutionIDs,
                axis = dataSetter.solutionAxis,
                state = dataSetter.solutionState,
            };
        }

        // 맵 저장
        for (int i = 0; i < cluesParent.childCount; i++)
        {
            stageData.clueDatas[i].prefab = PrefabUtility.GetCorrespondingObjectFromSource(cluesParent.GetChild(i).gameObject);
            stageData.clueDatas[i].pose = new Pose(cluesParent.GetChild(i).position, cluesParent.GetChild(i).rotation);
        }

        EditorUtility.SetDirty(stageData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [ContextMenu("Load Stage Data")]
    public void LoadStageData()
    {
        if (stageData == null)
        {
            Debug.LogWarning("스테이지 데이터가 선택되지 않았습니다.");
            return;
        }

        if ((0 != FindObjectsByType(typeof(ArticleSocket), FindObjectsSortMode.None).Length) ||
            (0 != FindObjectsByType(typeof(InteractableArticle), FindObjectsSortMode.None).Length))
        {
            Debug.LogWarning("씬에 소켓 또는 기물이 존재합니다.");
            return;
        }

        // 기물 생성
        InteractableArticle[] articles = new InteractableArticle[stageData.articleDatas.Length];
        for (int i = 0; i < stageData.articleDatas.Length; i++)
        {
            articles[i] = (InteractableArticle)PrefabUtility.InstantiatePrefab(stageData.articleDatas[i].prefab);
            articles[i].transform.SetPositionAndRotation(stageData.articleDatas[i].pose.position, stageData.articleDatas[i].pose.rotation);
        }

        // 소켓 생성 및 정답 정보 입력
        for (int i = 0; i < stageData.socketDatas.Length; i++)
        {
            var socket = (ArticleSocket)PrefabUtility.InstantiatePrefab(stageData.socketDatas[i].prefab);
            socket.transform.SetPositionAndRotation(stageData.socketDatas[i].pose.position, stageData.socketDatas[i].pose.rotation);
            var dataSetter = socket.gameObject.AddComponent<SocketDataSetter>();

            dataSetter.solutionAxis = stageData.socketDatas[i].solution.axis;
            dataSetter.solutionState = stageData.socketDatas[i].solution.state;

            // 정답 기물 목록은 id를 참조로 변환해서 불러오기
            dataSetter.solutionArticles = new();
            if (stageData.socketDatas[i].solution.id != (Stage.Solution)~0)
            {
                for (int id = 0; id < stageData.articleDatas.Length; id++)
                {
                    if (stageData.socketDatas[i].solution.id.HasFlag(id.GetAnswerFlag()))
                    {
                        dataSetter.solutionArticles.Add(articles[id]);
                    }
                }
            }
        }

        // 맵 생성
        for (int i = 0; i < stageData.clueDatas.Length; i++)
        {
            GameObject clue = (GameObject)PrefabUtility.InstantiatePrefab(stageData.clueDatas[i].prefab, cluesParent);
            clue.transform.SetPositionAndRotation(stageData.clueDatas[i].pose.position, stageData.clueDatas[i].pose.rotation);
        }
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
            return;

        Debug.Log("StageEditor 시작");
        EditorApplication.hierarchyChanged += AddDataSetter;
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
            return;

        Debug.Log("StageEditor 정지");
        EditorApplication.hierarchyChanged -= AddDataSetter;
    }

    private void AddDataSetter()
    {
        var sockets = (ArticleSocket[])FindObjectsByType(typeof(ArticleSocket), FindObjectsSortMode.None);

        foreach (var socket in sockets)
        {
            if (! socket.TryGetComponent<SocketDataSetter>(out _))
            {
                socket.gameObject.AddComponent<SocketDataSetter>();
                Debug.Log("[StageEditor] SocketDataSetter가 자동으로 추가됨");
            }
        }
    }
}
#endif
