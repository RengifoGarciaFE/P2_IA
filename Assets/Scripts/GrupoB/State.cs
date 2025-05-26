using NavigationDJIA.World;

namespace GrupoB
{
    public class State
    {
        //Numero que indican posicion relativa del enemigo (-1,0,1)
        //En el eje x -1: izquierda, 0: misma columna, 1: derecha
        public int relativeX;
        //En el eje y -1: abajo, 0: misma fila, 1: arriba
        public int relativeY;  

        //variables boleanas que indican si pueden moverse en cada direcci�n
        public bool northFree;
        public bool southFree;
        public bool eastFree;
        public bool westFree;

        //identificador de estado
        public string idState;

        //Constructor
        public State(CellInfo agentPos, CellInfo enemyPos, WorldInfo world)
        {
            //Determina la posicion relativa del enemigo
            relativeX = GetRelativeDirection(enemyPos.x - agentPos.x);  
            relativeY = GetRelativeDirection(enemyPos.y - agentPos.y);  
            
            //Eval�a las direcciones cardinales
            northFree = IsDirectionFree(agentPos, world, 0); // N
            eastFree = IsDirectionFree(agentPos, world, 1); // E
            southFree = IsDirectionFree(agentPos, world, 2); // S
            westFree = IsDirectionFree(agentPos, world, 3); // O

            idState = $"{relativeX}_{relativeY}_{(northFree ? 1 : 0)}_{(southFree ? 1 : 0)}_{(eastFree ? 1 : 0)}_{(westFree ? 1 : 0)}";
        }

        //M�todo que devuelve un valor en funci�n de la posici�n del enemigo y el agente
        private int GetRelativeDirection(int diff)
        {
            if (diff < 0) return -1;//si el enemigo est� antes
            else if (diff > 0) return 1;//si el enemigo est� despu�s
            else return 0;//si eel enemigo est� en la misma posici�n
        }

        //Determina si el agente puede moverse en la direcci�n dada desde su posici�n actual
        private bool IsDirectionFree(CellInfo pos, WorldInfo world, int dir)
        {
            CellInfo next = world.NextCell(pos, world.AllowedMovements.FromIntValue(dir));
            return next.Walkable;
        }
    }
}