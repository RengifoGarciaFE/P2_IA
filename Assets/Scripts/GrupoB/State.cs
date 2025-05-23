using NavigationDJIA.World;

namespace GrupoB
{
    public class State
    {
        public int relativeX;  // -1, 0, 1
        public int relativeY;  // -1, 0, 1
        public int freePaths;  // 0 a 4
        public bool isCorner;  // true si está en una esquina del mapa
        public string idState;

        public State(CellInfo agentPos, CellInfo enemyPos, WorldInfo world)
        {
            relativeX = GetRelativeDirection(enemyPos.x - agentPos.x);
            relativeY = GetRelativeDirection(enemyPos.y - agentPos.y);
            freePaths = CountFreePaths(agentPos, world);

            isCorner = IsCorner(agentPos, world);

            idState = $"{relativeX}_{relativeY}_{freePaths}_{(isCorner ? 1 : 0)}";
        }

        private int GetRelativeDirection(int diff)
        {
            if (diff < 0) return -1;
            else if (diff > 0) return 1;
            else return 0;
        }

        private int CountFreePaths(CellInfo pos, WorldInfo world)
        {
            int count = 0;
            for (int i = 0; i < 4; i++)
            {
                CellInfo next = world.NextCell(pos, world.AllowedMovements.FromIntValue(i));
                if (next.Walkable)
                    count++;
            }
            return count;
        }

        private bool IsCorner(CellInfo pos, WorldInfo world)
        {
            int maxX = world.WorldSize.x - 1;
            int maxY = world.WorldSize.y - 1;
            return
                (pos.x == 0 && pos.y == 0) ||
                (pos.x == 0 && pos.y == maxY) ||
                (pos.x == maxX && pos.y == 0) ||
                (pos.x == maxX && pos.y == maxY);
        }
    }
}