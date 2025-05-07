using NavigationDJIA.World; 
using UnityEngine;

namespace GrupoB
{
    public class State
    {
        // Información entorno
        public bool nWall, sWall, eWall, wWall;     // Hay muros alrededor?
        public bool enemyNear;                      // Está el enemigo cerca?
        public string enemyDirection;               // Dirección cardinal hacia el enemigo (N, S, E, W, NE, etc.)
        public string agentZone;                    // Zona del mapa 

        public string idState;                      // Identificador del estado

        private const float nearThreshold = 3.0f;   // Distancia para considerar que el enemigo está cerca

        //-->Constructor, creará un nuevo estado dado la pos del agente, la del enemigo y la info del entorno
        public State(CellInfo agentPos, CellInfo enemyPos, WorldInfo world)
        {
            // Muros (usamos nextcell para ver cual es la celda adyacente en esa dirección)
            nWall = !world.NextCell(agentPos, world.AllowedMovements.FromIntValue(0)).Walkable;
            eWall = !world.NextCell(agentPos, world.AllowedMovements.FromIntValue(1)).Walkable;
            sWall = !world.NextCell(agentPos, world.AllowedMovements.FromIntValue(2)).Walkable;
            wWall = !world.NextCell(agentPos, world.AllowedMovements.FromIntValue(3)).Walkable;

            // Cercanía al enemigo (calculamos distancia manhattan entre agente y enem si es <=3 esq ue está cerca)
            float distance = agentPos.Distance(enemyPos, CellInfo.DistanceType.Manhattan);
            enemyNear = distance <= nearThreshold;

            // Dirección del enemigo relativa al agente
            enemyDirection = CalculateEnemyDirection(agentPos, enemyPos);

            // Zona del mapa (dividimos en 4 cuadrantes para determnar en cual está el agente)
            agentZone = CalculateMapZone(agentPos, world);

            // ID de estado codificado
            idState = GenerateId();
        }

        //--> Calcula la dirección cardinal desde el agente hacia el enemigo
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

        private string GenerateId() //convertir info del estado en una cadena
        {
            return $"{(nWall ? 1 : 0)}{(sWall ? 1 : 0)}{(eWall ? 1 : 0)}{(wWall ? 1 : 0)}" +
                   $"_{(enemyNear ? 1 : 0)}_{enemyDirection}_{agentZone}";
        }
    }
}
