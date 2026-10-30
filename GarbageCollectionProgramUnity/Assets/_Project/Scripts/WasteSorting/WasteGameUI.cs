
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

    private void OnEnable()
    {
        GameEventBus.Subscribe<WasteProgressChangedEvent>(
            OnProgressChanged
        );

        GameEventBus.Subscribe<WasteFeedbackEvent>(
            OnFeedbackChanged
        );
    }

    private void OnDisable()
    {
        GameEventBus.Unsubscribe<WasteProgressChangedEvent>(
            OnProgressChanged
        );

        GameEventBus.Unsubscribe<WasteFeedbackEvent>(
            OnFeedbackChanged
        );
    }

    /// <summary>
    /// 更新顶部关卡进度。
    /// </summary>
    private void OnProgressChanged(
        WasteProgressChangedEvent eventData)
    {
        if (stageProgressText != null)
        {
            stageProgressText.text =
                $"{eventData.CurrentStage}단계" +
                $"第 {eventData.CurrentStage} 关";
        }

        if (remainingItemsText != null)
        {
            remainingItemsText.text =
                $"남은 쓰레기: {eventData.RemainingItems}개\n" +
                $"剩余垃圾：{eventData.RemainingItems} 件";
        }

        if(slider != null)
        {
            slider.value = eventData.Progress;
        }
    }

    /// <summary>
    /// 更新底部中韩双语提示。
    /// </summary>
    private void OnFeedbackChanged(
        WasteFeedbackEvent eventData)
    {
        if (feedbackMessageText == null)
            return;

        feedbackMessageText.text =
            $"{eventData.KoreanMessage}\n" +
            $"{eventData.ChineseMessage}";
    }
}
