
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 垃圾分类投放区域。
/// 只负责接收玩家投放操作并发布请求事件。
/// 不负责预处理检查和分类正确性判断。
/// </summary>
[RequireComponent(typeof(Image))]
public class WasteDropZone : MonoBehaviour, IDropHandler
{
    [Header("分类标识 / 분류 키")]
    [SerializeField]
    private cfg.waste.WasteCategory expectedCategoryKey;

    /// <summary>
    /// 玩家将垃圾拖入当前分类区域时触发。
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        WasteItemView itemView =
            eventData.pointerDrag.GetComponent<WasteItemView>();

        WasteItemDragHandler dragHandler =
            eventData.pointerDrag.GetComponent<WasteItemDragHandler>();

        if (itemView == null || dragHandler == null)
            return;

        // 发布投放尝试事件。
        // 预处理检查和分类判断交由其他 Manager 处理。
        GameEventBus.Publish(
            new WasteDropAttemptEvent(
                eventData.pointerDrag.GetEntityId(),
                itemView.ItemId,
                itemView.CategoryKey,
                expectedCategoryKey
            )
        );

        Debug.Log(
            $"[WasteDropZone] 投放请求：" +
            $"ItemId = {itemView.ItemId}, " +
            $"物品分类 = {itemView.CategoryKey}, " +
            $"目标分类 = {expectedCategoryKey}"
        );
    }
}
