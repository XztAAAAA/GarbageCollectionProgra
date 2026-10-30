
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局事件总线。
/// 负责事件的订阅、取消订阅和发布。
/// </summary>
public static class GameEventBus
{
    // Key：事件类型
    // Value：该事件类型对应的所有监听方法
    private static readonly Dictionary<Type, Delegate> EventTable =
        new Dictionary<Type, Delegate>();

    /// <summary>
    /// 订阅事件
    /// </summary>
    public static void Subscribe<T>(Action<T> callback)
        where T : struct
    {
        if (callback == null)
        {
            Debug.LogError("[GameEventBus] 订阅失败：回调为空。");
            return;
        }

        Type eventType = typeof(T);

        if (EventTable.TryGetValue(eventType, out Delegate existing))
        {
            EventTable[eventType] =
                Delegate.Combine(existing, callback);
        }
        else
        {
            EventTable.Add(eventType, callback);
        }
    }

    /// <summary>
    /// 取消订阅
    /// </summary>
    public static void Unsubscribe<T>(Action<T> callback)
        where T : struct
    {
        if (callback == null)
            return;

        Type eventType = typeof(T);

        if (!EventTable.TryGetValue(eventType, out Delegate existing))
            return;

        Delegate remaining = Delegate.Remove(existing, callback);

        if (remaining == null)
        {
            EventTable.Remove(eventType);
        }
        else
        {
            EventTable[eventType] = remaining;
        }
    }

    /// <summary>
    /// 发布事件
    /// </summary>
    public static void Publish<T>(T eventData)
        where T : struct
    {
        Type eventType = typeof(T);

        if (!EventTable.TryGetValue(eventType, out Delegate callbacks))
            return;

        // 获取当前监听者快照，避免回调执行时修改订阅列表造成问题。
        Delegate[] invocationList = callbacks.GetInvocationList();

        foreach (Delegate callback in invocationList)
        {
            try
            {
                ((Action<T>)callback).Invoke(eventData);
            }
            catch (Exception exception)
            {
                // 一个监听者出现异常，不影响其他监听者接收事件。
                Debug.LogException(exception);
            }
        }
    }

    /// <summary>
    /// Unity 运行时初始化时清空静态事件数据。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetEventBus()
    {
        EventTable.Clear();
    }
}
