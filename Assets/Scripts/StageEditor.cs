using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode] // 에디터 모드에서 라이프사이클 호출
public class StageEditor : MonoBehaviour
{
    public StageData stageData;

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

        for (int i = 0; i < articles.Length; i++)
        {
            // 배열에 저장된 순서(인덱스)는 읽어올 때 id로 사용된다
            stageData.articleDatas[i].prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(articles[i]);
            stageData.articleDatas[i].pose = new Pose(articles[i].transform.position, articles[i].transform.rotation);
        }

        for (int i = 0; i < sockets.Length; i++)
        {
            stageData.socketDatas[i].prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(sockets[i]);
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
    }

    [ContextMenu("Load Stage Data")]
    public void LoadStageData()
    {
        if (stageData == null)
        {
            Debug.LogWarning("스테이지 데이터가 선택되지 않음");
            return;
        }

        Debug.LogError("TODO: 스테이지 편집 툴 형식에 맞게 불러오기");
    }

    private void OnEnable()
    {
        Debug.Log("StageEditor 시작");
        EditorApplication.hierarchyChanged += AddDataSetter;
    }

    private void OnDisable()
    {
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
