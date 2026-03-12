using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
// 僅用於 Vector3, Quaternion 等數學類型
using UnityEngine;
// ========================================================================
// Object Pool（對象池 - Pure C#）
// ========================================================================
namespace Nobeta.EUG.BulletSystem.ECSLike.Pure
{
    /// <summary>
    /// ID 池（重用子彈 ID）
    /// </summary>
    public class IdPool
    {
        private readonly Queue<int> availableIds;
        private int nextId = 0;
        private readonly int maxCapacity;

        public IdPool(int capacity)
        {
            maxCapacity = capacity;
            availableIds = new Queue<int>(capacity);
        }

        public int Get()
        {
            if (availableIds.Count > 0)
            {
                return availableIds.Dequeue();
            }

            if (nextId < maxCapacity)
            {
                return nextId++;
            }

            throw new InvalidOperationException("ID 池已耗盡！");
        }

        public void Return(int id)
        {
            availableIds.Enqueue(id);
        }

        public void Clear()
        {
            availableIds.Clear();
            nextId = 0;
        }
    }
}