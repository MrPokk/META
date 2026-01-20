using System;
using kcp2k;
#region Data Structures

[Serializable]
public struct KcpTransportConfig
{
    public ushort port;
    public bool dualMode;
    public bool noDelay;
    public uint interval;
    public int timeout;
    public int recvBufferSize;
    public int sendBufferSize;
    public int fastResend;
    public uint receiveWindowSize;
    public uint sendWindowSize;
    public uint maxRetransmit;
    public bool maximizeSocketBuffers;
    public int reliableMaxMessageSize;
    public int unreliableMaxMessageSize;

    public static KcpTransportConfig Default => new()
    {
        port = 7777,
        dualMode = true,
        noDelay = true,
        interval = 10,
        timeout = 10000,
        recvBufferSize = 7361536,
        sendBufferSize = 7361536,
        fastResend = 2,
        receiveWindowSize = 4096,
        sendWindowSize = 4096,
        maxRetransmit = 40,
        maximizeSocketBuffers = true,
        reliableMaxMessageSize = 297433,
        unreliableMaxMessageSize = 1194
    };

    public void ApplyTo(KcpTransport kcp)
    {
        if (kcp == null) return;
        kcp.Port = port;
        kcp.DualMode = dualMode;
        kcp.NoDelay = noDelay;
        kcp.Interval = interval;
        kcp.Timeout = timeout;
        kcp.RecvBufferSize = recvBufferSize;
        kcp.SendBufferSize = sendBufferSize;
        kcp.FastResend = fastResend;
        kcp.ReceiveWindowSize = receiveWindowSize;
        kcp.SendWindowSize = sendWindowSize;
        kcp.MaxRetransmit = maxRetransmit;
        kcp.MaximizeSocketBuffers = maximizeSocketBuffers;
        kcp.ReliableMaxMessageSize = reliableMaxMessageSize;
        kcp.UnreliableMaxMessageSize = unreliableMaxMessageSize;
    }
}

#endregion
