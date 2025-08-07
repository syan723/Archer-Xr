using UnityEngine;
using UnityEngine.SceneManagement;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance;
    public SessionInfo sessionInfo;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
        string i = PlayerPrefs.GetString("Session Info", "");
        if (string.IsNullOrEmpty(i)) return;
        sessionInfo = JsonUtility.FromJson<SessionInfo>(i);
        SceneManager.LoadScene(1);
    }
}
