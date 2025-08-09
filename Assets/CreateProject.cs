using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Net;

[System.Serializable]
public class CreateProjectRequest
{
    public string name;
    public string description;
}

public class CreateProject : MonoBehaviour
{
    public TMP_InputField projectName, projectDescription;
    public Button create;
    public TMP_Text statusText; // Optional: Add a UI text field to show messages

    private void OnEnable()
    {
        projectName.text = "";
        projectDescription.text = "";

        // Attach listener
        create.onClick.RemoveAllListeners();
        create.onClick.AddListener(() => StartCoroutine(Create()));
    }

    IEnumerator Create()
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(projectName.text) || string.IsNullOrWhiteSpace(projectDescription.text))
        {
            if (statusText != null)
                statusText.text = "Please fill in both fields.";
            yield break;
        }

        string url = StateManager.baseUrl + "projects/";
        Debug.LogError(url);

        CreateProjectRequest requestData = new CreateProjectRequest
        {
            name = projectName.text,
            description = projectDescription.text
        };

        string jsonBody = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + StateManager.Instance.sessionInfo.accessToken);


            yield return request.SendWebRequest();
            Debug.LogError(request.responseCode);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Create Project Error: " + request.error);
                if (statusText != null)
                    statusText.text = "Network Error: " + request.error;
            }
            else if (request.responseCode == 201)
            {
                Debug.Log("Project created successfully!");
                if (statusText != null)
                    statusText.text = "Project created successfully!";

                // Optional: Refresh project list or switch panel
                // Example: SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                Debug.LogError("API Error: " + request.downloadHandler.text);
                if (statusText != null)
                    statusText.text = "API Error: " + request.downloadHandler.text;
            }
        }
    }
}
