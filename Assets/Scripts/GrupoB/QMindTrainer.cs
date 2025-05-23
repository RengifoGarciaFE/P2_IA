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
    public class QMindTrainer : MonoBehaviour, IQMindTrainer
    {
        public QMindTrainerParams trainerParams;

        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; private set; } = 0f;
        public float ReturnAveraged { get; private set; } = 0f;
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm;

        private QTable _qTable;
        private int _episodeCount = 0;
        private int _stepCount = 0;
        private float totalReward = 0f;
        private bool terminal_state;

        private float cumulativeReturnSum = 0f;
        private Queue<CellInfo> lastPositions = new Queue<CellInfo>();
        private int stuckCheckWindow = 10;
        private int maxSamePositionCount = 8;

        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _worldInfo = worldInfo;
            trainerParams = qMindTrainerParams;

            navigationAlgorithm.Initialize(_worldInfo);
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(_worldInfo);

            _qTable = new QTable();

            string filePath = @"Assets/Scripts/GrupoB/TablaQ.csv";
            if (File.Exists(filePath))
                _qTable.Load();

            ResetEnvironment();
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        public void DoStep(bool train)
        {
            if (terminal_state || _stepCount >= trainerParams.maxSteps)
            {
                if (_stepCount >= trainerParams.maxSteps)
                    Debug.Log($"[QMindTrainer] Máximo de {trainerParams.maxSteps} pasos alcanzado, terminando episodio.");

                FinalizeEpisode();
                return;
            }

            State state = new State(AgentPosition, OtherPosition, _worldInfo);
            int action = selectAction(state);

            CellInfo oldAgentPos = AgentPosition;
            CellInfo oldEnemyPos = OtherPosition;

            (CellInfo newAgentPos, CellInfo newEnemyPos) = UpdateEnvironment(action);

            bool captured = (newAgentPos.x == newEnemyPos.x && newAgentPos.y == newEnemyPos.y);
            bool crossed = (newAgentPos.x == oldEnemyPos.x && newAgentPos.y == oldEnemyPos.y) &&
                           (newEnemyPos.x == oldAgentPos.x && newEnemyPos.y == oldAgentPos.y);

            State nextState = new State(newAgentPos, newEnemyPos, _worldInfo);
            float reward = CalculateReward(newAgentPos, newEnemyPos, captured || crossed);

            if (captured || crossed)
                terminal_state = true;

            totalReward += reward;
            Return = Mathf.Round(totalReward * 10f) / 10f;

            if (train)
                UpdateQtable(state, action, reward, nextState);

            AgentPosition = newAgentPos;
            OtherPosition = newEnemyPos;

            lastPositions.Enqueue(newAgentPos);
            if (lastPositions.Count > stuckCheckWindow)
                lastPositions.Dequeue();

            if (IsAgentStuck())
            {
                terminal_state = true;
                Debug.Log("[QMindTrainer] Agente atascado detectado, reiniciando episodio.");
            }

            if (terminal_state)
            {
                FinalizeEpisode();
                return;
            }

            _stepCount++;
            CurrentStep = _stepCount;
        }

        private void FinalizeEpisode()
        {
            if (CurrentEpisode > 0)
                cumulativeReturnSum += totalReward;

            ReturnAveraged = CurrentEpisode > 0 ? cumulativeReturnSum / CurrentEpisode : 0;
            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
            ResetEnvironment();
        }

        private bool IsAgentStuck()
        {
            if (lastPositions.Count < stuckCheckWindow)
                return false;

            var positionsArray = lastPositions.ToArray();
            int maxRepeats = 0;

            foreach (var pos in positionsArray)
            {
                int count = 0;
                foreach (var p in positionsArray)
                    if (p.x == pos.x && p.y == pos.y) count++;

                maxRepeats = Math.Max(maxRepeats, count);
            }

            return maxRepeats >= maxSamePositionCount;
        }

        private void ResetEnvironment()
        {
            bool validSpawn = false;
            int maxAttempts = 100;
            int attempts = 0;

            while (!validSpawn && attempts < maxAttempts)
            {
                AgentPosition = _worldInfo.RandomCell();
                OtherPosition = _worldInfo.RandomCell();

                if (!AgentPosition.Walkable || !OtherPosition.Walkable)
                {
                    attempts++;
                    continue;
                }

                var path = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
                if (path != null && path.Length > 0 && path[0] != null)
                    validSpawn = true;
                else
                    attempts++;
            }

            if (!validSpawn)
            {
                Debug.Log("[QMindTrainer] El agente está aislado y no puede ser atrapado. Saltando episodio.");
                _episodeCount++;
                CurrentEpisode = _episodeCount;
                totalReward = 0;
                Return = 0;
                OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
                return;
            }

            terminal_state = false;
            _episodeCount++;
            CurrentEpisode = _episodeCount;
            _stepCount = 0;
            totalReward = 0;
            Return = 0;
            lastPositions.Clear();

            if (_episodeCount % trainerParams.episodesBetweenSaves == 0 || _episodeCount == trainerParams.episodes)
            {
                _qTable.Save();
                Debug.Log($"[QMindTrainer] Tabla Q guardada en episodio {_episodeCount}.");
            }

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        private int selectAction(State state)
        {
            if (Random.value <= trainerParams.epsilon)
            {
                List<int> validActions = new List<int>();
                for (int a = 0; a < _qTable.actions; a++)
                {
                    CellInfo nextCell = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(a));
                    if (nextCell.Walkable)
                        validActions.Add(a);
                }
                return validActions.Count == 0 ? 0 : validActions[Random.Range(0, validActions.Count)];
            }
            else
            {
                float bestQ = float.NegativeInfinity;
                int bestAction = 0;
                float[] qValues = _qTable.qTable.ContainsKey(state.idState)
                    ? _qTable.qTable[state.idState]
                    : new float[_qTable.actions];

                for (int a = 0; a < _qTable.actions; a++)
                {
                    CellInfo nextCell = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(a));
                    if (nextCell.Walkable && qValues[a] > bestQ)
                    {
                        bestQ = qValues[a];
                        bestAction = a;
                    }
                }
                return bestAction;
            }
        }

        private (CellInfo, CellInfo) UpdateEnvironment(int action)
        {
            CellInfo newAgentPos = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            CellInfo[] newOtherPath = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            CellInfo newOtherPos = OtherPosition;

            if (newOtherPath.Length > 0 && newOtherPath[0] != null)
                newOtherPos = newOtherPath[0];

            return (newAgentPos, newOtherPos);
        }

        private void UpdateQtable(State state, int action, float reward, State nextState)
        {
            float actualQ = _qTable.GetQValue(state, action);
            float maxNextQ = _qTable.GetMaxQValue(nextState);
            float newQ = (1 - trainerParams.alpha) * actualQ + trainerParams.alpha * (reward + trainerParams.gamma * maxNextQ);
            _qTable.UpdateQValue(state, action, newQ);
        }

        private float CalculateReward(CellInfo newAgentPos, CellInfo newEnemyPos, bool terminal)
        {
            float oldDistance = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
            float newDistance = newAgentPos.Distance(newEnemyPos, CellInfo.DistanceType.Manhattan);

            if (terminal) return -1000f;
            return (newDistance >= oldDistance) ? 10f : -30f;
        }
    }
}