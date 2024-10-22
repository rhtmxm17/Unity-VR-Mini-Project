using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/StageData")]
public class StageData : ScriptableObject
{
    public ArticleData[] articleDatas;
    public SocketData[] socketDatas;
    public ClueData[] clueDatas;

    [System.Serializable]
    public struct ArticleData
    {
        public InteractableArticle prefab;
        public Pose pose;
    }

    [System.Serializable]
    public struct SocketData
    {
        public ArticleSocket prefab;
        public Pose pose;

        public Stage.SolutionSet solution;
    }

    [System.Serializable]
    public struct ClueData
    {
        public GameObject prefab;
        public Pose pose;
    }
}
