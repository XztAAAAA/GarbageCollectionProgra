using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WasteItemSpawner : MonoBehaviour
{
    [Header("生成设置")]
    [SerializeField] private RectTransform itemContainer;
    [SerializeField] private WasteItemView itemPrefab;

    [Header("测试数据 ID")]
    [SerializeField]
    private int[] itemIds =
    {
        1001, 1002, 1003, 1004, 1005, 1006
    };

    [Header("物体尺寸")]
    [SerializeField] private Vector2 itemSize =
        new Vector2(150f, 150f);

    [SerializeField] private float minDistance = 170f;

    private readonly List<Vector2> spawnedPositions =
        new List<Vector2>();

    private void Start()
    {
        SpawnItems();
    }

    private void SpawnItems()
    {
        if (itemContainer == null || itemPrefab == null)
        {
            Debug.LogError(
                "[WasteItemSpawner] 请绑定 Container 和 Prefab。",
                this
            );
            return;
        }

        Canvas.ForceUpdateCanvases();

        spawnedPositions.Clear();

        var tables = TableSystem.Tables;

        foreach (int id in itemIds)
        {
            var data = tables.TbWasteItem.Get(id);

            if (data == null)
            {
                Debug.LogWarning($"未找到垃圾数据，ID：{id}");
                continue;
            }

            WasteItemView view = Instantiate(
                itemPrefab,
                itemContainer,
                false
            );

            RectTransform rect = view.GetComponent<RectTransform>();

            // 以容器中心为基准自由分布。
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = itemSize;
            rect.localScale = Vector3.one;

            Vector2 position = GetRandomPosition();
            rect.anchoredPosition = position;
            spawnedPositions.Add(position);

            // 图片先尝试加载；没有资源时使用占位图形。
            Sprite sprite = Resources.Load<Sprite>(
                $"WasteItems/{data.IconKey}"
            );

            view.Initialize(data.Id, data.IconKey, sprite);
        }

        Debug.Log(
            $"垃圾物品生成完成，共生成 {spawnedPositions.Count} 个。"
        );
    }

    private Vector2 GetRandomPosition()
    {
        float halfWidth = Mathf.Max(
            0f,
            itemContainer.rect.width * 0.5f - itemSize.x * 0.5f
        );

        float halfHeight = Mathf.Max(
            0f,
            itemContainer.rect.height * 0.5f - itemSize.y * 0.5f
        );

        Vector2 candidate = Vector2.zero;

        // 尝试避免新物体和已有物体靠得太近。
        for (int attempt = 0; attempt < 50; attempt++)
        {
            candidate = new Vector2(
                Random.Range(-halfWidth, halfWidth),
                Random.Range(-halfHeight, halfHeight)
            );

            bool overlaps = false;

            foreach (Vector2 existing in spawnedPositions)
            {
                if (Vector2.Distance(candidate, existing) < minDistance)
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                return candidate;
            }
        }

        // 空间不足时仍然生成，不阻止关卡运行。
        return candidate;
    }
}