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
        public float Return { get; private set; } //recompensa total
        public float ReturnAveraged { get; private set; } //promedio
        //para notificar cuand comienza o acaba un episodio
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

        [Range(0f, 1f)]
        public float epsilon = 0.9f;
        private float epsilonInicial;

        [Range(0f, 1f)]
        public float minEpsilon = 0.1f;
        public int episodes = 5000;

        public float alpha; //tasa de aprendizaje
        public float gamma; //factor de descuento

        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _worldInfo = worldInfo;
            _qMindTrainerParams = qMindTrainerParams;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(_worldInfo);
            _qTable = new QTable();

            // Cargar tabla si existe
            string filePath = @"Assets/Scripts/GrupoB/TablaQ.csv";
            if (File.Exists(filePath))
            {
                Debug.Log("Archivo de tabla Q encontrado. Cargando...");
                _qTable.Load();
            }

            // Posiciones iniciales
            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);

            // Parámetros
            epsilon = _qMindTrainerParams.epsilon;
            epsilonInicial = epsilon; // se guarda valor fijo inicial
            alpha = _qMindTrainerParams.alpha;
            gamma = _qMindTrainerParams.gamma;
        }


        public void DoStep(bool train) //bucle de entrenamiento (importante) / train solo para actualizar tabla o no 
        {
            if (terminal_state)//si el episodio anterior a terminado devuelve la media de recompensas lanza el evnto y resetea
            {
                ReturnAveraged = ReturnAveraged * 0.9f + Return * 0.1f;
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                ResetEnvironment();
            }

            State state = new State(AgentPosition, OtherPosition, _worldInfo); //estado actual del entorno
            int action = selectAction(state); //decide que acción tomar
            (CellInfo newAgentPos, CellInfo newEnemyPos) = UpdateEnvironment(action); //aplicar acción elegida
            State nextState = new State(newAgentPos, newEnemyPos, _worldInfo); //siguiente estado después de mover a ambos
            float reward = CalculateReward(newAgentPos, newEnemyPos); //calcula la rescompensa obtenida por este paso

            totalReward += reward;
            Return = Mathf.Round(totalReward * 10f) / 10f;

            if (train) //si estamo entrenando no solo observando aplicaremos la formula de q-learning
            {
                UpdateQtable(state, action, reward, nextState);
            }

            AgentPosition = newAgentPos; //actualizamos las posiciones para el siguiente paso
            OtherPosition = newEnemyPos;
            _stepCount++;
            CurrentStep = _stepCount;

            Debug.Log($"[Ep {CurrentEpisode}] Step {CurrentStep} | Action: {action} | R: {reward} | Epsilon: {Math.Round(epsilon, 3)}");
        }

        private void ResetEnvironment()
        {
            // Colocar agente y enemigo en celdas transitables alejadas
            do { AgentPosition = _worldInfo.RandomCell(); } while (!AgentPosition.Walkable);
            do { OtherPosition = _worldInfo.RandomCell(); }
            while (!OtherPosition.Walkable || AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan) < 3);

            terminal_state = false;
            _episodeCount++;
            CurrentEpisode = _episodeCount;
            _stepCount = 0;
            CurrentStep = 0;
            totalReward = 0;

            // Guardar tabla periódicamente
            if (_episodeCount % _qMindTrainerParams.episodesBetweenSaves == 0 || _episodeCount == _qMindTrainerParams.episodes)
            {
                _qTable.Save();
                Debug.Log($"[Ep {_episodeCount}] Tabla Q guardada.");
            }

            // Decaimiento corregido de epsilon desde episodio 501
            if (_episodeCount > 500)
            {
                int decayEpisodes = episodes - 500;
                float decayRate = (epsilonInicial - minEpsilon) / decayEpisodes;
                epsilon = Mathf.Max(minEpsilon, epsilon - decayRate);
            }

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        private int selectAction(State state) //genera un nº (0-1) si es menor o igual que epsilon 
        {
            return (Random.value <= epsilon)
                ? Random.Range(0, _qTable.actions) //elegira una acción aleatoria (explorará)
                : _qTable.GetAction(state); // sino elegirá la mehor acción aprendida (explotación)
        }

        private (CellInfo, CellInfo) UpdateEnvironment(int action)
        {
            CellInfo newAgentPos = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));

            CellInfo[] newOtherPath = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);

            if (newOtherPath == null || newOtherPath.Length == 0 || newOtherPath[0] == null)
            {
                Debug.LogWarning($"[Ep {CurrentEpisode}] SIN CAMINO entre enemigo y agente. Enemigo se queda quieto.");
                return (newAgentPos, OtherPosition);
            }

            return (newAgentPos, newOtherPath[0]);
        }

        private void UpdateQtable(State state, int action, float reward, State nextState)
        {//recibe estado actual, acción que se tomó desde el estado, recompensa recibida y estado siguiente al que se llega
            float actualQ = _qTable.GetQValue(state, action);
            float maxNextQ = _qTable.GetMaxQValue(nextState);

            // Q(s,a) <-- (1 - ?)    *  Q(s,a) +   ?   * [ R     +  ?    * max Q(s',a')]
            float newQ = (1 - alpha) * actualQ + alpha * (reward + gamma * maxNextQ);
            _qTable.UpdateQValue(state, action, newQ); //guardar nuevo valor
        }

        private float CalculateReward(CellInfo newAgentPos, CellInfo newEnemyPos)
        {
            // Si se mete en un muro o es capturado → final inmediato
            if (!newAgentPos.Walkable || (newAgentPos.x == newEnemyPos.x && newAgentPos.y == newEnemyPos.y))
            {
                terminal_state = true;
                return -1000f;
            }

            // Calcula si el agente se aleja o acerca
            float oldDistance = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
            float newDistance = newAgentPos.Distance(newEnemyPos, CellInfo.DistanceType.Manhattan);

            float reward = 0f;

            if (newDistance > oldDistance)
                reward += 10f; // Se aleja → positivo
            else if (newDistance < oldDistance)
                reward -= 10f; // Se acerca → negativo

            // NO SE APLICA penalización por cada paso
            //reward -= 1f; 

            return reward;
        }


        private void Update()
        {
            // Entrenamiento rápido sin depender de "train"
            if (_qMindTrainerParams != null && Application.isPlaying)
            {
                int stepsPerFrame = 100;
                for (int i = 0; i < stepsPerFrame; i++)
                {
                    DoStep(true);
                }
            }

            // Guardado manual con tecla S
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