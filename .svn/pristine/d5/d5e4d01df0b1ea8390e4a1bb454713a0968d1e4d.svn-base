using Unity.Netcode;

public class MuteTool : ToolComponent
{
    protected override void OnToolActivatedInternal()
    {
        base.OnToolActivatedInternal();
        AudioManager.Instance.SetMuteState(true);
        UpdateNetworkMute(true);
    }

    protected override void OnToolDeactivatedInternal()
    {
        base.OnToolDeactivatedInternal();
        AudioManager.Instance.SetMuteState(false);
        UpdateNetworkMute(false);
    }
    
    /// <summary>
    /// Busca el NetworkAudio del jugador local y actualiza su estado.
    /// </summary>
    private void UpdateNetworkMute(bool isMuted)
    {
        // Nos aseguramos de que Netcode está corriendo y tenemos un jugador local
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (localPlayer != null)
            {
                var networkAudio = localPlayer.GetComponent<NetworkAudio>();
                if (networkAudio != null)
                {
                    networkAudio.SetMute(isMuted);
                }
            }
        }
    }
}