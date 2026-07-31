using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Gestiona el estado global del juego sincronizado en red.
/// Permite compartir y actualizar el estado entre clientes y servidor.
/// </summary>
public class NetworkedGameState : NetworkBehaviour
    {
        internal NetworkVariable<int> playersConnected = new NetworkVariable<int>();
        ConnectionManager ConnectionManager => ApplicationEntryPoint.Singleton.ConnectionManager;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (NetworkManager.IsServer)
            {
                ConnectionManager.EventManager.AddListener<MinNumberPlayersConnectedEvent>(OnServerMinNumberPlayersConnected);
                ConnectionManager.EventManager.AddListener<ClientConnectedEvent>(OnServerClientConnected);
                ConnectionManager.EventManager.AddListener<ClientDisconnectedEvent>(OnServerClientDisconnected);
                playersConnected.Value = NetworkManager.ConnectedClientsIds.Count;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.IsServer)
            {
                ConnectionManager.EventManager.RemoveListener<MinNumberPlayersConnectedEvent>(OnServerMinNumberPlayersConnected);
                ConnectionManager.EventManager.RemoveListener<ClientConnectedEvent>(OnServerClientConnected);
                ConnectionManager.EventManager.RemoveListener<ClientDisconnectedEvent>(OnServerClientDisconnected);
            }
        }

        void OnServerMinNumberPlayersConnected(MinNumberPlayersConnectedEvent evt)
        {
  
        }

        void OnServerClientConnected(ClientConnectedEvent evt)
        {
            Debug.Log("[Server] Client connected. Current clients: " + NetworkManager.ConnectedClientsIds.Count);
            playersConnected.Value = NetworkManager.ConnectedClientsIds.Count;
        }

        void OnServerClientDisconnected(ClientDisconnectedEvent evt)
        {
            Debug.Log("[Server] Client disconnected. Remaining clients: " + NetworkManager.ConnectedClientsIds.Count);
            playersConnected.Value = NetworkManager.ConnectedClientsIds.Count;
        }

        /// <summary>
        /// Estado actual del juego.
        /// </summary>
        public string gameState;

        /// <summary>
        /// Actualiza el estado del juego y lo sincroniza en red.
        /// </summary>
        /// <param name="newState">Nuevo estado a establecer.</param>
        public void UpdateGameState(string newState)
        {
            gameState = newState;
            // Lógica de sincronización en red
        }
    }