using System;
using System.Security.Authentication;
using Mirror.SimpleWeb;
#region Data Structures

[Serializable]
public struct WebSocketTransportConfig
{
    public ushort port;
    public bool secure;
    public SslProtocols sslProtocols;
    public string sslCertJson;
    public int maxMessageSize;
    public int maxHandshakeSize;
    public int serverMaxMsgsPerTick;
    public int clientMaxMsgsPerTick;
    public int sendTimeout;
    public int receiveTimeout;
    public bool noDelay;
    public bool batchSend;
    public bool waitBeforeSend;

    public static WebSocketTransportConfig Default => new()
    {
        port = 8888,
        secure = false,
        sslProtocols = SslProtocols.Tls12,
        sslCertJson = "./cert.json",
        maxMessageSize = 16384,
        maxHandshakeSize = 16384,
        serverMaxMsgsPerTick = 10000,
        clientMaxMsgsPerTick = 1000,
        sendTimeout = 5000,
        receiveTimeout = 20000,
        noDelay = true,
        batchSend = false,
        waitBeforeSend = false
    };

    public void ApplyTo(SimpleWebTransport transport)
    {
        if (transport == null) return;
        transport.port = port;
        transport.sslEnabled = secure;
        transport.sslProtocols = sslProtocols;
        transport.sslCertJson = sslCertJson;
        transport.maxMessageSize = maxMessageSize;
        transport.maxHandshakeSize = maxHandshakeSize;
        transport.serverMaxMsgsPerTick = serverMaxMsgsPerTick;
        transport.clientMaxMsgsPerTick = clientMaxMsgsPerTick;
        transport.sendTimeout = sendTimeout;
        transport.receiveTimeout = receiveTimeout;
        transport.noDelay = noDelay;
        transport.batchSend = batchSend;
        transport.waitBeforeSend = waitBeforeSend;
    }
}

#endregion
