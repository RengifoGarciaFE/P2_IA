using NavigationDJIA.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrupoB 
{
    
    public class State : MonoBehaviour
    {

        private bool _nWall;
        private bool _sWall;
        private bool _eWall;
        private bool _wWall;

        private bool _nEnemy;
        //private bool _sEnemy;
        private bool _eEnemy;
        //private bool _wEnemy;

        private bool enemyNear = false;//cerca o lejos
        private float distance = 0;
        private float threshold = 4;

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

            _nWall = !up.Walkable;
            _sWall = !down.Walkable;
            _eWall = !left.Walkable;
            _wWall = !rigth.Walkable;

            //direccion enemigo
            _nEnemy = otherPosition.y > agentPosition.y; // Enemigo está arriba si su Y es mayor, si false abajo
            //_sEnemy = otherPosition.y < agentPosition.y; // Enemigo está al sur si su Y es menor
            _eEnemy = otherPosition.x > agentPosition.x; // Enemigo está a la derecha si su X es mayor, si false izquierda
            //_wEnemy = otherPosition.x < agentPosition.x; // Enemigo está al oeste si su X es menor


        }

        public State(bool nWall, bool sWall, bool eWall, bool wWall, bool nEnemy, bool sEnemy, bool eEnemy, bool wEnemy, bool enemyNear, int idState)
        {
            NWall = nWall;
            SWall = sWall;
            EWall = eWall;
            WWall = wWall;
            NEnemy = nEnemy;
            //SEnemy = sEnemy;
            EEnemy = eEnemy;
            //WEnemy = wEnemy;
            this.EnemyNear = enemyNear;
            IdState = idState;
        }

        public bool NWall { get => _nWall; set => _nWall = value; }
        public bool SWall { get => _sWall; set => _sWall = value; }
        public bool EWall { get => _eWall; set => _eWall = value; }
        public bool WWall { get => _wWall; set => _wWall = value; }
        public bool NEnemy { get => _nEnemy; set => _nEnemy = value; }
        //public bool SEnemy { get => _sEnemy; set => _sEnemy = value; }
        public bool EEnemy { get => _eEnemy; set => _eEnemy = value; }
        //public bool WEnemy { get => _wEnemy; set => _wEnemy = value; }
        public bool EnemyNear { get => enemyNear; set => enemyNear = value; }
        public int IdState { get => _idState; set => _idState = value; }
    }
}

   
    

