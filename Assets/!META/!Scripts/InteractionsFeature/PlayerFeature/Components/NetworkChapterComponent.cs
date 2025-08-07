using Mirror;

public struct NetworkChapterComponent
{
    public NetworkConnectionToClient connection;
    public uint ID;

    public NetworkChapterComponent(uint iD, NetworkConnectionToClient connectionToClient)
    {
        ID = iD;
        connection = connectionToClient;
    }
}
