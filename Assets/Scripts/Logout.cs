using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class Logout : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            StartCoroutine(LogoutUser());
        });
    }

    private IEnumerator LogoutUser()
    {
        string baseUrl = StateManager.baseUrl;
        string accessToken = StateManager.Instance.sessionInfo?.accessToken;

        if (!string.IsNullOrEmpty(accessToken))
        {
            using var request = new UnityWebRequest($"{baseUrl}/api/mobile/auth/logout", "POST");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Logout request failed: {request.error}");
            }
            else
            {
                Debug.Log("Logout request successful");
            }
        }

        // Clear local session data
        PlayerPrefs.DeleteAll();
        StateManager.Instance.sessionInfo = null;
        StateManager.Instance.currentProject = null;

        // Redirect to login scene
        SceneManager.LoadScene(0);
    }
}
