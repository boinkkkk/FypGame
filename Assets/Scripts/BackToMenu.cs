  using UnityEngine;
  using UnityEngine.SceneManagement;

  public class BackToMenu : MonoBehaviour
  {
      // Method to load a scene by name
      public void LoadSceneByName(string sceneName)
      {
          SceneManager.LoadScene(sceneName);
      }

      // Method to load a scene by build index
      public void LoadSceneByIndex(int sceneIndex)
      {
          SceneManager.LoadScene(sceneIndex);
      }
  }
