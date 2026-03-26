using Nobeta.EUG.FSM.Core;
using System;
using System.Net;
using Unity.Mathematics;

using UnityEngine;
using VContainer.Unity;
// ========================================================================
// Supporting Classes（輔助類）
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    /// <summary>
    /// 可互動物體接口
    /// </summary>
    public interface IInteractable
    {
        string InteractableName { get; }
        float InteractionDuration { get; }
        void OnInteract();
    }

    /// <summary>
    /// 道具數據
    /// </summary>
    [System.Serializable]
    public class ItemData
    {
        public string ItemName;
        public ItemType ItemType;
        public int EffectValue;
        public float UseDuration = 0.8f;
        public float BuffDuration = 0f;
    }

    /// <summary>
    /// 道具類型
    /// </summary>
    public enum ItemType
    {
        HealthPotion,   // 生命藥水
        ManaPotion,     // 魔力藥水
        Buff,           // 增益道具
        Tool,           // 工具（如鑰匙、繩索）
        Consumable      // 其他消耗品
    }
    // ========================================================================
    // Game Events（遊戲事件）
    // ========================================================================

    public struct InteractionCompletedEvent
    {
        public string InteractableType;
        public Vector3 Position;
    }

    public struct ItemUsedEvent
    {
        public string ItemName;
        public ItemType ItemType;
        public int EffectValue;
    }

    public struct PlayerHealedEvent
    {
        public int Amount;
        public int CurrentHealth;
    }

    public struct ManaRestoredEvent
    {
        public int Amount;
        public int CurrentMana;
    }

    public struct BuffAppliedEvent
    {
        public string BuffName;
        public float Duration;
        public int EffectValue;
    }

    public struct ToolUsedEvent
    {
        public string ToolName;
        public Vector3 Position;
    }
}
