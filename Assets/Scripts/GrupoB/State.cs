using NavigationDJIA.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrupoB 
{
    
    public class State : MonoBehaviour
    {

        public bool nWall;
        public bool sWall;
        public bool eWall;
        public bool wWall;

        public bool nEnemy;
        //private bool _sEnemy;
        public bool eEnemy;
        //private bool _wEnemy;

        public bool enemyNear = false;//cerca o lejos
        private float distance = 0;
        private const float threshold = 4;

        private CellInfo up;
        private CellInfo down;
        private CellInfo left;
        private CellInfo rigth;

        public string idState;

        public State(CellInfo agentPosition, CellInfo otherPosition, WorldInfo worldInfo)
        {
            //distancia enemigo
            distance = agentPosition.Distance(otherPosition, CellInfo.DistanceType.Manhattan);
            if (distance <= threshold)
            {
                enemyNear = true;//enemigo cerca
            }
            else
            {
                enemyNear = false;//enemigo lejos
            }

            //muros
            up = worldInfo.NextCell(agentPosition, worldInfo.AllowedMovements.FromIntValue(0));//0 = arriba (logica de array Directions)
            down = worldInfo.NextCell(agentPosition, worldInfo.AllowedMovements.FromIntValue(2));// 2 = abajo
            left = worldInfo.NextCell(agentPosition, worldInfo.AllowedMovements.FromIntValue(3));
            rigth = worldInfo.NextCell(agentPosition, worldInfo.AllowedMovements.FromIntValue(1));

            nWall = !up.Walkable;
            sWall = !down.Walkable;
            eWall = !rigth.Walkable;
            wWall = !left.Walkable;

            //direccion enemigo
            nEnemy = otherPosition.y > agentPosition.y; // Enemigo está arriba si su Y es mayor, si false abajo
            //_sEnemy = otherPosition.y < agentPosition.y; // Enemigo está al sur si su Y es menor
            eEnemy = otherPosition.x > agentPosition.x; // Enemigo está a la derecha si su X es mayor, si false izquierda
            //_wEnemy = otherPosition.x < agentPosition.x; // Enemigo está al oeste si su X es menor

            idState = GenerateId();
        }

        public State() { }
        public State(bool nWall, bool sWall, bool eWall, bool wWall, bool nEnemy, bool eEnemy, bool enemyNear)
        {
            this.nWall = nWall;
            this.sWall = sWall;
            this.eWall = eWall;
            this.wWall = wWall;
            this.nEnemy = nEnemy;
            this.eEnemy = eEnemy;
            this.enemyNear = enemyNear;


        }

        private string GenerateId()
        {
            return $"{(nWall ? 1 : 0)}" +
                   $"{(sWall ? 1 : 0)}" +
                   $"{(eWall ? 1 : 0)}" +
                   $"{(wWall ? 1 : 0)}" +
                   $"{(nEnemy ? 1 : 0)}" +
                   $"{(eEnemy ? 1 : 0)}" +
                   $"{(enemyNear ? 1 : 0)}";
        }

    }
}

   
    

