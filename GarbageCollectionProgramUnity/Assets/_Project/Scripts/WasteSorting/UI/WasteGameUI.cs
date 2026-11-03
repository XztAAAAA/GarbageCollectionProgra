
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 垃圾分类游戏 UI 控制器。
/// 只负责监听 UI 事件并更新界面。
/// </summary>
public class WasteGameUI : MonoBehaviour
{
    [Header("顶部进度 / 상단 진행도")]
    [SerializeField] private TMP_Text stageProgressText;
    [SerializeField] private TMP_Text remainingItemsText;
    [SerializeField]private Slider slider;
    [Header("底部提示 / 하단 안내")]
    [SerializeField] private TMP_Text feedbackMessageText;

    private RectTransform topProgressPanel;
    private RectTransform feedbackPanel;
    private Vector2 topProgressTargetPosition;
    private Vector2 feedbackTargetPosition;
    private Tween topProgressTween;
    private Tween feedbackTween;
    private WasteProgressChangedEvent lastProgressEvent;
    private WasteFeedbackEvent lastFeedbackEvent;
    private bool hasProgressEvent;
    private bool hasFeedbackEvent;

    private void Awake()
    {
        topProgressPanel = transform.Find("TopProgressPanel") as RectTransform;
        feedbackPanel = transform.Find("FeedbackPanel") as RectTransform;

        if (topProgressPanel == null)
        {
            CommonTool.LogError(
                "[WasteGameUI] 找不到 TopProgressPanel，无法播放进入动画。",
                this
            );
        }
        else
        {
            topProgressTargetPosition = topProgressPanel.anchoredPosition;
        }

        if (feedbackPanel == null)
        {
            CommonTool.LogError(
                "[WasteGameUI] 找不到 FeedbackPanel，无法播放进入动画。",
                this
            );
        }
        else
        {
            feedbackTargetPosition = feedbackPanel.anchoredPosition;
        }
    }

    private void OnEnable()
    {
        PlayPanelEntranceAnimations();

        GameEventBus.Subscribe<WasteProgressChangedEvent>(
            OnProgressChanged
        );

        GameEventBus.Subscribe<WasteFeedbackEvent>(
            OnFeedbackChanged
        );
        TableSystem.LanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        StopPanelEntranceAnimations();

        GameEventBus.Unsubscribe<WasteProgressChangedEvent>(
            OnProgressChanged
        );

        GameEventBus.Unsubscribe<WasteFeedbackEvent>(
            OnFeedbackChanged
        );
        TableSystem.LanguageChanged -= OnLanguageChanged;
    }

    private void PlayPanelEntranceAnimations()
    {
        if (topProgressPanel != null)
        {
            topProgressTween?.Kill();
            topProgressPanel.anchoredPosition =
                topProgressTargetPosition +
                Vector2.up * (topProgressPanel.rect.height + 30f);
            topProgressTween = topProgressPanel
                .DOAnchorPos(topProgressTargetPosition, 0.4f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }

        if (feedbackPanel != null)
        {
            feedbackTween?.Kill();
            feedbackPanel.anchoredPosition =
                feedbackTargetPosition -
                Vector2.up * (feedbackPanel.rect.height + 30f);
            feedbackTween = feedbackPanel
                .DOAnchorPos(feedbackTargetPosition, 0.4f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }
    }

    private void StopPanelEntranceAnimations()
    {
        topProgressTween?.Kill();
        topProgressTween = null;
        feedbackTween?.Kill();
        feedbackTween = null;

        if (topProgressPanel != null)
        {
            topProgressPanel.anchoredPosition = topProgressTargetPosition;
        }

        if (feedbackPanel != null)
        {
            feedbackPanel.anchoredPosition = feedbackTargetPosition;
        }
    }

    /// <summary>
    /// 更新顶部关卡进度。
    /// </summary>
    private void OnProgressChanged(
        WasteProgressChangedEvent eventData)
    {
        lastProgressEvent = eventData;
        hasProgressEvent = true;
        RefreshProgressText(eventData);

        if(slider != null)
        {
            slider.value = eventData.Progress;
        }
    }

    private void RefreshProgressText(WasteProgressChangedEvent eventData)
    {
        if (stageProgressText != null)
        {
            stageProgressText.text = TableSystem.GetLanguage("game_level", eventData.CurrentStage);
        }

        if (remainingItemsText != null)
        {
            remainingItemsText.text = TableSystem.GetLanguage("game_remaining", eventData.RemainingItems);
        }
    }

    /// <summary>
    /// 更新底部中韩双语提示。
    /// </summary>
    private void OnFeedbackChanged(
        WasteFeedbackEvent eventData)
    {
        lastFeedbackEvent = eventData;
        hasFeedbackEvent = true;
        RefreshFeedbackText(eventData);
    }

    private void RefreshFeedbackText(WasteFeedbackEvent eventData)
    {
        if (feedbackMessageText == null)
            return;

        feedbackMessageText.text = string.IsNullOrEmpty(eventData.LanguageKey)
            ? eventData.Message
            : TableSystem.GetLanguage(eventData.LanguageKey);
    }

    private void OnLanguageChanged()
    {
        if (hasProgressEvent)
        {
            RefreshProgressText(lastProgressEvent);
        }

        if (hasFeedbackEvent)
        {
            RefreshFeedbackText(lastFeedbackEvent);
        }
    }
}
