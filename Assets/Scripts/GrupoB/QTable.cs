using NavigationDJIA.World;
using QMind.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GrupoB
{
    public class QTable : MonoBehaviour //lógica para crear, guardar, cargar y consultar la tabla Q
    {
        public int actions;  //total deacciones posibles (4 norte, este, sur y oeste)
        public Dictionary<string, float[]> qTable; //tablaQ, tendrá los valores q por acción

        public QTable()
        {
            actions = 4;
            qTable = new Dictionary<string, float[]>();
        }

        public void Save()
        {
            string filePath = @"Assets/Scripts/GrupoB/TablaQ.csv"; //guardadr la tabla
            File.WriteAllLines(filePath, ConvertCsv(qTable)); 
        }

        public void Load() //cargar la tabla
        {
            try
            {
                string[] lines = File.ReadAllLines(@"Assets/Scripts/GrupoB/TablaQ.csv"); //lee todas las lineas del csv
                qTable.Clear();

                foreach (string line in lines) //para cada linea
                {
                    string[] parts = line.Split('|'); //separa el estado y sus Q-values por |
                    string idState = parts[0]; //extrael ID que será siempre el primer elem
                    float[] qValues = new float[actions]; //crea un nuevo array para guardar los qvalues del estado

                    for (int i = 0; i < actions; i++)
                    {
                        qValues[i] = float.Parse(parts[i + 1]);//convierte cada string en float
                    }

                    qTable[idState] = qValues;//inserta en el diccionario siendo la clave el id u su valor el array de q-values
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

        public int GetAction(State state) //dado un state usca en la qtable que accion tiene mayor qvalue
        {
            if (!qTable.ContainsKey(state.idState))
            {
                qTable[state.idState] = new float[actions]; //si el estado aún no está en la tabla lo añade con valores iniciales 0 para todas las acciones
            }

            float[] qValues = qTable[state.idState]; //si existe pues recupera el array de qvalues para ese estado
            float maxValue = qValues[0];
            int action = 0;

            for (int i = 1; i < qValues.Length; i++)//recorre el array y busca el indice de la acción con mayor qvalue
            {
                if (qValues[i] > maxValue)
                {
                    maxValue = qValues[i];
                    action = i;
                }
            }
            return action; //devolverá el indice de esa acción
        }

        public float GetQValue(State state, int action) 
        {
            if (!qTable.ContainsKey(state.idState))
            {
                qTable[state.idState] = new float[actions];
            }

            float[] qValues = qTable[state.idState];
            // Debug.Log($"Obteniendo QValue para el estado {state.idState} y acción {action}.");
            return qValues[action];//devolverá el valor para la acción concreta
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

            return maxValue; //devuelve el mayor qvalue del estado
        }

        //-->Recibe el estado la acción y el nuevo valor para esa acción en ese estado
        public void UpdateQValue(State state, int action, float newValue)
        {
            if (!qTable.ContainsKey(state.idState))
            {
                qTable[state.idState] = new float[actions];
            }

            float[] qValues = qTable[state.idState];//array de qvalues para ese estado
            qValues[action] = newValue; //actualizar valor con el nuevo
            qTable[state.idState] = qValues; //gardar en el diccionario

            // Debug.Log($"Actualizando QValue para {state.idState}, acción {action}: {newValue}");
        }
    }
}
