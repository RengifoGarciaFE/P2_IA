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
        public int actions;
        public Dictionary<string, float[]> qTable;

        public QTable()
        {
            actions = 4;
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
                    string idState = parts[0];
                    float[] qValues = new float[actions];

                    for (int i = 0; i < actions; i++)
                    {
                        qValues[i] = float.Parse(parts[i + 1]);
                    }

                    qTable[idState] = qValues;
                }

                Debug.Log("Tabla Q cargada correctamente desde Assets/Scripts/GrupoB/TablaQ.csv");
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
                string stateString = entry.Key;
                string qValuesString = string.Join("|", entry.Value);
                lines.Add($"{stateString}|{qValuesString}");
            }
            return lines;
        }

        public int GetAction(State state)
        {
            if (!qTable.ContainsKey(state.idState))
            {
                qTable[state.idState] = new float[actions];
            }

            float[] qValues = qTable[state.idState];
            float maxValue = qValues[0];
            int action = 0;

            for (int i = 1; i < qValues.Length; i++)
            {
                if (qValues[i] > maxValue)
                {
                    maxValue = qValues[i];
                    action = i;
                }
            }
            return action;
        }

        public float GetQValue(State state, int action)
        {
            if (!qTable.ContainsKey(state.idState))
            {
                qTable[state.idState] = new float[actions];
            }

            float[] qValues = qTable[state.idState];
            // Debug.Log($"Obteniendo QValue para el estado {state.idState} y acción {action}.");
            return qValues[action];
        }

        public float GetMaxQValue(State state)
        {
            if (!qTable.ContainsKey(state.idState))
            {
                qTable[state.idState] = new float[actions];
            }

            float[] qValues = qTable[state.idState];
            float maxValue = qValues[0];

            for (int i = 1; i < qValues.Length; i++)
            {
                if (qValues[i] > maxValue)
                {
                    maxValue = qValues[i];
                }
            }

            return maxValue;
        }

        public void UpdateQValue(State state, int action, float newValue)
        {
            if (!qTable.ContainsKey(state.idState))
            {
                qTable[state.idState] = new float[actions];
            }

            float[] qValues = qTable[state.idState];
            qValues[action] = newValue;
            qTable[state.idState] = qValues;

            // Debug.Log($"Actualizando QValue para {state.idState}, acción {action}: {newValue}");
        }
    }
}
