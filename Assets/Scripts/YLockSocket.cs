using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YLockSocket : ArticleSocket
{
    protected override void AttachRotation(Transform article)
    {
        // EnableAxis는 여러개고 상호작용 대상은 하나이므로 상호작용 대상을 소켓의 로컬 공간으로 가져와서 계산한다
        Vector3 interactableForwardInSocketSpace = this.transform.InverseTransformDirection(article.forward);
        float maxSimilarity = -1f;

        for (int i = 0; i < EnableAxis.Count; i++)
        {
            // 유사성은 사잇각이 가장 작은 것(방향 벡터의 Dot이 최대값에 가까운 것)으로 판단
            float similarity = Vector3.Dot(interactableForwardInSocketSpace, EnableAxis[i]);
            if (maxSimilarity < similarity)
            {
                maxSimilarity = similarity;
                attachAxisIndex = i;
            }
        }

        socketAttach.forward = transform.TransformDirection(EnableAxis[attachAxisIndex]);
    }
}
