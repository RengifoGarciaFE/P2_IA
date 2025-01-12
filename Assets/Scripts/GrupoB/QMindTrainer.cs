using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace GrupoB
{
    public class QMindTrainer : IQMindTrainer
    {
        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; private set; }
        public float ReturnAveraged { get; private set; }
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        private QMindTrainerParams _qMindTrainerParams;
        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm;

        private QTable _qTable;
        private int _episodeCount = 0;
        private int _stepCount = 0;
        private float totalReward = 0;

        private bool terminal_state;

        public float epsilon;
        public float alpha;
        public float gamma;


        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            // Inicialización como antes
            _worldInfo = worldInfo;
            _qMindTrainerParams = qMindTrainerParams;
            navigationAlgorithm.Initialize(_worldInfo);
            _navigationAlgorithm = navigationAlgorithm;
            _qTable = new QTable();

            string filePath = @"Assets/Scripts/GrupoB/TablaQ.csv";
            // Obtener el directorio del archivo
            string directory = Path.GetDirectoryName(filePath);
            // Verificar si el directorio existe
            if (File.Exists(filePath))
            {
                Debug.Log("Archivo de tabla Q encontrado. Cargando...");
                _qTable.Load();
            }

            Debug.Log("QMindTrainerDummy: initialized");
            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);

            epsilon = _qMindTrainerParams.epsilon;
            alpha = _qMindTrainerParams.alpha;
            gamma = _qMindTrainerParams.gamma;
        }

        public void DoStep(bool train)
        {
            if (terminal_state)
            {
                ReturnAveraged = (float)(ReturnAveraged * 0.9 + Return * 0.1);
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                ResetEnvironment();
            }

            State state = new State(AgentPosition, OtherPosition, _worldInfo);
            int action = selectAction(state);
            (CellInfo newAgentPosition, CellInfo newOtherPosition) = UpdateEnvironment(action);
            State nextState = new State(newAgentPosition, newOtherPosition, _worldInfo);
            float reward = CalculateReward(newAgentPosition, newOtherPosition);
            totalReward += reward;
            Return = Mathf.Round(totalReward * 10) / 10;
            UpdateQtable(state, action, reward, nextState);

            AgentPosition = newAgentPosition;
            OtherPosition = newOtherPosition;

            _stepCount++;
            CurrentStep = _stepCount;
        }

        private void ResetEnvironment()
        {
            //reiniciar el entorno
            AgentPosition = _worldInfo.RandomCell();
            OtherPosition = _worldInfo.RandomCell();
            terminal_state = false;
            _episodeCount++;
            CurrentEpisode = _episodeCount;
            _stepCount = 0;
            CurrentStep = _stepCount;

            if (_episodeCount % _qMindTrainerParams.episodesBetweenSaves == 0)
            {
                _qTable.Save();
            }

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        private int selectAction(State state)
        {
            //seleccionar accion de forma aleatoria (epsilon) o la que mayor valor tenga en la tabla
            float random = Random.value;
            int action;
            if (random <= epsilon)
            {
                //Accion aleatoria
                action = Random.Range(0, _qTable.actions);
                return action;
            }
            else
            {
                //Accion de mayor valor Q
                action = _qTable.GetAction(state);
                return action;
            }
        }

        private (CellInfo, CellInfo) UpdateEnvironment(int action)
        {
            //mov Agente (nueva poscion del agente usando la accion)
            CellInfo newAgentPos = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            //mov Enemigo (nueva posicion del enemigo usando el algoritmo A*)
            CellInfo[] newOtherPath = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            CellInfo newOtherPos = OtherPosition;
            try
            {
                if (newOtherPath[0] != null)//a veces daba error por null
                {
                    newOtherPos = newOtherPath[0];
                }
            }
            catch (Exception e) { }

            return (newAgentPos, newOtherPos);
        }

        private void UpdateQtable(State state, int action, float reward, State nextState)
        {
            //actualizar los valores de la tabla con la ecuacion de la regla de aprendizaje
            float actualQValue = _qTable.GetQValue(state, action);
            float bestNextQValue = _qTable.GetMaxQValue(nextState);
            float newQValue = (1 - alpha) * actualQValue + alpha * (reward + gamma * bestNextQValue);
            _qTable.UpdateQValue(state, action, newQValue);
            // Depuración: Ver cómo se actualiza el valor de Q
            /*Debug.Log($"Estado: {state.idState}, Acción: {action}, Q actual: {actualQValue}, " +
                      $"Recompensa: {reward}, Q siguiente: {bestNextQValue}, Nuevo Q: {newQValue}");*/
        }

        private float CalculateReward(CellInfo agentPosition, CellInfo otherPosition)
        {
            //devolver la recompensa segun el estado nuevo
            float actualDistance = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
            float newDistance = agentPosition.Distance(otherPosition, CellInfo.DistanceType.Manhattan);


            bool near = newDistance <= 2; //si esta al lado del enemigo
            float reward = 0;

            //Si no caminable, o nos captura -> penalización y terminal state
            if (!agentPosition.Walkable)
            {
                terminal_state = true;
                return -1000;
            }

            if (agentPosition.x == otherPosition.x && agentPosition.y == otherPosition.y)
            {
                terminal_state = true;
                return -100;
            }


            //Debug.Log("nueva distancia " + newDistance + " actual distancia " + actualDistance);
            

            //si nos alejamos -> recompensa
            if (newDistance > actualDistance)
            {
                reward += 100;
            }
            else //si nos acercamos -> penalización
            {
                if (near) //si estamos al lado, más penalización
                {
                    reward -= 100;
                }
                reward -= 10;      
            }




            return reward;
        }

    }
}
