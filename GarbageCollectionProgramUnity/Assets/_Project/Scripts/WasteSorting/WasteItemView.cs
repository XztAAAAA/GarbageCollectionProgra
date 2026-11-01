using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class WasteItemView : MonoBehaviour
{
    [SerializeField] private Image itemImage;

    public int ItemId { get; private set; }
    public string IconKey { get; private set; }

    // 使用 Luban 生成的枚举名称作为分类标识
    public cfg.waste.WasteCategory CategoryKey { get; private set; }

    private void Awake()
    {
        if (itemImage == null)
            itemImage = GetComponent<Image>();
    }

    public void Initialize(
        int itemId,
        string iconKey,
        cfg.waste.WasteCategory categoryKey,
        Sprite sprite)
    {
        ItemId = itemId;
        IconKey = iconKey;
        CategoryKey = categoryKey;

        if (itemImage == null)
            itemImage = GetComponent<Image>();

        itemImage.sprite = sprite;
        if (sprite != null)
            itemImage.SetNativeSize();
        itemImage.preserveAspect = true;
        itemImage.raycastTarget = true;
        itemImage.color = sprite == null
            ? new Color(0.75f, 0.85f, 0.75f, 0.8f)
            : Color.white;

        gameObject.SetActive(true);
        gameObject.transform.localScale = Vector3.one * 0.13f;
        gameObject.name = itemId.ToString() + "_" + CategoryKey.ToString();
    }
}