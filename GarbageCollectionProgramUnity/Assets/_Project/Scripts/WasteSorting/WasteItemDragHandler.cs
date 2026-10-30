using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class WasteItemDragHandler : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
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
    /// 监听垃圾分类结果事件。
    /// </summary>
    private void OnWasteDropResult(WasteDropResultEvent eventData)
    {
        // 只处理当前垃圾自身对应的事件。
        if (eventData.ItemEntityId != gameObject.GetEntityId())
            return;

        dropAccepted = eventData.IsCorrect;
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
}