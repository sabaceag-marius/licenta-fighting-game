
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core.UI
{
    public class UIDesyncManager : MonoBehaviour
    {
        void Start()
        {
            foreach (var remotePlayer in FindObjectsOfType<PlayerHandlerBase>())
            {
                Destroy(remotePlayer.gameObject);
            }
        }

        public void HandleMainMenu()
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}