using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class WasteItemView : MonoBehaviour
{
    [SerializeField] private Image itemImage;

    public int ItemId { get; private set; }
    public string IconKey { get; private set; }

    private void Awake()
    {
        if (itemImage == null)
        {
            itemImage = GetComponent<Image>();
        }
    }

    public void Initialize(
        int itemId,
        string iconKey,
        Sprite sprite)
    {
        ItemId = itemId;
        IconKey = iconKey;

        if (itemImage == null)
        {
            itemImage = GetComponent<Image>();
        }

        itemImage.sprite = sprite;
        itemImage.preserveAspect = true;

        // 没有图片时显示浅绿色占位方块。
        itemImage.color = sprite == null
            ? new Color(0.75f, 0.85f, 0.75f, 0.8f)
            : Color.white;

        itemImage.raycastTarget = true;
        gameObject.SetActive(true);
        gameObject.name = ItemId.ToString();
    }
}