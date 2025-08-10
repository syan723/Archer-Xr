using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class LocationData
{
    public float latitude;
    public float longitude;
    public string name;
    public string address;
    public string description;

}

[System.Serializable]
public class CreateProjectRequest
{
    public string name;
    public string description;
    public LocationData location;
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
        string url = StateManager.baseUrl + "admin/projects"; // removed trailing slash
        Debug.LogError(url);

        CreateProjectRequest requestData = new CreateProjectRequest
        {
            name = projectName.text,
            description = projectDescription.text,
            location = new LocationData()
            {
                address = "null",
                description = "null",
                latitude = 0,
                longitude = 0,
                name = "null"
            }
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        Debug.Log("Request JSON: " + jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + StateManager.Instance.sessionInfo.accessToken);

            yield return request.SendWebRequest();

            Debug.LogError($"Response Code: {request.responseCode}");
            Debug.LogError($"Response Text: {request.downloadHandler.text}");

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
