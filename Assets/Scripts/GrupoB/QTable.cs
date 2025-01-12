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

                // Limpiar el diccionario actual
                qTable.Clear();

                // Iterar cada por cada linea
                foreach (string line in lines)
                {
                    // Separar la linea 
                    string[] parts = line.Split(',');

                    //idState
                    string idState = parts[0];

                    // Las siguientes partes son los valores Q
                    float[] qValues = new float[actions];
                    for (int i = 0; i < actions; i++)
                    {
                        qValues[i] = float.Parse(parts[i + 1]);
                    }

                    // Agregar la clave y los valores al diccionario
                    qTable[idState] = qValues;
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
                string qValuesString = string.Join(",", qValues);//cambio del separadaor para ver bien los valores de los numeros

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
            /*foreach(var value in qTable)
            {
                Debug.Log($"Clave: {value.Key}, Valor: {value.Value}");
            }*/
            return action;
        }

        public float GetQValue(State state, int action)
        {
            if (!qTable.ContainsKey(state.idState))//si el estado no esta en la tabla se inicializa a 0
            {
                qTable[state.idState] = new float[actions];
            }

            float[] qValues = qTable[state.idState];//cogemos valor de la accion del estado indicado
            Debug.Log($"Obteniendo QValue para el estado {state.idState} y acción {action}. Total de acciones: {qValues.Length}");
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
            Debug.Log($"Actualizando QValue para el estado {state.idState}, acción {action}. Total de acciones: {qValues.Length}");
            qValues[action] = newValue;
            qTable[state.idState] = qValues;
        }
    }
}

