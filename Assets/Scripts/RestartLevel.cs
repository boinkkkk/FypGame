using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class RestartLevel : NetworkBehaviour
{
    public void RestartLevel1() 
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Level1", LoadSceneMode.Single);
        
    }
}
