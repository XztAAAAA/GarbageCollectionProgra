using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("UI/Common Button")]
public class CommonButton : Button
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (
            eventData.button == PointerEventData.InputButton.Left &&
            IsActive() &&
            IsInteractable()
        )
        {
            CommonTool.PlayClickSound();
        }

        base.OnPointerClick(eventData);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        if (IsActive() && IsInteractable())
        {
            CommonTool.PlayClickSound();
        }

        base.OnSubmit(eventData);
    }
}
