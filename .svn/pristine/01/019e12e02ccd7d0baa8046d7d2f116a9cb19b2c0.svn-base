using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa una sala dentro de la aplicación, incluyendo sus propiedades y estado.
/// </summary>
public class Room : MonoBehaviour
{
    /// <summary>
    /// Nombre de la sala.
    /// </summary>
    public string roomName;

    private ushort port = 0;
    private string ip = null;
    private string creatorName = "";
    private int chelementID = -1;
    private bool hasAudio = false;
    private List<string> usuariosAceptados = new List<string>();
    private int jugadoresActuales = 0;

    public Room(string roomName, string creatorName, ushort port, string ip, int chelementID, bool hasAudio, List<string> usuariosAceptados, int jugadoresActuales)
    {
        this.roomName = roomName;
        this.creatorName = creatorName;
        this.port = port;
        this.ip = ip;
        this.chelementID = chelementID;
        this.hasAudio = hasAudio;
        this.usuariosAceptados = usuariosAceptados ?? new List<string>();
        this.jugadoresActuales = jugadoresActuales;
    }

    public void setPort(ushort port)
    {
        this.port = port;
    }

    public void setIP(string ip)
    {
        this.ip = ip;
    }

    public ushort getPort()
    {
        return port;
    }

    public string getIP()
    {
        return ip;
    }
    
    public string getRoomName()
    {
        return roomName;
    }
    
    public string getCreatorName()
    {
        return creatorName;
    }
    
    public int getCHElementID()
    {
        return chelementID;
    }

    public void setHasAudio(bool hasAudio)
    {
        this.hasAudio = hasAudio;
    }

    public bool getHasAudio()
    {
        return hasAudio;
    }
    
    public void setUsuariosAceptados(List<string> usuariosAceptados)
    {
        this.usuariosAceptados = usuariosAceptados ?? new List<string>();
    }

    public List<string> getUsuariosAceptados()
    {
        return usuariosAceptados;
    }

    public int getJugadoresActuales()
    {
        return jugadoresActuales;
    }
}