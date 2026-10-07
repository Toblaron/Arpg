// InventorySerializer.cs
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Game.Persistence
{
    /// <summary>
    /// Data structures used for JSON round‑trip.
    /// </summary>
    private record SlotData
    {
        public int Index { get; init; }
        public string ItemPath { get; init; } = string.Empty;
    }

    private record EquipmentData
    {
        public string SlotName { get; init; } = string.Empty;
        public string ItemPath { get; init; } = string.Empty;
    }

    private record InventoryState
    {
        public List<SlotData> Slots { get; init; } = new();
        public List<EquipmentData> Equipped { get; init; } = new();
    }

    /// <summary>
    /// Handles serialising/deserialising the player's inventory and equipment.
    /// </summary>
    public static class InventorySerializer
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        /// <summary>
        /// Create a JSON string representing the current state.
        /// </summary>
        public static string Save(Game.Inventory.InventoryGrid grid, Game.Equipment.EquipmentComponent equipment)
        {
            var state = new InventoryState();

            // Inventory slots
            for (int i = 0; i < grid.SlotCount; i++)
            {
                var slot = grid.GetSlot(i);
                var path = slot?.Item?.ResourcePath ?? string.Empty;
                state.Slots.Add(new SlotData { Index = i, ItemPath = path });
            }

            // Equipped items
            foreach (var kvp in equipment.Slots)
            {
                var path = kvp.Value?.Item?.ResourcePath ?? string.Empty;
                state.Equipped.Add(new EquipmentData { SlotName = kvp.Key, ItemPath = path });
            }

            return JsonSerializer.Serialize(state, _jsonOptions);
        }

        /// <summary>
        /// Load a previously saved state into the grid and equipment.
        /// </summary>
        public static void Load(
            Game.Inventory.InventoryGrid grid,
            Game.Equipment.EquipmentComponent equipment,
            string json)
        {
            var state = JsonSerializer.Deserialize<InventoryState>(json, _jsonOptions);
            if (state == null) return;

            // Clear current state
            grid.ClearAll();
            equipment.ClearAll();

            // Restore inventory slots
            foreach (var slotData in state.Slots)
            {
                var item = LoadItem(slotData.ItemPath);
                if (item != null)
                {
                    grid.SetItem(slotData.Index, item);
                }
            }

            // Restore equipped items
            foreach (var equipData in state.Equipped)
            {
                var item = LoadItem(equipData.ItemPath);
                if (item != null)
                {
                    equipment.Equip(equipData.SlotName, item);
                }
            }

            // Fire change event so UI can refresh
            equipment.OnEquipmentChanged?.Invoke();
        }

        /// <summary>
        /// Helper to load an ItemResource from a resource path.
        /// </summary>
        private static ItemResource? LoadItem(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            var res = ResourceLoader.Load<ItemResource>(path);
            return res;
        }
    }
}
