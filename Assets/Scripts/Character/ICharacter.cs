using Unity.Netcode;

/// <summary>
/// Interfaz que define las propiedades y métodos básicos que debe implementar un personaje en la aplicación.
/// </summary>
internal interface ICharacter
{
    NetworkObject NetworkObject { get; }
}