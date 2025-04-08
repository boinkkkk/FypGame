  using UnityEngine;
  using UnityEngine.SceneManagement;
  using Unity.Services.Authentication;
  using System.Collections;

  public class BackToMenu : MonoBehaviour
  {
      // Method to load a scene by name
    //   public void LoadSceneByName(string sceneName)
    //   {
    //       SceneManager.LoadScene(sceneName);
    //   }

    //   // Method to load a scene by build index
    //   public void LoadSceneByIndex(int sceneIndex)
    //   {
    //       SceneManager.LoadScene(sceneIndex);
    //   }




    public void SignOutAndChangeScene(string sceneName)
    {
        // Sign out the user
        AuthenticationService.Instance.SignOut(true);
        Debug.Log("User signed out successfully.");

        // Load the specified scene
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null; // Wait until the scene is fully loaded
        }
        Debug.Log($"Scene '{sceneName}' loaded successfully.");
    }
}

