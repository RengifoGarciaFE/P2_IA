using NavigationDJIA.World;
using QMind.Interfaces;
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
            _worldInfo = worldInfo;
            _qTable = new QTable();

            // Cargar la tabla Q entrenada
            _qTable.Load();
            Debug.Log("[QMindTester] Tabla Q cargada.");
        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            // Crear el estado actual
            currentState = new State(currentPosition, otherPosition, _worldInfo);

            // Obtener la mejor acción según la tabla Q aprendida
            int action = _qTable.GetAction(currentState);

            // Calcular la nueva posición resultante de aplicar la acción
            CellInfo newAgentPos = _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(action));

            return newAgentPos;
        }
    }
}