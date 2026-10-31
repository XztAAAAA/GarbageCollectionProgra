using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 垃圾预处理 UI 管理器。
/// 负责预处理面板的显示、文本更新和按钮交互。
/// 不负责预处理步骤的具体逻辑。
/// </summary>
public class WastePreprocessUI : MonoBehaviour
{
    [Header("面板标题 / 패널 제목")]
    [SerializeField] private TMP_Text titleText;

    [Header("步骤说明 / 단계 설명")]
    [SerializeField] private TMP_Text descText;

    [Header("完成按钮 / 완료 버튼")]
    [SerializeField] private Button completeButton;
    [SerializeField] private TMP_Text completeButtonText;

    [Header("步骤处理时间 / 단계 처리 시간")]
    [SerializeField, Min(0f)] private float completionDelay = 2f;

    [SerializeField] private Image completionProgressImage;

    // 当前面板对应的垃圾实体 ID
    private EntityId currentItemEntityId;

    // 是否存在正在处理的垃圾
    private bool hasCurrentItem;

    private Coroutine completionCoroutine;
    private bool isProcessing;

    private void Awake()
    {
        if (completionProgressImage == null)
        {
            Debug.LogError(
                "[WastePreprocessUI] 没有绑定处理进度 Image。",
                this
            );
            return;
        }

        completionProgressImage.type = Image.Type.Filled;
        completionProgressImage.fillMethod = Image.FillMethod.Horizontal;
        completionProgressImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        completionProgressImage.raycastTarget = false;
        completionProgressImage.fillAmount = 1f;
    }

    private void OnEnable()
    {
        if (completeButton != null)
        {
            completeButton.onClick.AddListener(
                OnCompleteButtonClicked
            );
        }
    }

    private void OnDisable()
    {
        StopPendingCompletion();

        if (completeButton != null)
        {
            completeButton.onClick.RemoveListener(
                OnCompleteButtonClicked
            );
        }
    }

    /// <summary>
    /// 接收预处理面板变化事件。
    /// 只更新 UI 内容，不控制面板开关。
    /// </summary>
    public void OnPreprocessPanelChanged(
        WastePreprocessPanelChangedEvent eventData)
    {
        // 面板关闭时，清除当前操作状态。
        if (!eventData.IsVisible)
        {
            hasCurrentItem = false;
            StopPendingCompletion();

            if (completeButton != null)
            {
                completeButton.interactable = false;
            }

            Debug.Log("[WastePreprocessUI] 收到面板关闭消息。");
            return;
        }

        if (hasCurrentItem &&
            !EqualityComparer<EntityId>.Default.Equals(
                currentItemEntityId,
                eventData.ItemEntityId))
        {
            StopPendingCompletion();
        }

        // 保存当前正在处理的垃圾 ID。
        currentItemEntityId = eventData.ItemEntityId;
        hasCurrentItem = true;

        if (titleText != null)
        {
            titleText.text = "전처리 안내\n预处理说明";
        }

        if (descText != null)
        {
            descText.text = eventData.ContentText;
        }

        if (completeButtonText != null)
        {
            completeButtonText.text = isProcessing
                ? "진행 중...\n正在进行中..."
                : eventData.ButtonText;
        }

        if (completeButton != null)
        {
            completeButton.interactable = !isProcessing;
        }

        if (isProcessing && completionCoroutine == null)
        {
            StartStepProcessing();
        }

        Debug.Log(
            $"[WastePreprocessUI] 更新预处理内容，垃圾 ID：{currentItemEntityId}"
        );
    }

    /// <summary>
    /// 玩家点击完成按钮。
    /// </summary>
    private void OnCompleteButtonClicked()
    {
        if (!hasCurrentItem)
        {
            Debug.LogWarning(
                "[WastePreprocessUI] 当前没有正在处理的垃圾。"
            );
            return;
        }

        if (isProcessing)
            return;

        isProcessing = true;

        if (completeButton != null)
        {
            completeButton.interactable = false;
        }

        if (completeButtonText != null)
        {
            completeButtonText.text = "진행 중...\n正在进行中...";
        }

        Debug.Log(
            $"[WastePreprocessUI] 开始处理预处理流程：{currentItemEntityId}"
        );

        StartStepProcessing();
    }

    private void StartStepProcessing()
    {
        if (!hasCurrentItem || completionCoroutine != null)
            return;

        if (completionProgressImage != null)
            completionProgressImage.fillAmount = 0f;

        completionCoroutine = StartCoroutine(
            CompleteStepAfterDelay(currentItemEntityId)
        );
    }

    private IEnumerator CompleteStepAfterDelay(EntityId itemEntityId)
    {
        float duration = Mathf.Max(0f, completionDelay);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            if (completionProgressImage != null)
            {
                completionProgressImage.fillAmount =
                    Mathf.Clamp01(elapsed / duration);
            }

            yield return null;
        }

        if (completionProgressImage != null)
            completionProgressImage.fillAmount = 1f;

        completionCoroutine = null;

        if (!hasCurrentItem ||
            !EqualityComparer<EntityId>.Default.Equals(
                currentItemEntityId,
                itemEntityId))
        {
            yield break;
        }

        Debug.Log(
            $"[WastePreprocessUI] 请求完成当前预处理步骤：{itemEntityId}"
        );

        GameEventBus.Publish(
            new PreprocessStepCompleteRequestedEvent(
                itemEntityId
            )
        );
    }

    private void StopPendingCompletion()
    {
        if (completionCoroutine != null)
        {
            StopCoroutine(completionCoroutine);
            completionCoroutine = null;
        }

        isProcessing = false;

        if (completionProgressImage != null)
            completionProgressImage.fillAmount = 1f;
    }
}