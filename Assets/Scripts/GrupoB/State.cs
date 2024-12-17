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
        private bool _sEnemy;
        private bool _eEnemy;
        private bool _wEnemy;

        private bool enemyNear;//cerca o lejos
        

        private int _idState;

        public State(bool nWall, bool sWall, bool eWall, bool wWall, bool nEnemy, bool sEnemy, bool eEnemy, bool wEnemy, bool enemyNear, int idState)
        {
            NWall = nWall;
            SWall = sWall;
            EWall = eWall;
            WWall = wWall;
            NEnemy = nEnemy;
            SEnemy = sEnemy;
            EEnemy = eEnemy;
            WEnemy = wEnemy;
            this.EnemyNear = enemyNear;
            IdState = idState;
        }

        public bool NWall { get => _nWall; set => _nWall = value; }
        public bool SWall { get => _sWall; set => _sWall = value; }
        public bool EWall { get => _eWall; set => _eWall = value; }
        public bool WWall { get => _wWall; set => _wWall = value; }
        public bool NEnemy { get => _nEnemy; set => _nEnemy = value; }
        public bool SEnemy { get => _sEnemy; set => _sEnemy = value; }
        public bool EEnemy { get => _eEnemy; set => _eEnemy = value; }
        public bool WEnemy { get => _wEnemy; set => _wEnemy = value; }
        public bool EnemyNear { get => enemyNear; set => enemyNear = value; }
        public int IdState { get => _idState; set => _idState = value; }
    }
}

    // Start is called before the first frame update
    

