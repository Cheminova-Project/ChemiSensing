using System;
using UnityEngine;

using System.Linq;
[Serializable]
/// <summary>
/// Tipos de personajes de jugador soportados.
/// </summary>
public enum PlayerCharacterType : int
{
    Desktop = 1,
    VR = 2,
    Mobile = 3,
    AR = 4,
    SERVER = 5,
    None = 0
}

namespace Unity.Netcode
{
    /// <summary>
    /// Controlador de plataforma que determina el tipo de personaje del jugador
    /// basado en la configuración de compilación.
    /// Utiliza un singleton para acceso global.
    /// </summary>
    public class PlatformController : MonoBehaviourSingleton<PlatformController>
    {
        [SerializeField] PlayerCharacterType m_PlayerCharacterType;

        protected override void SingletonAwakened()
        {
            // Cargar configuración de compilación
            LoadBuildType();
        }


        private void LoadBuildType()
        {
            BuildConfig config = Resources.Load<BuildConfig>("BuildSettings");
            if (config != null)
            {
                m_PlayerCharacterType = BuildTypeToCharacterType(config.CurrentBuildType);
                //Debug.Log($"CharacterType cargado: {m_PlayerCharacterType}");
            }
            else
            {
                Debug.LogError("No se encontró el BuildConfig en Resources.");
            }
        }

        private PlayerCharacterType BuildTypeToCharacterType(BuildType buildType)
        {
            PlayerCharacterType target = buildType switch
            {
                BuildType.Desktop => PlayerCharacterType.Desktop,
                BuildType.VR_META => PlayerCharacterType.VR,
                BuildType.VR_PICO => PlayerCharacterType.VR,
                BuildType.MobileiOS => PlayerCharacterType.Mobile,
                BuildType.MobileAndroid => PlayerCharacterType.Mobile,
                BuildType.ARiOS => PlayerCharacterType.AR,
                BuildType.ARAndroid => PlayerCharacterType.AR,
                BuildType.WindowsServer => PlayerCharacterType.SERVER,
                BuildType.LinuxServer => PlayerCharacterType.SERVER,
                BuildType.LinuxClient => PlayerCharacterType.Desktop,
                BuildType.MacOS => PlayerCharacterType.Desktop,
                BuildType.UWP => PlayerCharacterType.Desktop,
                _ => PlayerCharacterType.None
            };

            return target;
        }
        /// <summary>
        /// Establece el tipo de personaje del jugador.
        /// </summary>
        public void SetPlayerCharacterType(PlayerCharacterType playerCharacterType)
        {
            m_PlayerCharacterType = playerCharacterType;
        }
        /// <summary>
        /// Obtiene el tipo de personaje del jugador.
        /// </summary>
        public PlayerCharacterType GetPlayerCharacterType()
        {
            //Copmprobaremos si estamos en debug
            bool isPCClient = Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Contains("Test_PC");
            bool isVRClient = Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Contains("Test_VR");
            bool isARClient = Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Contains("Test_AR");
            bool isMobileClient = Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Contains("Test_Mobile");

            if (isPCClient)
                return PlayerCharacterType.Desktop;
            
            if (isVRClient)
                return PlayerCharacterType.VR;
            
            if (isARClient)
                return PlayerCharacterType.AR;
            
            if (isMobileClient)
                return PlayerCharacterType.Mobile;

            return m_PlayerCharacterType;
        }
    }
}