
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 垃圾生成器。
/// 根据关卡配置生成指定的垃圾物品。
/// </summary>
public class WasteItemSpawner : MonoBehaviour
{
    [Header("生成设置 / 생성 설정")]
    [SerializeField] private RectTransform itemContainer;
    [SerializeField] private WasteItemView itemPrefab;

    [Header("物体尺寸 / 오브젝트 크기")]
    [SerializeField]
    private Vector2 itemSize = new Vector2(150f, 150f);

    [SerializeField] private float minDistance = 170f;

    private readonly List<Vector2> spawnedPositions =
        new List<Vector2>();

    // 当前由本 Spawner 生成的垃圾实例
    private readonly List<GameObject> spawnedItems =
        new List<GameObject>();

    private void OnEnable()
    {
        GameEventBus.Subscribe<WasteLevelStartRequestedEvent>(
            OnLevelStartRequested
        );
    }

    private void OnDisable()
    {
        GameEventBus.Unsubscribe<WasteLevelStartRequestedEvent>(
            OnLevelStartRequested
        );
    }

    /// <summary>
    /// 收到关卡开始请求。
    /// </summary>
    private void OnLevelStartRequested(
        WasteLevelStartRequestedEvent eventData)
    {
        SpawnLevelItems(eventData.LevelId);
    }

    /// <summary>
    /// 根据关卡 ID 生成对应垃圾。
    /// </summary>
    private void SpawnLevelItems(int levelId)
    {
        if (itemContainer == null || itemPrefab == null)
        {
            CommonTool.LogError(
                "[WasteItemSpawner] 请绑定 Container 和 Prefab。",
                this
            );
            return;
        }

        // 读取关卡数据
        var levelData = TableSystem.Tables.TbLevel.Get(levelId);

        if (levelData == null)
        {
            CommonTool.LogError(
                $"[WasteItemSpawner] 找不到关卡配置：{levelId}"
            );
            return;
        }

        // 清理上一关生成的垃圾
        ClearSpawnedItems();

        Canvas.ForceUpdateCanvases();

        spawnedPositions.Clear();

        CommonTool.Log(
            $"[WasteItemSpawner] 开始生成第 {levelId} 关：" +
            $"{levelData.LevelName}"
        );

        // 根据关卡配置的垃圾 ID 列表生成
        foreach (int itemId in levelData.WasteItemIds)
        {
            SpawnSingleItem(itemId);
        }

        // 通知游戏管理器：本关垃圾已经全部生成
        GameEventBus.Publish(
            new WasteItemsSpawnedEvent(spawnedItems.Count)
        );

        CommonTool.Log(
            $"[WasteItemSpawner] 第 {levelId} 关生成完成，" +
            $"共生成 {spawnedItems.Count} 个垃圾。"
        );
    }

    /// <summary>
    /// 生成单个垃圾物品。
    /// </summary>
    private void SpawnSingleItem(int itemId)
    {
        var data = TableSystem.Tables.TbWasteItem.Get(itemId);

        if (data == null)
        {
            CommonTool.LogError(
                $"[WasteItemSpawner] 找不到垃圾配置：{itemId}"
            );
            return;
        }

        WasteItemView view = Instantiate(
            itemPrefab,
            itemContainer,
            false
        );

        RectTransform rect = view.GetComponent<RectTransform>();

        // 以容器中心为基准自由分布
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = itemSize;
        rect.localScale = Vector3.one;

        // 获取随机位置
        Vector2 position = GetRandomPosition();

        rect.anchoredPosition = position;
        spawnedPositions.Add(position);

        // 加载垃圾图片
        // Resources.Load 路径必须相对于 Resources 文件夹，且不能包含扩展名。
        // 对应资源应位于 Assets/Resources/_Project/UI/Item/ 下。
        Sprite sprite = Resources.Load<Sprite>(
            $"UI/Item/{data.IconKey}"
        );

        if (sprite == null)
        {
            CommonTool.LogError(
                $"[WasteItemSpawner] 无法加载图片：UI/Item/{data.IconKey}。" +
                "请确认资源位于 Assets/Resources/UI/Item/ 下。",
                this
            );
        }

        // 初始化垃圾物品
        view.Initialize(
            data.Id,
            data.IconKey,
            data.Category,
            sprite
        );

        spawnedItems.Add(view.gameObject);

        // 通知预处理管理器初始化垃圾状态
        GameEventBus.Publish(
            new WasteItemSpawnedEvent(
                view.gameObject.GetEntityId(),
                data.Id
            )
        );
    }

    /// <summary>
    /// 清理上一关生成的垃圾。
    /// </summary>
    private void ClearSpawnedItems()
    {
        foreach (GameObject item in spawnedItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }

        spawnedItems.Clear();
        spawnedPositions.Clear();
    }

    /// <summary>
    /// 获取随机位置，并尽量避免物体重叠。
    /// </summary>
    private Vector2 GetRandomPosition()
    {
        float halfWidth = Mathf.Max(
            0f,
            itemContainer.rect.width * 0.5f -
            itemSize.x * 0.5f
        );

        float halfHeight = Mathf.Max(
            0f,
            itemContainer.rect.height * 0.5f -
            itemSize.y * 0.5f
        );

        Vector2 candidate = Vector2.zero;

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

        // 空间不足时仍然生成
        return candidate;
    }
}
