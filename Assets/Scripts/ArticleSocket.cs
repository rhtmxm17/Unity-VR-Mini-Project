using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class ArticleSocket : MonoBehaviour
{
    public List<Vector3> EnableAxis;

    /// <summary>
    /// 선택된(놓여있는) 상호작용 대상이 있을때만 축 번호를 반환<br/>
    /// 그렇지 않다면 -1
    /// </summary>
    public int SelectedAxisIndex
    {
        get
        {
            if (socketInteractor.hasSelection) 
                return attachAxisIndex;
            else
                return -1;
        }
    }

    public event UnityAction<SelectedChangedEventArgs> OnSelectedChanged;

    public struct SelectedChangedEventArgs
    {
        public int selectedAxisIndex;
        public int articleId;
        public int articleState;
    }

    private XRSocketInteractor socketInteractor;
    private Coroutine updateAttachRoutine;
    private InteractableArticle selectedArticle;

    // 소켓 Attach 로직에서 사용되야 할 정보
    protected Transform socketAttach;
    protected int attachAxisIndex = 0;

    protected virtual void Awake()
    {
        socketInteractor = GetComponent<XRSocketInteractor>();
    }

    protected virtual void Start()
    {
        // 소켓에 놓을 방향 설정 관련
        socketInteractor.hoverEntered.AddListener(EnterUpdateAttach);
        socketInteractor.hoverExited.AddListener(ExitUpdateAttach);
        socketInteractor.selectEntered.AddListener(ExitUpdateAttach); // 놓이고 나서면 갱신할 필요가 없으므로 중지
        socketAttach = socketInteractor.attachTransform;

        // 소켓에 놓인 기물 정보 관련
        socketInteractor.selectEntered.AddListener(OnSelectEntered);
        socketInteractor.selectExited.AddListener(OnSelectExited);

        // 디버깅
        socketInteractor.selectEntered.AddListener(_ =>
        {
            Debug.Log($"감지된 축: {attachAxisIndex}");
        });
    }


    private void EnterUpdateAttach(HoverEnterEventArgs args)
    {
        if (EnableAxis.Count == 0)
            return;

        if (args.interactableObject is XRGrabInteractable grabInteractable)
        {
            updateAttachRoutine = StartCoroutine(UpdateAttachRotation(grabInteractable));
        }
    }

    private void ExitUpdateAttach(HoverExitEventArgs args) => ExitUpdateAttach();

    private void ExitUpdateAttach(SelectEnterEventArgs args) => ExitUpdateAttach();

    private void ExitUpdateAttach()
    {
        if (updateAttachRoutine != null)
        {
            StopCoroutine(updateAttachRoutine);
            updateAttachRoutine = null;
        }
    }

    /// <summary>
    /// EnableAxis로 지정된 방향 벡터 중 입력된 상호작용 대상의 foward 방향과 가장 유사한 방향 벡터를 선택해 Attach를 회전
    /// </summary>
    /// <param name="grabInteractable">상호작용 대상</param>
    /// <returns></returns>
    private IEnumerator UpdateAttachRotation(XRGrabInteractable grabInteractable)
    {
        YieldInstruction updatePeriod = null;

        while (true)
        {
            AttachRotation(grabInteractable.transform);
            yield return updatePeriod;
        }
    }

    protected abstract void AttachRotation(Transform article);

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.TryGetComponent(out InteractableArticle article))
        {
            selectedArticle = article;
            SelectedChanged();
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (selectedArticle != null)
        {
            selectedArticle = null;
            SelectedChanged();
        }
    }

    private void SelectedChanged()
    {
        OnSelectedChanged?.Invoke(new()
        {
            selectedAxisIndex = SelectedAxisIndex,
            articleId = selectedArticle != null ? selectedArticle.Id : -1,
            articleState = selectedArticle != null ? selectedArticle.State : -1,
        });
    }

}
