﻿using NavigationDJIA.World;
using QMind.Interfaces;
using UnityEngine;
using System.Collections.Generic;
using NavigationDJIA.Interfaces;

namespace GrupoB
{
    public class QMindTester : IQMind
    {
        //Información del entorno
        private WorldInfo _worldInfo;
        //Tabla Q cargada desde archivo
        private QTable _qTable;
        //Algoritmo de navegación
        private INavigationAlgorithm _navigationAlgorithm;
        //Estado actual del agente
        private State currentState;
        //Posicion del enemigo
        private CellInfo _enemyPosition;

        //Inicialización general
        public void Initialize(WorldInfo worldInfo)
        {
            _worldInfo = worldInfo;
            _qTable = new QTable();
            _qTable.Load();
            Debug.Log("[QMindTester] Q-Table cargada con " + _qTable.qTable.Count + " estados.");
        }

        // Inyectar algoritmo de navegación externo (como en Trainer)
        public void SetNavigationAlgorithm(INavigationAlgorithm navigationAlgorithm)
        {
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(_worldInfo);
        }

        //Decide la siguiente celda a la que se moverá el agente
        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            // 1. El enemigo se mueve primero
            _enemyPosition = otherPosition;
            if (_navigationAlgorithm != null)
            {
                CellInfo[] path = _navigationAlgorithm.GetPath(_enemyPosition, currentPosition, 1);
                if (path.Length > 0 && path[0] != null)
                {
                    _enemyPosition = path[0];
                    Debug.Log($"[ENEMIGO] Se mueve primero a: ({_enemyPosition.x}, {_enemyPosition.y})");
                }
            }

            // 2. El agente reacciona
            currentState = new State(currentPosition, _enemyPosition, _worldInfo);
            Debug.Log($"[Frame {Time.frameCount}] Estado actual: {currentState.idState}");

            int distance = (int)currentPosition.Distance(_enemyPosition, CellInfo.DistanceType.Manhattan);
            Debug.Log($"[DISTANCIA] Agent - Enemy: {distance}");

            float bestQ = float.NegativeInfinity;//Inicializa la mejor Q encontrada
            int bestAction = -1;//Acción asociada a la mejor Q

            float[] qValues = _qTable.qTable.ContainsKey(currentState.idState)
                ? _qTable.qTable[currentState.idState]
                : new float[_qTable.actions];

            if (!_qTable.qTable.ContainsKey(currentState.idState))
                Debug.LogWarning($"[QMindTester] Estado no aprendido: {currentState.idState}");

            //Se recorren todas las acciones posibles
            for (int action = 0; action < _qTable.actions; action++)
            {
                //Se obtiene la celda resultante si se realiza la acción
                CellInfo candidatePos = _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
                if (candidatePos.Walkable)
                {
                    Debug.Log($"   Acción {action}: Q = {qValues[action]}, destino = ({candidatePos.x}, {candidatePos.y})");
                    if (qValues[action] > bestQ)
                    {
                        //Se seleccion la acción con mejor valor Q
                        bestQ = qValues[action];
                        bestAction = action;
                    }
                }
            }
            //Si no se encontró ninguna acción válida conocida, se eleige la primera válida que se encuentre 
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
            //Si sigue sin encontrarse acción válida, se queda en la posición actual
            if (bestAction == -1)
            {
                Debug.LogError("[QMindTester] No hay ninguna acción válida. Manteniéndose en posición actual.");
                return currentPosition;
            }
            //Se devuelve la siguiente celda del agente según la acción elegida
            CellInfo nextAgent = _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(bestAction));
            Debug.Log($"[AGENTE] Luego reacciona y va a: ({nextAgent.x}, {nextAgent.y})");

            return nextAgent;
        }

        // Exponer la nueva posición del enemigo para quien llame a este tester
        public CellInfo GetEnemyPosition()
        {
            return _enemyPosition;
        }
}
}