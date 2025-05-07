using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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
            _worldInfo = worldInfo;
            _qMindTrainerParams = qMindTrainerParams;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(_worldInfo);
            _qTable = new QTable();

            // Cargar tabla solo si existe
            string filePath = @"Assets/Scripts/GrupoB/TablaQ.csv";
            if (File.Exists(filePath))
            {
                Debug.Log("Archivo de tabla Q encontrado. Cargando...");
                _qTable.Load();
            }

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
                ReturnAveraged = ReturnAveraged * 0.9f + Return * 0.1f;
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                ResetEnvironment();
            }

            State state = new State(AgentPosition, OtherPosition, _worldInfo);
            int action = selectAction(state);
            (CellInfo newAgentPos, CellInfo newEnemyPos) = UpdateEnvironment(action);
            State nextState = new State(newAgentPos, newEnemyPos, _worldInfo);
            float reward = CalculateReward(newAgentPos, newEnemyPos);

            totalReward += reward;
            Return = Mathf.Round(totalReward * 10f) / 10f;

            if (train)
            {
                UpdateQtable(state, action, reward, nextState);
            }

            AgentPosition = newAgentPos;
            OtherPosition = newEnemyPos;
            _stepCount++;
            CurrentStep = _stepCount;

            Debug.Log($"[Ep {CurrentEpisode}] Step {CurrentStep} | Action: {action} | R: {reward} | Epsilon: {Math.Round(epsilon, 3)}");
        }

        private void ResetEnvironment()
        {
            do { AgentPosition = _worldInfo.RandomCell(); } while (!AgentPosition.Walkable);
            do { OtherPosition = _worldInfo.RandomCell(); } while (!OtherPosition.Walkable || AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan) < 3);

            terminal_state = false;
            _episodeCount++;
            CurrentEpisode = _episodeCount;
            _stepCount = 0;
            CurrentStep = 0;
            totalReward = 0;

            if (_episodeCount % _qMindTrainerParams.episodesBetweenSaves == 0 || _episodeCount == _qMindTrainerParams.episodes)
            {
                _qTable.Save();
                Debug.Log($"[Ep {_episodeCount}] Tabla Q guardada.");
            }

            if (epsilon > 0.1f)
            {
                epsilon *= 0.999f;
                epsilon = Mathf.Max(epsilon, 0.1f);
            }

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        private int selectAction(State state)
        {
            return (Random.value <= epsilon)
                ? Random.Range(0, _qTable.actions)
                : _qTable.GetAction(state);
        }

        private (CellInfo, CellInfo) UpdateEnvironment(int action)
        {
            CellInfo newAgentPos = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            CellInfo[] newOtherPath = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            CellInfo newOtherPos = (newOtherPath.Length > 0 && newOtherPath[0] != null) ? newOtherPath[0] : OtherPosition;
            return (newAgentPos, newOtherPos);
        }

        private void UpdateQtable(State state, int action, float reward, State nextState)
        {
            float actualQ = _qTable.GetQValue(state, action);
            float maxNextQ = _qTable.GetMaxQValue(nextState);
            float newQ = (1 - alpha) * actualQ + alpha * (reward + gamma * maxNextQ);
            _qTable.UpdateQValue(state, action, newQ);
        }

        private float CalculateReward(CellInfo newAgentPos, CellInfo newEnemyPos)
        {
            if (!newAgentPos.Walkable || (newAgentPos.x == newEnemyPos.x && newAgentPos.y == newEnemyPos.y))
            {
                terminal_state = true;
                return -1000f;
            }

            float oldDistance = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
            float newDistance = newAgentPos.Distance(newEnemyPos, CellInfo.DistanceType.Manhattan);

            float reward = (newDistance > oldDistance) ? 10f : -10f;
            reward -= 1f;

            return reward;
        }

        private void Update()
        {
            // ? Entrenamiento rápido sin depender de "train"
            if (_qMindTrainerParams != null && Application.isPlaying)
            {
                int stepsPerFrame = 100;
                for (int i = 0; i < stepsPerFrame; i++)
                {
                    DoStep(true);
                }
            }

            // ?? Guardado manual con tecla S
            if (Input.GetKeyDown(KeyCode.S))
            {
                _qTable.Save();
                Debug.Log("[Manual Save] Tabla Q guardada al pulsar S.");
            }
        }

        private void OnGUI()
        {
            GUIStyle guiStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.black }
            };

            GUI.Label(new Rect(10, 10, 400, 30), $"Episode: {CurrentEpisode} / Step: {CurrentStep}", guiStyle);
            GUI.Label(new Rect(10, 40, 400, 30), $"Avg Reward: {ReturnAveraged}", guiStyle);
            GUI.Label(new Rect(10, 70, 400, 30), $"Total Reward: {Return}", guiStyle);
            GUI.Label(new Rect(10, 100, 400, 30), $"Epsilon: {Mathf.Round(epsilon * 1000f) / 1000f}", guiStyle);
        }
    }
}