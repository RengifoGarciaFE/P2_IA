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
        //Número de posibles acciones (N, E, S, O)
        public int actions = 4; 
        //Diccionario que asocia estados a valores Q por acción
        public Dictionary<string, float[]> qTable;

        //Constructor
        public QTable()
        {
            qTable = new Dictionary<string, float[]>();
        }

        //Guarda el contenido de la tabla Q en un archivo CSV
        public void Save()
        {
            string filePath = @"Assets/Scripts/GrupoB/TablaQ.csv";
            File.WriteAllLines(filePath, ConvertCsv(qTable));
        }

        //Carga la tabla Q desde un archivo CSV
        public void Load()
        {
            try
            {
                string[] lines = File.ReadAllLines(@"Assets/Scripts/GrupoB/TablaQ.csv");
                qTable.Clear();//limpia la tabla antes de cargar

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
                        qTable[idState] = qValues;//asigna valores Q al estado
                    }
                }
                Debug.Log("Tabla Q cargada correctamente.");
            }
            catch (Exception ex)
            {
                //Se muestra un error si algo falla al cargar
                Debug.LogError($"Error al cargar la tabla Q: {ex.Message}");
            }
        }

        //Convierte el diccionario Q a formato CSV en forma de lista de strings
        private IEnumerable<string> ConvertCsv(Dictionary<string, float[]> qTable)
        {
            List<string> lines = new List<string>();
            foreach (var entry in qTable)
            {
                
                string qValuesString = string.Join("|", entry.Value);
                lines.Add($"{entry.Key}|{qValuesString}");
            }
            //devuelve un string con este formato: estado|q1|q2|q3|q4
            return lines;
        }

        //Devuelve la mejor acción para un estado dado según los valores Q
        public int GetAction(State state)
        {
            EnsureStateExists(state.idState);//Verifica que el estado está en la tabla
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
            return bestAction;//Devuelve el índice de la mejor acción
        }

        //Devuelve el valor Q de un estado específico para una acción determinada
        public float GetQValue(State state, int action)
        {
            EnsureStateExists(state.idState);
            return qTable[state.idState][action];
        }

        //Devuelve el valor Q máximo para un estado dado
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

        //Actualiza el valor Q para un estado y acción específica
        public void UpdateQValue(State state, int action, float newValue)
        {
            EnsureStateExists(state.idState);
            qTable[state.idState][action] = newValue;
        }

        // Si el estado no existe en la tabla Q, lo inicializa con valores Q = 0
        private void EnsureStateExists(string idState)
        {
            if (!qTable.ContainsKey(idState))
            {
                qTable[idState] = new float[actions]; // inicializar con ceros
            }
        }
    }
}