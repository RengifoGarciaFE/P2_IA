using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace GrupoB
{
    public class QMindTrainer : IQMindTrainer
    {
        public int CurrentEpisode { get; }
        public int CurrentStep { get; }
        public CellInfo AgentPosition { get; private set; } //agente que estamos implementando, aca vez que se actualiza va alli
        public CellInfo OtherPosition { get; private set; } //posicion del enemigo
        public float Return { get; }
        public float ReturnAveraged { get; private set; }
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        private QMindTrainerParams _qMindTrainerParams;
        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm;

        private QTable _qTable;
        private int _episodeCount=0;

        private bool terminal_state;

        public float epsilon;
        public float alpha;
        public float gamma;


        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            //inicializamos atributos de la clase para poder acceder a ellos
            _worldInfo = worldInfo;
            _qMindTrainerParams = qMindTrainerParams;
            navigationAlgorithm.Initialize(_worldInfo);
            _navigationAlgorithm = navigationAlgorithm;
            _qTable = new QTable();

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
            //movimiento enemigo
            /*int action = Random.Range(0, 4);
            CellInfo newAgentPos = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            AgentPosition = newAgentPos;
            CellInfo[] newOtherPath = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            if (newOtherPath != null)
            {
                OtherPosition = newOtherPath[0];
            }*/

            //movimiento agente
            /*if (terminal_state)
            {
                state = RandomState()
            }
            state = getstategraph(agentposition, otherpos);
            action = selecaction(state, available_actions);
            next_state, reward = moveAgentOther(state, action);
            updateQtable(state, action, next_state, reward)*/

            if (terminal_state)
            {
                ReturnAveraged = (float)(ReturnAveraged * 0.9 + Return * 0.1);
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);//¿?
                ResetEnvironment();
            }
            State state = new State(AgentPosition, OtherPosition);
            int action = selectAction(state);
            (CellInfo newAgentPosition, CellInfo newOtherPosition) = UpdateEnvironment(action);
            State nextState = new State(newAgentPosition, newOtherPosition);
            float reward = CalculateReward(newAgentPosition, newOtherPosition);
            UpdateQtable(state, action, reward, nextState);

            AgentPosition = newAgentPosition;
            OtherPosition = newOtherPosition;


        }

        private void ResetEnvironment() 
        {
            //reiniciar el entorno
            AgentPosition = _worldInfo.RandomCell();
            OtherPosition = _worldInfo.RandomCell();
            terminal_state = false;
            _episodeCount++;
            if (_episodeCount % _qMindTrainerParams.episodesBetweenSaves == 0)
            {
                _qTable.Save();
            }
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);//¿?
        }

        private int selectAction(State state)
        {
            //seleccionar accion de forma a aleatroia (epsilon) o la que mayor valor tenga en la tabla
            float random = Random.value;
            int action;
            if(random <= epsilon)
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
            //AgentPosition = newAgentPos;
            //mov Enemigo (nueva posicion del enemigo usando el algoritmo A*)
            CellInfo[] newOtherPath = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            CellInfo newOtherPos = newOtherPath[0];//////////////**************************A VECES DA INDEXOUTOFBOUNDS  
            /*if (newOtherPath != null)
            {
                newOtherPos = newOtherPath[0];
            }*/

            return (newAgentPos,newOtherPos);
                
        }

        private void UpdateQtable(State state, int action, float reward, State nextState) 
        {
            //actualizar los valores de la tabla con la ecuacion de la regla de aprendizaje
            float actualQValue = _qTable.GetQValue(state, action);
            float bestNextQValue = _qTable.GetMaxQValue(nextState);
            float newQValue = (1 - alpha) * actualQValue + alpha * (reward + gamma * bestNextQValue);
            _qTable.UpdateQValue(state,action,newQValue);
        }

        private float CalculateReward(CellInfo agentPosition, CellInfo otherPosition)
        {
            //devolver la recompensa segun el estado nuevo
            float actualDistance = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
            float newDistance = agentPosition.Distance(otherPosition, CellInfo.DistanceType.Manhattan);
            bool near = newDistance <= 2; //si esta al lado del enemigo
            float reward = 0;
            //Si no caminable, o nos captura -> penalizacion y terminal state
            if (!agentPosition.Walkable || agentPosition.x == otherPosition.x && agentPosition.y == otherPosition.y)
            {
                terminal_state = true;
                return -1000;
            }
            //si nos alejamos -> recompensa
            if(newDistance > actualDistance)
            {
                reward = +100;
            }
            else//si nos acercamos -> penalizacion
            {
                if (near) //si estamos al lado, mas penalizacion
                {
                    reward = -100;
                }
                reward = -20;
            }
            
            return reward;
        }
    }

    

}

