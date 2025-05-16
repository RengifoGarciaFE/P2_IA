using NavigationDJIA.World;
using QMind.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace GrupoB
{
    public class QMindTester : IQMind
    {
        private WorldInfo _worldInfo; //ref al entorno
        private QTable _qTable; //nuestra tablaQ 
        private State currentState; //estado actual del agente

        public void Initialize(WorldInfo worldInfo)
        {
            _worldInfo = worldInfo;
            _qTable = new QTable();

            // Cargar la tabla Q entrenada
            _qTable.Load();
            Debug.Log("[QMindTester] Tabla Q cargada.");
        }

        //Este métdo recibirá la posición del agente y la del enemigo
        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            // Crear el estado actual
            currentState = new State(currentPosition, otherPosition, _worldInfo);

            // Obtener los Q-values para todas las acciones posibles
            float[] qValues = _qTable.qTable.ContainsKey(currentState.idState)
                ? _qTable.qTable[currentState.idState]
                : new float[_qTable.actions];

            int bestValidAction = -1;
            float bestQ = float.NegativeInfinity;

            // Buscar la mejor acción válida (que no vaya a una celda bloqueada)
            for (int i = 0; i < qValues.Length; i++)
            {
                CellInfo next = _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(i));
                if (next.Walkable && qValues[i] > bestQ)
                {
                    bestQ = qValues[i];
                    bestValidAction = i;
                }
            }

            if (bestValidAction != -1)
            {
                return _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(bestValidAction));
            }
            else
            {
                Debug.LogWarning($"[QMindTester] Ninguna acción válida desde {currentPosition}. Se queda quieto.");
                return currentPosition;
            }
        }

    }
}