using System;

[Serializable]
public class NetworkConfigData
{
    public CommonSettings? commonSettings;
    public KcpTransportConfig? kcpConfig;
    public WebSocketTransportConfig? webSocketConfig;
}
