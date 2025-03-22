using UnityEngine;
using Unity.Netcode;
using UnityEngine.Video;

public class VideoSync : NetworkBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject videoPanel; // UI Panel holding the video

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Only the host should start the video
        {
            StartVideoServerRpc();
        }

        
    }

    void Start()
    {
        // Attach event listener for when video ends
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartVideoServerRpc()
    {
        StartVideoClientRpc(NetworkManager.Singleton.ServerTime.Time);
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Working");
        // if (IsServer) // Only the host should trigger hiding the video panel
        // {
            
            HideVideoPanelServerRpc();
        // }
    }

    [ClientRpc]
    private void StartVideoClientRpc(double serverStartTime)
    {
        double timeSinceStart = NetworkManager.Singleton.ServerTime.Time - serverStartTime;
        videoPlayer.time = timeSinceStart; // Sync time across clients
        videoPlayer.Play();
        videoPanel.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HideVideoPanelServerRpc()
    {
        // Debug.Log("Hiding vid panel");
        
        HideVideoPanelClientRpc();
    }

    [ClientRpc]
    private void HideVideoPanelClientRpc()
    {
        Debug.Log("Hiding vid panel");
        videoPanel.SetActive(false);
    }
}
