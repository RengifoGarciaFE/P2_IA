using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


namespace GrupoB
{
    public class QTable : MonoBehaviour
    {
        public int actions;//numero de columnas (una por accion), filas son los estados
        public Dictionary<State, float[]> qTable;
        //(mirar hacer un id de state en vez de poner todo el state)*******************
        public QTable()
        {
            actions = 4;
            qTable= new Dictionary<State, float[]>();

        }

        public void Save()
        {
            File.WriteAllLines(@"Assets/Scripts/GrupoB/TablaQ.csv",
                ConvertCsv(qTable));
        }

        public void Load()
        {
            try
            {
                string[] lines = File.ReadAllLines(@"Assets/Scripts/GrupoB/TablaQ.csv");

                // Limpiar el diccionario actual (si es necesario)
                qTable.Clear();

                // Saltar la primera l�nea si contiene encabezados
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string[] parts = line.Split(',');

                    // Parsear el estado (las primeras columnas)
                    State state = new State
                    {
                        nWall = bool.Parse(parts[0]),
                        sWall = bool.Parse(parts[1]),
                        eWall = bool.Parse(parts[2]),
                        wWall = bool.Parse(parts[3]),
                        nEnemy = bool.Parse(parts[4]),
                        eEnemy = bool.Parse(parts[5]),
                        enemyNear = bool.Parse(parts[6])
                    };

                    // Parsear los valores Q (las columnas restantes)
                    float[] qValues = new float[actions];
                    for (int j = 0; j < actions; j++)
                    {
                        qValues[j] = float.Parse(parts[9 + j]);
                    }

                    // Agregar el estado y sus valores Q al diccionario
                    qTable[state] = qValues;
                }

                Debug.Log($"Tabla Q cargada correctamente desde Assets/Scripts/GrupoB/TablaQ.csv");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error al cargar la tabla Q: {ex.Message}");
            }
        }

        private IEnumerable<string> ConvertCsv(Dictionary<State, float[]> qTable)
        {
            List<string> lines = new List<string>();

            // Iterar sobre cada entrada en la tabla Q
            foreach (var entry in qTable)
            {
                State state = entry.Key;
                float[] qValues = entry.Value;

                // Convertir el estado a una cadena CSV
                string stateString = $"{state.nWall},{state.sWall},{state.eWall},{state.wWall}," +
                                     $"{state.nEnemy},{state.eEnemy}," +
                                     $"{state.enemyNear}";

                // Convertir los valores Q a una cadena CSV
                string qValuesString = string.Join(",", qValues);

                // Combinar el estado y los valores Q en una l�nea
                lines.Add($"{stateString},{qValuesString}");
            }

            return lines;
        }

        public int GetAction(State state)
        {
            if (!qTable.ContainsKey(state))//si el estado no esta en la tabla se inicializa a 0
            {
                qTable[state] = new float[actions];
            }

            float[] qValues = qTable[state];
            float maxValue = qValues[0];
            int action = 0;

            for (int i = 1; i < qValues.Length;i++)//recorremos la lista de qValues de las acciones en busca de la mejor
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
            return 0;
        }

        public float GetMaxQValue(State state)
        {
            return 0;
        }

        public void UpdateQValue(State state, int action, float newValue)
        {

        }
    }
}

