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

        WasteItemDragHandler dragHandler =
            eventData.pointerDrag.GetComponent<WasteItemDragHandler>();

        WasteItemView itemView =
            eventData.pointerDrag.GetComponent<WasteItemView>();

        if (dragHandler == null || itemView == null)
            return;

        bool isCorrect = itemView.CategoryKey == expectedCategoryKey;

        dragHandler.SetDropResult(isCorrect);

        if (isCorrect)
        {
            Debug.Log(
                $"分类正确 / 분류 정답: {itemView.CategoryKey}"
            );
        }
        else
        {
            Debug.Log(
                $"分类错误 / 분류 오답：物品={itemView.CategoryKey}，" +
                $"目标={expectedCategoryKey}"
            );
        }
    }
}