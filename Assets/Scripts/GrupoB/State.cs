using NavigationDJIA.World;
using UnityEngine;

namespace GrupoB
{
    public class State
    {
        // Informaci�n estructural
        public bool nWall, sWall, eWall, wWall;     // Presencia de muros alrededor
        public bool enemyNear;                      // �Est� el enemigo cerca?
        public string enemyDirection;               // Direcci�n cardinal hacia el enemigo (N, S, E, W, NE, etc.)
        public string agentZone;                    // Zona del mapa (ej. cuadrante o sector)

        public string idState;                      // Identificador �nico del estado

        private const float nearThreshold = 3.0f;   // Distancia para considerar que el enemigo est� cerca

        public State(CellInfo agentPos, CellInfo enemyPos, WorldInfo world)
        {
            // Muros
            nWall = !world.NextCell(agentPos, world.AllowedMovements.FromIntValue(0)).Walkable;
            eWall = !world.NextCell(agentPos, world.AllowedMovements.FromIntValue(1)).Walkable;
            sWall = !world.NextCell(agentPos, world.AllowedMovements.FromIntValue(2)).Walkable;
            wWall = !world.NextCell(agentPos, world.AllowedMovements.FromIntValue(3)).Walkable;

            // Cercan�a al enemigo
            float distance = agentPos.Distance(enemyPos, CellInfo.DistanceType.Manhattan);
            enemyNear = distance <= nearThreshold;

            // Direcci�n del enemigo relativa al agente
            enemyDirection = CalculateEnemyDirection(agentPos, enemyPos);

            // Zona del mapa (dividimos en 4 cuadrantes como ejemplo)
            agentZone = CalculateMapZone(agentPos, world);

            // ID de estado codificado
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
                   $"_{(enemyNear ? 1 : 0)}_{enemyDirection}_{agentZone}";
        }
    }
}
