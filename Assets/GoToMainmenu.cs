using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoToMainmenu : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(1));
    }
}
