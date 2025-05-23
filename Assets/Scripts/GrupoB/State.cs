using NavigationDJIA.World;

namespace GrupoB
{
    public class State
    {
        public int relativeX;  // -1, 0, 1
        public int relativeY;  // -1, 0, 1
        public bool northFree;
        public bool southFree;
        public bool eastFree;
        public bool westFree;

        public string idState;

        public State(CellInfo agentPos, CellInfo enemyPos, WorldInfo world)
        {
            relativeX = GetRelativeDirection(enemyPos.x - agentPos.x);
            relativeY = GetRelativeDirection(enemyPos.y - agentPos.y);

            northFree = IsDirectionFree(agentPos, world, 0); // N
            eastFree = IsDirectionFree(agentPos, world, 1); // E
            southFree = IsDirectionFree(agentPos, world, 2); // S
            westFree = IsDirectionFree(agentPos, world, 3); // O

            idState = $"{relativeX}_{relativeY}_{(northFree ? 1 : 0)}_{(southFree ? 1 : 0)}_{(eastFree ? 1 : 0)}_{(westFree ? 1 : 0)}";
        }

        private int GetRelativeDirection(int diff)
        {
            if (diff < 0) return -1;
            else if (diff > 0) return 1;
            else return 0;
        }

        private bool IsDirectionFree(CellInfo pos, WorldInfo world, int dir)
        {
            CellInfo next = world.NextCell(pos, world.AllowedMovements.FromIntValue(dir));
            return next.Walkable;
        }
    }
}