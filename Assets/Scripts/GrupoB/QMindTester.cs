using NavigationDJIA.World;
using QMind.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrupoB
{
    public class QMindTester : IQMind
    {
        private WorldInfo _worldInfo;
        private QTable _qTable;
        private State currentState;
        public void Initialize(WorldInfo worldInfo)
        {
            _worldInfo= worldInfo;
            _qTable = new QTable();

            _qTable.Load();
        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            //Debug.Log("QMindTester: GetNextStep");
            //estado en el que estamos
            currentState = new State(currentPosition, otherPosition, _worldInfo);
            //accion a hacer
            int action = _qTable.GetAction(currentState);
            //siguiente posicion
            CellInfo newAgentPos = _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            
            return newAgentPos;
        }
    }
}

