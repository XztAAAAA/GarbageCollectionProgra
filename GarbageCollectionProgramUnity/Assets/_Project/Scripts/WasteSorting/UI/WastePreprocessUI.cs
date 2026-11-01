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
    [SerializeField] private Image oldImage;
    [SerializeField] private Image newImage;
    [SerializeField] private Image Arrow;

    [Header("完成按钮 / 완료 버튼")]
    [SerializeField] private Button completeButton;
    [SerializeField] private TMP_Text completeButtonText;

    [Header("步骤处理时间 / 단계 처리 시간")]
    [SerializeField, Min(0f)] private float completionDelay = 2f;

    // 当前面板对应的垃圾实体 ID
    private EntityId currentItemEntityId;

    // 当前面板对应的垃圾配置 ID
    private int currentItemId;

    // 是否存在正在处理的垃圾
    private bool hasCurrentItem;

    private Coroutine completionCoroutine;
    private bool isProcessing;
    private Image arrowProgressImage;
    private Color arrowNormalColor;

    private void Awake()
    {
        if (Arrow != null)
        {
            arrowNormalColor = Arrow.color;
            Arrow.color = new Color(
                arrowNormalColor.r * 0.5f,
                arrowNormalColor.g * 0.5f,
                arrowNormalColor.b * 0.5f,
                arrowNormalColor.a
            );
            CreateArrowProgressImage();
        }
    }

    private void OnEnable()
    {
        SetArrowProgress(0f);

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
        WastePreprocessPanelChangedEvent eventData,
        int? itemId)
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

            CommonTool.Log("[WastePreprocessUI] 收到面板关闭消息。");
            return;
        }

        if (!itemId.HasValue)
        {
            CommonTool.LogError(
                "[WastePreprocessUI] 显示预处理面板时未传入垃圾配置 ID。"
            );
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
        currentItemId = itemId.Value;
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

        var data = TableSystem.Tables.TbWasteItem.Get(currentItemId);
        oldImage.sprite = CommonTool.LoadItemSprite(data.IconKey);
        oldImage.SetNativeSize();
        oldImage.transform.localScale = Vector3.one*0.16f;
        newImage.sprite = CommonTool.LoadItemSprite(data.NewIconKey);
        newImage.SetNativeSize();
        newImage.transform.localScale = Vector3.one*0.16f;

        if (isProcessing && completionCoroutine == null)
        {
            StartStepProcessing();
        }

        CommonTool.Log(
            $"[WastePreprocessUI] 更新预处理内容，垃圾 ID：{currentItemId}，" +
            $"实体 ID：{currentItemEntityId}"
        );
    }

    /// <summary>
    /// 玩家点击完成按钮。
    /// </summary>
    private void OnCompleteButtonClicked()
    {
        if (!hasCurrentItem)
        {
            CommonTool.LogWarning(
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

        CommonTool.Log(
            $"[WastePreprocessUI] 开始处理预处理流程：{currentItemEntityId}"
        );

        StartStepProcessing();
    }

    private void StartStepProcessing()
    {
        if (!hasCurrentItem || completionCoroutine != null)
            return;

        SetArrowProgress(0f);

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
            SetArrowProgress(duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f);

            yield return null;
        }

        SetArrowProgress(1f);

        completionCoroutine = null;

        if (!hasCurrentItem ||
            !EqualityComparer<EntityId>.Default.Equals(
                currentItemEntityId,
                itemEntityId))
        {
            yield break;
        }

        CommonTool.Log(
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
        SetArrowProgress(0f);
    }

    private void SetArrowProgress(float progress)
    {
        if (arrowProgressImage != null)
        {
            arrowProgressImage.fillAmount = Mathf.Clamp01(progress);
        }
    }

    private void CreateArrowProgressImage()
    {
        GameObject arrowProgressObject = new GameObject(
            "ArrowProgress",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        RectTransform progressRect =
            arrowProgressObject.GetComponent<RectTransform>();
        progressRect.SetParent(Arrow.transform, false);
        progressRect.anchorMin = Vector2.zero;
        progressRect.anchorMax = Vector2.one;
        progressRect.offsetMin = Vector2.zero;
        progressRect.offsetMax = Vector2.zero;
        progressRect.localScale = Vector3.one;

        arrowProgressImage = arrowProgressObject.GetComponent<Image>();
        arrowProgressImage.sprite = Arrow.sprite;
        arrowProgressImage.material = Arrow.material;
        arrowProgressImage.color = arrowNormalColor;
        arrowProgressImage.preserveAspect = Arrow.preserveAspect;
        arrowProgressImage.raycastTarget = false;
        arrowProgressImage.type = Image.Type.Filled;
        // The arrow is rotated 90 degrees, so vertical fill moves left to right on screen.
        arrowProgressImage.fillMethod = Image.FillMethod.Vertical;
        arrowProgressImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        arrowProgressImage.fillAmount = 0f;
    }
}