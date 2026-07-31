using System.Collections.Generic;
using UnityEngine;

public class RoomColorManager : MonoBehaviour
{
    public static RoomColorManager Instance { get; private set; }

    [Header("Colour Palette")]
    public List<Color> availableColors;

    private void Awake()
    {
        // Configuración básica de Singleton
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    // El Servidor llama a esto para obtener un color aleatorio que nadie más tiene
    public Color GetUniqueRandomColor()
    {
        if (availableColors == null || availableColors.Count == 0)
        {
            Debug.LogWarning("[RoomColorManager] ¡No quedan colores disponibles! Asignando blanco por defecto.");
            return Color.white;
        }

        int randomIndex = Random.Range(0, availableColors.Count);
        Color chosenColor = availableColors[randomIndex];
        
        // Lo quitamos de la lista para que no se repita
        availableColors.RemoveAt(randomIndex); 
        
        return chosenColor;
    }

    // Cuando un jugador sale de la sala, devuelve su color a la lista
    public void ReturnColor(Color colorToReturn)
    {
        if (colorToReturn != Color.white && !availableColors.Contains(colorToReturn))
        {
            availableColors.Add(colorToReturn);
        }
    }
}