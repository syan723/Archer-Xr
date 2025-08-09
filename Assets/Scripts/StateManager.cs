using UnityEngine;
using UnityEngine.SceneManagement;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance;
    public SessionInfo sessionInfo;
    public static string baseUrl = "https://xrarchy.vercel.app/api/mobile/";
    public Project currentProject;
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
        if (SceneManager.GetActiveScene().buildIndex != 0) return;
        SceneManager.LoadScene(1);
    }
}
