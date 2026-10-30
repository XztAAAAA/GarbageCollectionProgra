
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class WasteDropZone : MonoBehaviour, IDropHandler
{
    [Header("分类标识 / 분류 키")]
    [SerializeField] private cfg.waste.WasteCategory expectedCategoryKey;

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

        // 判断垃圾自身分类和当前投放目标是否一致。
        bool isCorrect = itemView.CategoryKey == expectedCategoryKey;

        // 通过事件中心分发结果，不直接调用其他组件。
        GameEventBus.Publish(
            new WasteDropResultEvent(
                eventData.pointerDrag.GetEntityId(),
                itemView.ItemId,
                itemView.CategoryKey,
                expectedCategoryKey,
                isCorrect
            )
        );

        Debug.Log(
            $"[WasteDropZone] 分类结果：{isCorrect}，" +
            $"物品：{itemView.CategoryKey}，" +
            $"目标：{expectedCategoryKey}"
        );
    }
}
