using NavigationDJIA.World;
using QMind.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GrupoB
{
    public class QTable : MonoBehaviour
    {
        public int actions = 4; // N, E, S, O
        public Dictionary<string, float[]> qTable;

        public QTable()
        {
            qTable = new Dictionary<string, float[]>();
        }

        public void Save()
        {
            string filePath = @"Assets/Scripts/GrupoB/TablaQ.csv";
            File.WriteAllLines(filePath, ConvertCsv(qTable));
        }

        public void Load()
        {
            try
            {
                string[] lines = File.ReadAllLines(@"Assets/Scripts/GrupoB/TablaQ.csv");
                qTable.Clear();

                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == actions + 1)
                    {
                        string idState = parts[0];
                        float[] qValues = new float[actions];
                        for (int i = 0; i < actions; i++)
                        {
                            qValues[i] = float.Parse(parts[i + 1].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                        }
                        qTable[idState] = qValues;
                    }
                }
                Debug.Log("Tabla Q cargada correctamente.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error al cargar la tabla Q: {ex.Message}");
            }
        }

        private IEnumerable<string> ConvertCsv(Dictionary<string, float[]> qTable)
        {
            List<string> lines = new List<string>();
            foreach (var entry in qTable)
            {
                string qValuesString = string.Join("|", entry.Value);
                lines.Add($"{entry.Key}|{qValuesString}");
            }
            return lines;
        }

        public int GetAction(State state)
        {
            EnsureStateExists(state.idState);
            float[] qValues = qTable[state.idState];

            int bestAction = 0;
            float bestQ = qValues[0];
            for (int i = 1; i < qValues.Length; i++)
            {
                if (qValues[i] > bestQ)
                {
                    bestQ = qValues[i];
                    bestAction = i;
                }
            }
            return bestAction;
        }

        public float GetQValue(State state, int action)
        {
            EnsureStateExists(state.idState);
            return qTable[state.idState][action];
        }

        public float GetMaxQValue(State state)
        {
            EnsureStateExists(state.idState);
            float[] qValues = qTable[state.idState];
            float maxQ = qValues[0];
            for (int i = 1; i < qValues.Length; i++)
            {
                if (qValues[i] > maxQ)
                    maxQ = qValues[i];
            }
            return maxQ;
        }

        public void UpdateQValue(State state, int action, float newValue)
        {
            EnsureStateExists(state.idState);
            qTable[state.idState][action] = newValue;
        }

        private void EnsureStateExists(string idState)
        {
            if (!qTable.ContainsKey(idState))
            {
                qTable[idState] = new float[actions]; // inicializar con ceros
            }
        }
    }
}