
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core.UI
{
    public class UIMainMenuManager : MonoBehaviour
    {
        void Start()
        {
            foreach (var remotePlayer in FindObjectsOfType<PlayerHandlerBase>())
            {
                Destroy(remotePlayer.gameObject);
            }
        }

        public void HandleLocalPlay()
        {
            SceneManager.LoadScene("CharacterSelectScene");
        }

        public void HandleJoinLobby()
        {
            SceneManager.LoadScene("JoinLobbyCharacterSelectScene");
        }

        public void HandleCreateLobby()
        {
            SceneManager.LoadScene("CreateLobbyCharacterSelectScene");
        }
    }
}