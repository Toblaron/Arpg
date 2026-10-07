// EquipmentComponent.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Inventory;
using Godot;

namespace Game.Equipment
{
    /// <summary>
    /// Handles equipping/unequipping items and aggregates their stats.
    /// Fires an event whenever the equipped set changes.
    /// </summary>
    public partial class EquipmentComponent : Node
    {
        /// <summary>Map slot name (e.g. "Head", "Chest") to the equipped item.</summary>
        private readonly Dictionary<string, InventorySlot> _slots = new();

        /// <summary>Raised whenever the equipment set changes.</summary>
        public event Action OnEquipmentChanged;

        /// <summary>Current aggregated stats of the equipped items.</summary>
        public Stats TotalStats => AggregateStats();

        /// <summary>
        /// Initialize the equipment slots. Call once after construction.
        /// </summary>
        /// <param name="slotNames">Enumerable of slot identifiers.</param>
        /// <param name="capacity">Maximum items per slot (default 1).</param>
        public void Init(IEnumerable<string> slotNames, int capacity = 1)
        {
            foreach (var name in slotNames)
            {
                if (!_slots.ContainsKey(name))
                {
                    _slots[name] = new InventorySlot(capacity);
                }
            }
        }

        /// <summary>
        /// Equip an item into the specified slot. Replaces any existing item.
        /// </summary>
        public void Equip(string slotName, ItemResource item)
        {
            if (!_slots.ContainsKey(slotName))
                throw new ArgumentException($"Slot '{slotName}' not defined.");

            var slot = _slots[slotName];
            slot.SetItem(item);
            OnEquipmentChanged?.Invoke();
        }

        /// <summary>
        /// Unequip the item from the specified slot.
        /// </summary>
        public void Unequip(string slotName)
        {
            if (!_slots.ContainsKey(slotName))
                return;

            var slot = _slots[slotName];
            if (!slot.IsEmpty)
            {
                slot.Clear();
                OnEquipmentChanged?.Invoke();
            }
        }

        /// <summary>
        /// Get the currently equipped item for a slot, or null if empty.
        /// </summary>
        public ItemResource GetEquipped(string slotName)
        {
            return _slots.TryGetValue(slotName, out var slot) && !slot.IsEmpty
                ? slot.Item
                : null;
        }

        /// <summary>
        /// Aggregate stats from all equipped items.
        /// </summary>
        private Stats AggregateStats()
        {
            var total = new Stats();
            foreach (var slot in _slots.Values)
            {
                if (!slot.IsEmpty)
                {
                    total += slot.Item.GetTotalStats();
                }
            }
            return total;
        }

        /// <summary>
        /// Serialize the equipped items into a simple dictionary for JSON.
        /// </summary>
        public Dictionary<string, string> Serialize()
        {
            var dict = new Dictionary<string, string>();
            foreach (var kvp in _slots)
            {
                dict[kvp.Key] = kvp.Value.IsEmpty ? null : kvp.Value.Item.ResourcePath;
            }
            return dict;
        }

        /// <summary>
        /// Deserialize from a dictionary, re-equipping items.
        /// </summary>
        public void Deserialize(Dictionary<string, string> data)
        {
            foreach (var kvp in data)
            {
                if (string.IsNullOrEmpty(kvp.Value))
                {
                    Unequip(kvp.Key);
                }
                else
                {
                    var res = ResourceLoader.Load<ItemResource>(kvp.Value);
                    if (res != null)
                        Equip(kvp.Key, res);
                }
            }
        }
    }
}
