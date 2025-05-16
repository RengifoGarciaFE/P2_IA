using NavigationDJIA.World;
using UnityEngine;

namespace GrupoB
{
    public class State
    {
        // Información entorno
        public bool nWall, sWall, eWall, wWall;     // Muros adyacentes
        public bool enemyNear;                      // ¿Enemigo cerca?
        public string enemyDirection;               // Dirección cardinal hacia enemigo
        public string agentZone;                    // Cuadrante del mapa
        public int freePaths;                       // Caminos abiertos desde la celda actual
        public bool isEdgeZone;                     // ¿Está cerca del borde?
        public string idState;                      // Identificador único del estado

        private const float nearThreshold = 3.0f;

        public State(CellInfo agentPos, CellInfo enemyPos, WorldInfo world)
        {
            // Muros
            nWall = !world.NextCell(agentPos, world.AllowedMovements.FromIntValue(0)).Walkable;
            eWall = !world.NextCell(agentPos, world.AllowedMovements.FromIntValue(1)).Walkable;
            sWall = !world.NextCell(agentPos, world.AllowedMovements.FromIntValue(2)).Walkable;
            wWall = !world.NextCell(agentPos, world.AllowedMovements.FromIntValue(3)).Walkable;

            // Cercanía al enemigo
            float distance = agentPos.Distance(enemyPos, CellInfo.DistanceType.Manhattan);
            enemyNear = distance <= nearThreshold;

            // Dirección relativa al enemigo
            enemyDirection = CalculateEnemyDirection(agentPos, enemyPos);

            // Zona del mapa (cuadrante)
            agentZone = CalculateMapZone(agentPos, world);

            // Caminos libres
            freePaths = 0;
            if (!nWall) freePaths++;
            if (!sWall) freePaths++;
            if (!eWall) freePaths++;
            if (!wWall) freePaths++;

            // ¿Está en una zona de borde?
            isEdgeZone = agentPos.x == 0 || agentPos.x == world.WorldSize.x - 1 ||
                         agentPos.y == 0 || agentPos.y == world.WorldSize.y - 1;

            // Codificar el estado
            idState = GenerateId();
        }

        private string CalculateEnemyDirection(CellInfo agent, CellInfo enemy)
        {
            int dx = enemy.x - agent.x;
            int dy = enemy.y - agent.y;

            if (dx == 0 && dy > 0) return "N";
            if (dx == 0 && dy < 0) return "S";
            if (dx > 0 && dy == 0) return "E";
            if (dx < 0 && dy == 0) return "W";
            if (dx > 0 && dy > 0) return "NE";
            if (dx < 0 && dy > 0) return "NW";
            if (dx > 0 && dy < 0) return "SE";
            if (dx < 0 && dy < 0) return "SW";
            return "SAME";
        }

        private string CalculateMapZone(CellInfo pos, WorldInfo world)
        {
            int midX = world.WorldSize.x / 2;
            int midY = world.WorldSize.y / 2;

            if (pos.x < midX && pos.y < midY) return "Q1"; // Top-left
            if (pos.x >= midX && pos.y < midY) return "Q2"; // Top-right
            if (pos.x < midX && pos.y >= midY) return "Q3"; // Bottom-left
            return "Q4"; // Bottom-right
        }

        private string GenerateId()
        {
            return $"{(nWall ? 1 : 0)}{(sWall ? 1 : 0)}{(eWall ? 1 : 0)}{(wWall ? 1 : 0)}" +
                   $"_{(enemyNear ? 1 : 0)}_{enemyDirection}_{agentZone}_{freePaths}_{(isEdgeZone ? 1 : 0)}";
        }
    }
}
