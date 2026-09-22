using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton que almacena variables globales de la aplicación.
/// Mantiene datos de modelos 3D e instancias seleccionadas durante la sesión.
/// </summary>
public class GlobalVariables : MonoBehaviourSingleton<GlobalVariables>
{
    /// <summary>
    /// Datos del CHElement actualmente seleccionado.
    /// </summary>
    private CHElementData _selectedCHElementData;
    /// <summary>
    /// Datos del modelo E3D actualmente seleccionado.
    /// </summary>
    private E3DModelData _selectedE3DModelData;
    /// <summary>
    /// Datos de la instancia 3D actualmente seleccionada.
    /// </summary>
    private ThreeDInstanceData _selected3DInstanceData;

    /// <summary>
    /// Flag para incluir audio en la sala colaborativa.
    /// </summary>
    private bool _isAudioRoom;
    
    /// <summary>
    /// Nombre de la sala creada.
    /// </summary>
    private string _roomName;
    
    /// <summary>
    /// Listado de usuarios aceptados en la sala.
    /// </summary>
    private List<string> _acceptedUsers = new List<string>();
    
    /// <summary>
    /// Inicialización del singleton.
    /// </summary>
    protected override void SingletonAwakened()
    {
        base.SingletonAwakened();
    }
    
    /// <summary>
    /// Establece el CHElement seleccionado.
    /// </summary>
    /// <param name="chElementData">Datos del CHElement a seleccionar.</param>
    public void SetSelectedCHElementData(CHElementData chElementData)
    {
        _selectedCHElementData = chElementData;
    }
    
    /// <summary>
    /// Obtiene el CHElement actualmente seleccionado.
    /// </summary>
    /// <returns>Datos del CHElement seleccionado.</returns>
    public CHElementData GetSelectedCHElementData()
    {
        return _selectedCHElementData;
    }
    
    /// <summary>
    /// Establece el modelo E3D seleccionado.
    /// </summary>
    /// <param name="e3DModelData">Datos del modelo E3D a seleccionar.</param>
    public void SetSelectedE3DModelData(E3DModelData e3DModelData)
    {
        _selectedE3DModelData = e3DModelData;
    }
    
    /// <summary>
    /// Obtiene el modelo E3D actualmente seleccionado.
    /// </summary>
    /// <returns>Datos del modelo E3D seleccionado.</returns>
    public E3DModelData GetSelectedE3DModelData()
    {
        return _selectedE3DModelData;
    }
    
    /// <summary>
    /// Establece la instancia 3D seleccionada.
    /// </summary>
    /// <param name="threeDInstanceData">Datos de la instancia 3D a seleccionar.</param>
    public void SetSelected3DInstanceData(ThreeDInstanceData threeDInstanceData)
    {
        _selected3DInstanceData = threeDInstanceData;
    }
    
    /// <summary>
    /// Obtiene la instancia 3D actualmente seleccionada.
    /// </summary>
    /// <returns>Datos de la instancia 3D seleccionada.</returns>
    public ThreeDInstanceData GetSelected3DInstanceData()
    {
        return _selected3DInstanceData;
    }
    
    /// <summary>
    /// Establece el flag de audio.
    /// </summary>
    /// <param name="isAudioRoom">Flag de audio a seleccionar.</param>
    public void SetIsAudioRoom(bool isAudioRoom)
    {
        _isAudioRoom = isAudioRoom;
    }
    
    /// <summary>
    /// Obtiene el flag de audio actualmente seleccionado.
    /// </summary>
    /// <returns>Flag de audio.</returns>
    public bool GetIsAudioRoom()
    {
        return _isAudioRoom;
    }
    
    /// <summary>
    /// Establece el nombre de la sala.
    /// </summary>
    /// <param name="roomName">Nombre de la sala a crear.</param>
    public void SetRoomName(string roomName)
    {
        _roomName = roomName;
    }
    
    /// <summary>
    /// Obtiene el nombre de la sala.
    /// </summary>
    /// <returns>Nombre de la sala.</returns>
    public string GetRoomName()
    {
        return _roomName;
    }
    
    public void SetAcceptedUsers(List<string> users)
    {
        _acceptedUsers = users ?? new List<string>();
    }
    
    public List<string> GetAcceptedUsers()
    {
        return _acceptedUsers;
    }
}
