using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

/// <summary>
/// Controlador de la interfaz de usuario para la visualización y gestión de salas.
/// Permite mostrar la lista de salas y gestionar interacciones de usuario.
/// </summary>
public class RoomsUIController : MonoBehaviour
{
    /// <summary>
    /// Referencia a la lista de salas mostradas en la UI.
    /// </summary>
    public object roomsListUI;

    public Transform UIParent;
    public GameObject RoomUIPrefab;
    public TextMeshProUGUI CHElementNameText;
        
    public UnityEvent<Room> OnRoomSelected;

    private void Start()
    {
        // ...existing code...
    }

    public void SetCHElementName(string name)
    {
        if (CHElementNameText != null)
            CHElementNameText.text = name;
        else
            Debug.LogWarning("CHElementNameText is not assigned in the inspector.");
    }
    public void AddRoom(string roomname, string creatorname, int puerto, string ip, int chelementID)
    {
        GameObject go = Instantiate(RoomUIPrefab, UIParent);
        RoomUIController roomUIController = go.GetComponent<RoomUIController>();
        roomUIController.SetArtifactName(roomname);
        roomUIController.SetCreatorName(creatorname);
        Room room = go.GetComponent<Room>();
        room.setIP(ip);
        room.setPort((ushort)puerto);
        
        go.GetComponent<Button>().onClick.AddListener(() =>
        {
            OnRoomSelected.Invoke(room);
        });
    }

    private void OnDisable()
    {
    }

    private void OnEnable()
    {
    }

    public void ClearRooms()
    {
        for(int i = UIParent.childCount - 1; i >= 0; i--)
            Destroy(UIParent.GetChild(i).gameObject);
    }
}
