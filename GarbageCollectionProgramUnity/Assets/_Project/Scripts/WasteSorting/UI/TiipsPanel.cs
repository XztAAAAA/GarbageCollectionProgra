using System;
using UnityEngine;
using UnityEngine.UI;

public class TiipsPanel : MonoBehaviour
{
    [SerializeField] private Button closeButton;

    [SerializeField] private WasteItemView wasteItemView;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Init(WasteItemView itemView)
    {
        wasteItemView.Initialize(itemView.ItemId, itemView.IconKey, itemView.CategoryKey, itemView.Sprite);
        wasteItemView.transform.position = itemView.transform.position;
    }

    private void OnEnable()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    private void OnDisable()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
        }
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
