using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSceneManager : MonoBehaviour
{
    [SerializeField] Stage stage;
    [SerializeField] List<StageData> stageList = new();

    private int currentStage = 0;

    private IEnumerator Start()
    {
        if (stage == null || stageList.Count == 0)
        {
            Debug.LogWarning("[StageSceneManager] 스테이지 정보 누락");
            yield break;
        }

        yield return null;

        stage.OnClear.AddListener(NextStage);
        stage.data = stageList[currentStage];
        stage.SetupStage();
    }

    private void NextStage()
    {
        currentStage++;

        if(currentStage >= stageList.Count)
        {
            // 모든 스테이지를 완료한 경우
            Debug.Log("[StageSceneManager] 정의되지 않은 동작: 더이상 스테이지가 없습니다.");
            return;
        }

        stage.data = stageList[currentStage];
        stage.SetupStage();
    }
}
