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
    public GameObject prefab;
    public List<GameObject> slots;
    public bool isExisting;
    public GameObject manageProjectPanel;
    private void OnEnable()
    {
        StartCoroutine(Fetch());
    }
    IEnumerator Fetch()
    {
        string u = StateManager.baseUrl + "mobile/projects/browse";
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
                        if (isExisting)
                        {
                            var buttons = slot.GetComponentsInChildren<Button>();
                            buttons[0].onClick.AddListener(() =>
                            {
                                StateManager.Instance.currentProject = project;
                                gameObject.SetActive(false);
                                manageProjectPanel.SetActive(true);
                            });
                            buttons[1].onClick.AddListener(() =>
                            {
                                StateManager.Instance.currentProject = project;
                                // delete api
                                var p = project;
                                StartCoroutine(DeleteProject(p.id));
                                var s = slot;
                                slots.Remove(slot);
                                Destroy(s.gameObject);

                            });
                        }
                        else
                            slot.GetComponent<Button>().onClick.AddListener(() =>
                            {
                                StateManager.Instance.currentProject = project;
                                SceneManager.LoadScene(2);
                            });
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
    IEnumerator DeleteProject(string projectId)
    {
        string deleteUrl = $"{StateManager.baseUrl}admin/projects/{projectId}";
        Debug.LogError("Deleting project at: " + deleteUrl);

        using (UnityWebRequest deleteRequest = UnityWebRequest.Delete(deleteUrl))
        {
            deleteRequest.SetRequestHeader("Content-Type", "application/json");

            if (StateManager.Instance.sessionInfo != null &&
                !string.IsNullOrEmpty(StateManager.Instance.sessionInfo.accessToken))
            {
                deleteRequest.SetRequestHeader("Authorization", "Bearer " + StateManager.Instance.sessionInfo.accessToken);
            }
            else
            {
                Debug.LogWarning("Access token not found, aborting delete.");
                yield break;
            }

            yield return deleteRequest.SendWebRequest();

            if (deleteRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Delete request failed: " + deleteRequest.error);
            }
            else if (deleteRequest.responseCode == 200 || deleteRequest.responseCode == 204)
            {
                Debug.Log("Project deleted successfully!");
                // Optionally refresh project list
                StartCoroutine(Fetch());
            }
            else
            {
                Debug.LogError($"API Delete Error ({deleteRequest.responseCode}): {deleteRequest.downloadHandler.text}");
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

