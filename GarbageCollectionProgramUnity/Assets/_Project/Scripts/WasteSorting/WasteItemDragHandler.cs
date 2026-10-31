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
    [Header("垃圾自由移动区域 / 이동 가능 영역")]
    [SerializeField] private RectTransform wasteItemContainer;

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

    private void OnEnable()
    {
        GameEventBus.Subscribe<WasteDropResultEvent>(OnWasteDropResult);
    }

    private void OnDisable()
    {
        GameEventBus.Unsubscribe<WasteDropResultEvent>(OnWasteDropResult);
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
        if (dragParent == null)
        {
            Debug.LogError(
                "[WasteItemDragHandler] 垃圾物体没有 RectTransform 父物体。",
                this
            );
            return;
        }

        // 如果 Inspector 没有绑定 Container，则默认使用垃圾当前的直接父物体。
        // 当前生成结构为 WasteItemContainer 的直接子物体时，无需额外绑定。
        if (wasteItemContainer == null)
            wasteItemContainer = dragParent;

        originalPosition = rectTransform.anchoredPosition;
        originalSiblingIndex = rectTransform.GetSiblingIndex();
        dropAccepted = false;

        // 拖动中的垃圾显示在同级最上层。
        rectTransform.SetAsLastSibling();

        // 拖动时忽略自身射线，确保分类区域能够接收 OnDrop。
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragParent == null || rootCanvas == null)
            return;

        Camera eventCamera = GetEventCamera(eventData);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragParent,
            eventData.position,
            eventCamera,
            out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 分类区域已确认分类正确：销毁垃圾物体。
        if (dropAccepted)
        {
            Destroy(gameObject);
            return;
        }

        // 松开位置仍在 Container 内：保留当前位置，允许自由摆放。
        Camera eventCamera = GetEventCamera(eventData);
        bool releasedInsideContainer =
            wasteItemContainer != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                wasteItemContainer,
                eventData.position,
                eventCamera
            );

        if (releasedInsideContainer)
        {
            Debug.Log(
                "[WasteItemDragHandler] 垃圾仍在 Container 内，保留当前位置。",
                this
            );
            return;
        }

        // 松开位置在 Container 外，但没有被正确分类：回到拖动前的位置。
        RestoreOriginalPosition();
    }

    private void RestoreOriginalPosition()
    {
        if (rectTransform == null)
            return;

        rectTransform.anchoredPosition = originalPosition;
        rectTransform.SetSiblingIndex(originalSiblingIndex);

        Debug.Log(
            "[WasteItemDragHandler] 垃圾未正确放入分类区域，返回拖动前的位置。",
            this
        );
    }

    private Camera GetEventCamera(PointerEventData eventData)
    {
        if (rootCanvas == null ||
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return eventData.pressEventCamera;
    }

    /// <summary>
    /// 只处理当前垃圾自己的最终投放结果。
    /// </summary>
    private void OnWasteDropResult(WasteDropResultEvent eventData)
    {
        if (!EqualityComparer<EntityId>.Default.Equals(
            eventData.ItemEntityId,
            gameObject.GetEntityId()))
        {
            return;
        }

        // 只有分类正确且预处理已完成，才允许销毁物体。
        dropAccepted = eventData.IsCorrect && !eventData.RequiresPreprocessing;
    }

    /// <summary>
    /// 玩家点击垃圾物品时发布选中事件。
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
