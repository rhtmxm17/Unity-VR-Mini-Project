using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Stage;

[ExecuteInEditMode]
public class SocketDataSetter : MonoBehaviour
{
    public List<InteractableArticle> solutionArticles;
    public Solution solutionAxis = (Solution)~0;
    public Solution solutionState = (Solution)~0;

    private void Start()
    {
        var stageEditors = FindObjectsByType(typeof(StageEditor), FindObjectsSortMode.None);
        if (stageEditors.Length != 1)
        {
            Debug.LogWarning("스테이지 편집 툴이 아닌 씬에서 SocketDataSetter가 사용되고 있거나 StageEditor가 너무 많습니다.");
        }
    }
}
