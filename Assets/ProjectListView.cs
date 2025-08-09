using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProjectListView : MonoBehaviour
{
    public string url;
    public GameObject prefab;
    public List<GameObject> slots;
    private void OnEnable()
    {
        StartCoroutine(Fetch());
    }
    IEnumerator Fetch()
    {
        string u = StateManager.baseUrl + url;
        Debug.LogError(u);
        using (UnityWebRequest webRequest = new UnityWebRequest(u, "GET"))
        {
            // Set the content type header
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Add the Authorization header with the access token
            if (StateManager.Instance.sessionInfo != null && !string.IsNullOrEmpty(StateManager.Instance.sessionInfo.accessToken))
            {
                webRequest.SetRequestHeader("Authorization", "Bearer " + StateManager.Instance.sessionInfo.accessToken);
            }
            else
            {
                Debug.LogWarning("Access token not found!");
                // You might want to handle the missing token case here
            }

            // Set the download handler to receive the response
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            // Handle the response
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Network Error: " + webRequest.error);
            }
            else
            {
                if (webRequest.responseCode == 200)
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    Debug.Log("Projects fetched successfully: " + jsonResponse);

                    // Deserialize
                    foreach (var item in slots.ToList())
                    {
                        Destroy(item);
                    }
                    ProjectListResponse response = JsonUtility.FromJson<ProjectListResponse>(jsonResponse);

                    foreach (Project project in response.data.projects)
                    {
                        Debug.Log("Project: " + project.name + ", Created by: " + project.createdBy.email);

                        // Example: instantiate a slot prefab and fill in details (pseudo-code)
                        GameObject slot = Instantiate(prefab, prefab.transform.parent); // assuming parent is set properly
                        slot.SetActive(true);
                        slot.GetComponent<Button>().onClick.AddListener(() => StateManager.Instance.currentProject = project);
                        slots.Add(slot);

                        // Assuming the prefab has a script like ProjectSlotView.cs
                        slot.GetComponentInChildren<TextMeshProUGUI>().text = project.name;
                    }
                }
                else
                {
                    Debug.LogError("API Error: " + webRequest.responseCode + " " + webRequest.downloadHandler.text);
                }
            }
        }

    }
}

[System.Serializable]
public class ProjectListResponse
{
    public bool success;
    public ProjectListData data;
}

[System.Serializable]
public class ProjectListData
{
    public List<Project> projects;
    public string userRole;
    public int totalCount;
}

[System.Serializable]
public class Project
{
    public string id;
    public string name;
    public string description;
    public string createdAt;
    public string updatedAt;
    public CreatedBy createdBy;
    public ProjectStats stats;
}

[System.Serializable]
public class CreatedBy
{
    public string email;
}

[System.Serializable]
public class ProjectStats
{
    public int assignmentCount;
    public int fileCount;
}

