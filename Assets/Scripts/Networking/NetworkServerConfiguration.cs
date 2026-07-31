
namespace Unity.Netcode
{
    public enum ConnectMode
    {
        LOCAL,
        DEVELOPMENT_REMOTE,
        PRODUCTION_REMOTE
    }
    
    public class NetworkServerConfiguration : MonoBehaviourSingleton<NetworkServerConfiguration>
    {
        public ConnectMode connectMode = ConnectMode.PRODUCTION_REMOTE;
        public string remoteIP = "aulamac08.uv.es";
        public string localIP = "127.0.0.1";
        
        public string GetServerIP()
        {
            if(connectMode == ConnectMode.LOCAL)
                return localIP;
            else
                return remoteIP;
        }

        public string GetApiPrefix()
        {
            if (connectMode == ConnectMode.PRODUCTION_REMOTE)
                return "/roommanager";
            else
                return "/roommanagerdev";
        }
    }
}