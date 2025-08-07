using Mirror;
using UnityEngine;

public class PlayerHandler : NetworkBehaviour
{
    [SerializeField] private GameObject[] allPlayers;
    [SerializeField] private int playerToLoad;

    [Command]
    public void CmdLoadPlayer()
    {
        if (playerToLoad < 0 || playerToLoad >= allPlayers.Length) return;
        RPCLoadPlayerToAllClients(playerToLoad);
    }

    [ClientRpc]
    private void RPCLoadPlayerToAllClients(int aPlayerNum)
    {
        if (isLocalPlayer)
        {
            SpawnPlayer(aPlayerNum);
        }
    }

    private void SpawnPlayer(int aPlayerNum)
    {
        if (allPlayers[aPlayerNum] == null) return;

        GameObject tPlayerToLoad = Instantiate(allPlayers[aPlayerNum], transform);
        tPlayerToLoad.transform.localPosition = Vector3.zero;
        tPlayerToLoad.transform.localEulerAngles = Vector3.zero;

        if (isServer)
        {
            NetworkServer.Spawn(tPlayerToLoad, connectionToClient);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isLocalPlayer) return;

        CmdLoadPlayer();
    }
}
