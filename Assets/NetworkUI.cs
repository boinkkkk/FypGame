using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
   private void OnGUI()
    {
        if (GUILayout.Button("Host"))
        {
            NetworkManager.Singleton.StartHost();
        }
        
        if (GUILayout.Button("Client"))
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
