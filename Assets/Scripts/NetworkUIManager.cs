using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkUIManager : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;

    private void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
