using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableArticle : MonoBehaviour
{
    /// <summary>
    /// 스테이지 생성 시점에 결정해서 정답 판정에 사용할 값
    /// </summary>
    [field:SerializeField] public int Id { get; set; }

    /// <summary>
    /// 재정의해서 상호작용을 통해 상태가 전환되는 기물이나 주사위 윗면 판단에 사용할 정보
    /// </summary>
    public virtual int State { get => 0; } 

    /// <summary>
    /// 연출 완료 후 실제 작동 시작시 초기화
    /// </summary>
    public void OnStageInit()
    {
        GetComponent<Rigidbody>().isKinematic = false;
    }
}
