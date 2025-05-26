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

        //Porpiedades para monitorear el entrenamiento
        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; private set; } = 0f;
        public float ReturnAveraged { get; private set; } = 0f;

        //Eventos para determinar cuando comienza y termina un episodio
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        //Información del entorno y la navegación enemiga
        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm;
        
        //Tabla Q 
        private QTable _qTable;

        //Variables de control de episodios y pasos
        private int _episodeCount = 0;
        private int _stepCount = 0;
        private float totalReward = 0f;
        private bool terminal_state;

        //Para calcular el promedio de retorno
        private float cumulativeReturnSum = 0f;

        //Para detectar si el agente se queda atascado
        private Queue<CellInfo> lastPositions = new Queue<CellInfo>();
        private int stuckCheckWindow = 10;
        private int maxSamePositionCount = 8;

        //Iniciliza el entrenador
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

            //Prepara el entorno para el primer episodio
            ResetEnvironment();
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        //Realiza un paso en el entrenamiento
        public void DoStep(bool train)
        {
            //Si alcanza un estado terminal o el máximo, se termina el episodio
            if (terminal_state || _stepCount >= trainerParams.maxSteps)
            {
                if (_stepCount >= trainerParams.maxSteps)
                    Debug.Log($"[QMindTrainer] Máximo de {trainerParams.maxSteps} pasos alcanzado, terminando episodio.");

                FinalizeEpisode();
                return;
            }

            //Obtiene el estado actual
            State state = new State(AgentPosition, OtherPosition, _worldInfo);

            //Selecciona accion
            int action = selectAction(state);

            //Guarda posiciones previas
            CellInfo oldAgentPos = AgentPosition;
            CellInfo oldEnemyPos = OtherPosition;

            //Ejecuta acción y mueve enemigo
            (CellInfo newAgentPos, CellInfo newEnemyPos) = UpdateEnvironment(action);

            //Verifica si el agente fue capturado o cruzó con el enemigo
            bool captured = (newAgentPos.x == newEnemyPos.x && newAgentPos.y == newEnemyPos.y);
            bool crossed = (newAgentPos.x == oldEnemyPos.x && newAgentPos.y == oldEnemyPos.y) &&
                           (newEnemyPos.x == oldAgentPos.x && newEnemyPos.y == oldAgentPos.y);

            //Se define siguiente estado
            State nextState = new State(newAgentPos, newEnemyPos, _worldInfo);

            //Se calcula la recomensa
            float reward = CalculateReward(newAgentPos, newEnemyPos, captured || crossed);

            //Se marca si es terminal
            if (captured || crossed)
                terminal_state = true;

            //Se acumula la recomensa
            totalReward += reward;
            Return = Mathf.Round(totalReward * 10f) / 10f;

            //Actualiza la tabla Q si se está entrenando
            if (train)
                UpdateQtable(state, action, reward, nextState);

            //Actualiza posiciones del agente y enemigo
            AgentPosition = newAgentPos;
            OtherPosition = newEnemyPos;

            //Agrega a la cola cola para detectar bucles
            lastPositions.Enqueue(newAgentPos);
            if (lastPositions.Count > stuckCheckWindow)
                lastPositions.Dequeue();

            //Verifica si el agente está atascado
            if (IsAgentStuck())
            {
                terminal_state = true;
                Debug.Log("[QMindTrainer] Agente atascado detectado, reiniciando episodio.");
            }

            //Si termina, finaliza el episodio
            if (terminal_state)
            {
                FinalizeEpisode();
                return;
            }

            //Aumenta el contador de pasos
            _stepCount++;
            CurrentStep = _stepCount;
        }

        //Finaliza el episodio actual y reincia 
        private void FinalizeEpisode()
        {
            if (CurrentEpisode > 0)
                cumulativeReturnSum += totalReward;

            ReturnAveraged = CurrentEpisode > 0 ? cumulativeReturnSum / CurrentEpisode : 0;
            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
            ResetEnvironment();
        }

        //Verifica si el agente está dando vueltas o está atrapado en una zona
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

        //Reinicia el entorno
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

            //Si no es posible un entorno válido salta episodio
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

            //Restablece contadores y estado
            terminal_state = false;
            _episodeCount++;
            CurrentEpisode = _episodeCount;
            _stepCount = 0;
            totalReward = 0;
            Return = 0;
            lastPositions.Clear();

            //Guarda la tabla
            if (_episodeCount % trainerParams.episodesBetweenSaves == 0 || _episodeCount == trainerParams.episodes)
            {
                _qTable.Save();
                Debug.Log($"[QMindTrainer] Tabla Q guardada en episodio {_episodeCount}.");
            }

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        //Selecciona acción
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
                //Escoge la mejor acción conocida
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

        //Ejecuta el movimiento del agente y del enemigo
        private (CellInfo, CellInfo) UpdateEnvironment(int action)
        {
            CellInfo newAgentPos = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            CellInfo[] newOtherPath = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            CellInfo newOtherPos = OtherPosition;

            if (newOtherPath.Length > 0 && newOtherPath[0] != null)
                newOtherPos = newOtherPath[0];

            return (newAgentPos, newOtherPos);
        }

        //Aplica la formula de Q-learning 
        private void UpdateQtable(State state, int action, float reward, State nextState)
        {
            float actualQ = _qTable.GetQValue(state, action);
            float maxNextQ = _qTable.GetMaxQValue(nextState);
            float newQ = (1 - trainerParams.alpha) * actualQ + trainerParams.alpha * (reward + trainerParams.gamma * maxNextQ);
            _qTable.UpdateQValue(state, action, newQ);
        }

        //Calcula recompensa por paso
        private float CalculateReward(CellInfo newAgentPos, CellInfo newEnemyPos, bool terminal)
        {
            float oldDistance = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
            float newDistance = newAgentPos.Distance(newEnemyPos, CellInfo.DistanceType.Manhattan);

            if (terminal) return -1000f;
            return (newDistance >= oldDistance) ? 10f : -30f;
        }
    }
}