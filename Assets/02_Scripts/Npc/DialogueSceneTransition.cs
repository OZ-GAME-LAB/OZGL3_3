using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueSceneTransition : MonoBehaviour
{
    [SerializeField] private string _sceneName;

    public void LoadScene()
    {
        if (GameManager.UserData.SaveAllUserData())
        {
            SceneManager.LoadScene(_sceneName);
        }
    }
}
