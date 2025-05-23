using NavigationDJIA.World;
using QMind.Interfaces;
using UnityEngine;
using System.Collections.Generic;
using NavigationDJIA.Interfaces; // Asegúrate de incluir esto para INavigationAlgorithm

namespace GrupoB
{
    public class QMindTester : IQMind
    {
        private WorldInfo _worldInfo;
        private QTable _qTable;
        private State currentState;

        private CellInfo _enemyPosition;
        private INavigationAlgorithm _navigationAlgorithm;

        public void Initialize(WorldInfo worldInfo)
        {
            _worldInfo = worldInfo;
            _qTable = new QTable();
            _qTable.Load();
            Debug.Log("[QMindTester] Q-Table cargada con " + _qTable.qTable.Count + " estados.");
        }

        // Método adicional para poder usar navegación como en el trainer
        public void SetNavigationAlgorithm(INavigationAlgorithm navigationAlgorithm)
        {
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(_worldInfo);
        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            _enemyPosition = otherPosition;

            currentState = new State(currentPosition, _enemyPosition, _worldInfo);
            Debug.Log($"[Frame {Time.frameCount}] Estado actual: {currentState.idState}");

            int distance = (int)currentPosition.Distance(_enemyPosition, CellInfo.DistanceType.Manhattan);
            Debug.Log($"[DISTANCIA] Agent - Enemy: {distance}");

            float bestQ = float.NegativeInfinity;
            int bestAction = -1;

            float[] qValues = _qTable.qTable.ContainsKey(currentState.idState)
                ? _qTable.qTable[currentState.idState]
                : new float[_qTable.actions];

            if (!_qTable.qTable.ContainsKey(currentState.idState))
                Debug.LogWarning($"[QMindTester] Estado no aprendido: {currentState.idState}");

            for (int action = 0; action < _qTable.actions; action++)
            {
                CellInfo candidatePos = _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
                if (candidatePos.Walkable)
                {
                    Debug.Log($"   Acción {action}: Q = {qValues[action]}, destino = ({candidatePos.x}, {candidatePos.y})");
                    if (qValues[action] > bestQ)
                    {
                        bestQ = qValues[action];
                        bestAction = action;
                    }
                }
            }

            if (bestAction == -1)
            {
                for (int action = 0; action < _qTable.actions; action++)
                {
                    CellInfo candidatePos = _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
                    if (candidatePos.Walkable)
                    {
                        bestAction = action;
                        Debug.LogWarning($"[QMindTester] Seleccionando acción válida por defecto: {action}");
                        break;
                    }
                }
            }

            if (bestAction == -1)
            {
                Debug.LogError("[QMindTester] No hay ninguna acción válida. Manteniéndose en posición actual.");
                return currentPosition;
            }

            CellInfo nextAgent = _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(bestAction));
            Debug.Log($"[QMindTester] Acción elegida: {bestAction}, Próxima celda: ({nextAgent.x}, {nextAgent.y})");

            // Movimiento del enemigo como en el trainer
            if (_navigationAlgorithm != null)
            {
                CellInfo[] path = _navigationAlgorithm.GetPath(_enemyPosition, currentPosition, 1);
                if (path.Length > 0 && path[0] != null)
                {
                    _enemyPosition = path[0];
                    Debug.Log($"[QMindTester] Enemigo se mueve a: ({_enemyPosition.x}, {_enemyPosition.y})");
                }
            }

            return nextAgent;
        }

        // Permite acceder a la posición actual del enemigo desde fuera
        public CellInfo GetEnemyPosition()
        {
            return _enemyPosition;
        }
    }
}