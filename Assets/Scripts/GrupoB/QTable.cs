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
        public Dictionary<string, float[]> qTable;
        //(mirar hacer un id de state en vez de poner todo el state)*******************
        public QTable()
        {
            actions = 4;
            qTable= new Dictionary<string, float[]>();

        }

        public void Save()
        {
            string filePath = @"Assets/Scripts/GrupoB/TablaQ.csv";

            /*// Obtener el directorio del archivo
            string directory = Path.GetDirectoryName(filePath);

            // Verificar si el directorio existe
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory); // Crear el directorio si no existe
            }*/

            // Escribir el archivo
            File.WriteAllLines(filePath, ConvertCsv(qTable));
        }

        public void Load()
        {
            try
            {
                string[] lines = File.ReadAllLines(@"Assets/Scripts/GrupoB/TablaQ.csv");

                // Limpiar el diccionario actual (si es necesario)
                qTable.Clear();

                // Saltar la primera línea si contiene encabezados
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
                    qTable[state.idState] = qValues;
                }

                Debug.Log($"Tabla Q cargada correctamente desde Assets/Scripts/GrupoB/TablaQ.csv");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error al cargar la tabla Q: {ex.Message}");
            }
        }

        private IEnumerable<string> ConvertCsv(Dictionary<string, float[]> qTable)
        {
            List<string> lines = new List<string>();

            // Iterar sobre cada entrada en la tabla Q
            foreach (var entry in qTable)
            {
                //State state = entry.Key;
                float[] qValues = entry.Value;

                // Convertir el estado a una cadena CSV
                /*string stateString = $"{state.nWall},{state.sWall},{state.eWall},{state.wWall}," +
                                     $"{state.nEnemy},{state.eEnemy}," +
                                     $"{state.enemyNear}";*/

                string stateString =entry.Key;

                // Convertir los valores Q a una cadena CSV
                string qValuesString = string.Join(",", qValues);

                // Combinar el estado y los valores Q en una línea
                lines.Add($"{stateString},{qValuesString}");
            }

            return lines;
        }

        public int GetAction(State state)
        {
            if (!qTable.ContainsKey(state.idState))//si el estado no esta en la tabla se inicializa a 0
            {
                qTable[state.idState] = new float[actions];
            }

            float[] qValues = qTable[state.idState];
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
            Debug.Log("///////////////////Diccionario/////////////");
            foreach(var value in qTable)
            {
                Debug.Log($"Clave: {value.Key}, Valor: {value.Value}");
            }
            return action;
        }

        public float GetQValue(State state, int action)
        {
            if (!qTable.ContainsKey(state.idState))//si el estado no esta en la tabla se inicializa a 0
            {
                qTable[state.idState] = new float[actions];
            }

            float[] qValues = qTable[state.idState];//cogemos valor de la accion del estado indicado
            return qValues[action];
        }

        public float GetMaxQValue(State state)
        {
            if (!qTable.ContainsKey(state.idState))//si el estado no esta en la tabla se inicializa a 0
            {
                qTable[state.idState] = new float[actions];
            }
            float[] qValues = qTable[state.idState];
            float maxValue = qValues[0];

            for (int i = 1; i < qValues.Length; i++)//recorremos la lista de qValues de las acciones en busca del mayor
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
            if (!qTable.ContainsKey(state.idState))//si el estado no esta en la tabla se inicializa a 0
            {
                qTable[state.idState] = new float[actions];
            }
            float[] qValues = qTable[state.idState];//actualizar valor de una accion del estado indicado
            qValues[action] = newValue;
            qTable[state.idState] = qValues;
        }
    }
}

