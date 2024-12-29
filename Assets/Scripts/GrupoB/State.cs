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

        private int _idState;

        public State(CellInfo agentPosition, CellInfo otherPosition)
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
            up = new CellInfo(agentPosition.x, agentPosition.y + 1);
            down = new CellInfo(agentPosition.x, agentPosition.y - 1);
            left = new CellInfo(agentPosition.x-1, agentPosition.y);
            rigth = new CellInfo(agentPosition.x+1, agentPosition.y);

            nWall = !up.Walkable;
            sWall = !down.Walkable;
            eWall = !left.Walkable;
            wWall = !rigth.Walkable;

            //direccion enemigo
            nEnemy = otherPosition.y > agentPosition.y; // Enemigo está arriba si su Y es mayor, si false abajo
            //_sEnemy = otherPosition.y < agentPosition.y; // Enemigo está al sur si su Y es menor
            eEnemy = otherPosition.x > agentPosition.x; // Enemigo está a la derecha si su X es mayor, si false izquierda
            //_wEnemy = otherPosition.x < agentPosition.x; // Enemigo está al oeste si su X es menor


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
        
    }
}

   
    

