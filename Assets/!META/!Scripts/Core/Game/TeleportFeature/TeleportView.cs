using UnityEngine;

[DisallowMultipleComponent]
public class TeleportView : MonoBehaviour
{
    private TeleportPresenter _teleportPresenter;
    public int floorNumber;
    public float scaleFactor = 1f;


    public void Init(TeleportPresenter teleportPresenter)
    {
        _teleportPresenter = teleportPresenter;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<ITeleported>() != null)
        {
            // _teleportPresenter.UITeleport.ShowScreen();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<ITeleported>() != null)
        {
            // _teleportPresenter.UITeleport.HideScreen();
        }
    }
}

