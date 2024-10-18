using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/StageData")]
public class StageData : ScriptableObject
{
    public ArticleData[] articleDatas;
    public SocketData[] socketDatas;

    [System.Serializable]
    public struct ArticleData
    {
        public InteractableArticle prefab;
        public Pose pose;
    }

    [System.Serializable]
    public struct SocketData
    {
        [System.Serializable]
        public struct CheckInt
        {
            public bool check;
            [ShowIf("check")]
            public int value;
        }

        public ArticleSocket prefab;
        public Pose pose;
        public CheckInt id;
        public CheckInt axis;
        public CheckInt state;
    }
}
