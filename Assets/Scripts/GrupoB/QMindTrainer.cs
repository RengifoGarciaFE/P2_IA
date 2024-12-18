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

        private void ResetEnvironment() { }

        private int selectAction(State state) { return 0; }

        private (CellInfo, CellInfo) UpdateEnvironment(int action) { return }

        private void UpdateQtable(State state, int action, float reward, State nextState) { }

        private float CalculateReward(CellInfo agentPosition, CellInfo otherPosition) {  return 0; }
    }

    

}

