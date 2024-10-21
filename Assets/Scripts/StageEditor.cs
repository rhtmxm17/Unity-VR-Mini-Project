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

        Debug.LogError("TODO: SocketDataSetter를 참고해 StageData로 저장");
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
