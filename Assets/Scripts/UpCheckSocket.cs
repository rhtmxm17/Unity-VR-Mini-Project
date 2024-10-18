using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpCheckSocket : ArticleSocket
{
    protected override void AttachRotation(Transform article)
    {
        // 주사위의 면 벡터중 가장 소켓의 up벡터와 유사한 벡터를 검사
        Vector3 socketUpInArticleSpace = article.InverseTransformDirection(this.transform.up);
        float maxSimilarity = -1f;

        for (int i = 0; i < EnableAxis.Count; i++)
        {
            // 유사성은 사잇각이 가장 작은 것(방향 벡터의 Dot이 최대값에 가까운 것)으로 판단
            float similarity = Vector3.Dot(socketUpInArticleSpace, EnableAxis[i]);
            if (maxSimilarity < similarity)
            {
                maxSimilarity = similarity;
                attachAxisIndex = i;
            }
        }

        //// 주사위 틀에 맞춰서 놓는 코드. 단, 옆면 눈이 보존되지 않음
        //socketAttach.rotation = Quaternion.FromToRotation(EnableAxis[attachAxisIndex], Vector3.up);

        // 윗면만 정렬하는 코드.
        socketAttach.rotation = Quaternion.FromToRotation(article.TransformDirection(EnableAxis[attachAxisIndex]), Vector3.up) * article.rotation;

    }
}
