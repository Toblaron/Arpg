// SpatialGrid.cs – updated contract
using Godot;
using System.Collections.Generic;

namespace Game.Combat
{
    /// <summary>
    /// A simple uniform grid that maps world coordinates to a list of entities.
    /// </summary>
    public partial class SpatialGrid : Node
    {
        // Uniform cell size (square cells)
        private readonly int _cellSize;

        // Mapping from cell key → entities in that cell
        private readonly Dictionary<int, List<Entity>> _cells = new();

        public SpatialGrid(int cellSize)
        {
            _cellSize = cellSize;
        }

        // Helper to compute a unique key from cell coordinates
        private static int CellKey(int x, int y) => x + y * 1024; // 1024 is an arbitrary prime > max grid width

        /// <summary>
        /// Adds an entity to the grid based on its current global position.
        /// </summary>
        public void AddEntity(Entity entity)
        {
            var cellX = (int)(entity.GlobalPosition.X / _cellSize);
            var cellY = (int)(entity.GlobalPosition.Y / _cellSize);
            var key = CellKey(cellX, cellY);

            if (!_cells.TryGetValue(key, out var list))
            {
                list = new List<Entity>();
                _cells[key] = list;
            }
            list.Add(entity);
        }

        /// <summary>
        /// Removes an entity from its current cell.
        /// </summary>
        public void RemoveEntity(Entity entity)
        {
            var cellX = (int)(entity.GlobalPosition.X / _cellSize);
            var cellY = (int)(entity.GlobalPosition.Y / _cellSize);
            var key = CellKey(cellX, cellY);

            if (_cells.TryGetValue(key, out var list))
            {
                list.Remove(entity);
                if (list.Count == 0)
                    _cells.Remove(key);
            }
        }

        /// <summary>
        /// Queries all entities that lie within the square area defined by
        /// center and radius. Useful for projectile hit checks.
        /// </summary>
        public IEnumerable<Entity> Query(Vector2 center, float radius)
        {
            int minX = (int)((center.X - radius) / _cellSize);
            int maxX = (int)((center.X + radius) / _cellSize);
            int minY = (int)((center.Y - radius) / _cellSize);
            int maxY = (int)((center.Y + radius) / _cellSize);

            var results = new List<Entity>();

            for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            {
                var key = CellKey(x, y);
                if (_cells.TryGetValue(key, out var list))
                    results.AddRange(list);
            }

            return results;
        }
    }
}
