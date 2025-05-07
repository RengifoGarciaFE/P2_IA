using NavigationDJIA.World;
using QMind.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace GrupoB
{
    public class QMindTester : IQMind
    {
        private WorldInfo _worldInfo; //ref al entorno
        private QTable _qTable; //nuestra tablaQ 
        private State currentState; //estado actual del agente

        public void Initialize(WorldInfo worldInfo)
        {
            _worldInfo = worldInfo;
            _qTable = new QTable();

            // Cargar la tabla Q entrenada
            _qTable.Load();
            Debug.Log("[QMindTester] Tabla Q cargada.");
        }

        //Este m�tdo recibir� la posici�n del agente y la del enemigo
        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            // Crear el estado actual
            currentState = new State(currentPosition, otherPosition, _worldInfo);

            // Obtener la mejor acci�n seg�n la tabla Q aprendida
            int action = _qTable.GetAction(currentState);

            // Calcular la nueva posici�n resultante de aplicar la acci�n
            CellInfo newAgentPos = _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(action));

            return newAgentPos; //nueva pos donde deber�a ir el agente en ese paso
        }
    }
}