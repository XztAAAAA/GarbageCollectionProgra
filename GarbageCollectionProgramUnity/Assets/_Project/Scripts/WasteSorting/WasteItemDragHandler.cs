using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class WasteItemDragHandler : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerClickHandler
{
    private RectTransform rectTransform;
    private RectTransform dragParent;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Vector2 originalPosition;
    private int originalSiblingIndex;
    private bool dropAccepted;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null)
            rootCanvas = rootCanvas.rootCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rootCanvas == null)
        {
            Debug.LogError(
                "[WasteItemDragHandler] 没有找到 Canvas。",
                this
            );
            return;
        }

        dragParent = rectTransform.parent as RectTransform;
        originalPosition = rectTransform.anchoredPosition;
        originalSiblingIndex = rectTransform.GetSiblingIndex();

        dropAccepted = false;

        // 让拖动中的垃圾显示在同级物体最上层。
        rectTransform.SetAsLastSibling();

        // 让上方分类区域能够接收到拖放事件。
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragParent == null || rootCanvas == null)
            return;

        Camera eventCamera =
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : eventData.pressEventCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragParent,
            eventData.position,
            eventCamera,
            out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    private void OnEnable()
    {
        GameEventBus.Subscribe<WasteDropResultEvent>(
            OnWasteDropResult
        );
    }

    private void OnDisable()
    {
        GameEventBus.Unsubscribe<WasteDropResultEvent>(
            OnWasteDropResult
        );
    }

    /// <summary>
    /// 监听垃圾最终投放结果。
    /// </summary>
    private void OnWasteDropResult(WasteDropResultEvent eventData)
    {
        // 只处理当前垃圾自己的投放结果。
        if (!EqualityComparer<EntityId>.Default.Equals(
            eventData.ItemEntityId,
            gameObject.GetEntityId()))
        {
            return;
        }

        // 只有真正分类正确，才能移除当前垃圾。
        // 预处理未完成或分类错误，都需要复位。
        dropAccepted =
            eventData.IsCorrect &&
            !eventData.RequiresPreprocessing;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (dropAccepted)
        {
            // 暂时先移除分类正确的物品。
            // 后续接入关卡进度时再统一管理销毁。
            Destroy(gameObject);
            return;
        }

        // 分类错误或没有放到目标区域：回到原位置。
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.SetSiblingIndex(originalSiblingIndex);
    }

    /// <summary>
    /// 玩家点击垃圾物品时触发。
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        WasteItemView itemView = GetComponent<WasteItemView>();

        if (itemView == null)
        {
            Debug.LogError(
                "[WasteItemDragHandler] 找不到 WasteItemView。",
                this
            );
            return;
        }

        // 发布垃圾选中事件。
        GameEventBus.Publish(
            new WasteItemSelectedEvent(
                gameObject.GetEntityId(),
                itemView.ItemId
            )
        );

        Debug.Log(
            $"选中垃圾 / 쓰레기 선택: ID = {itemView.ItemId} + _ + {itemView.CategoryKey}"
        );
    }
}