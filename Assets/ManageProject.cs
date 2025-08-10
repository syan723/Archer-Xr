using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[Serializable]
public class ProjectDetailsRoot
{
    public bool success;
    public ProjectDetailsData data;
}

[Serializable]
public class ProjectDetailsData
{
    public ProjectDetail project;
    public string user_role;
    public ProjectPermissions permissions;
}

[Serializable]
public class ProjectPermissions
{
    public bool canEdit;
    public bool canDelete;
    public bool canViewFiles;
    public bool canUploadFiles;
    public bool canManageUsers;
    public bool isReadOnly;
}

[Serializable]
public class ProjectDetail
{
    public string id;
    public string name;
    public string description;
    public string created_at;
    public string updated_at;
    public string created_by;
    public string created_by_email;
    public List<ProjectAssignment> assignments;
    public List<ProjectFile> files;
    public ProjectStats stats;
}

[Serializable]
public class ProjectAssignment
{
    public string id;
    public string assigned_at;
    public string assigned_by;
    public string assigned_by_email;
    public AssignedUser assigned_user;
}

[Serializable]
public class AssignedUser
{
    public string email;
}

public class ManageProject : MonoBehaviour
{
    public TextMeshProUGUI projectInfo;
    public TMP_InputField newPersonInput;
    public Button addPerson;
    public Transform parent;
    public GameObject prefab;
    public List<GameObject> slots = new List<GameObject>();
    public ProjectDetailsRoot response;
    private void OnEnable()
    {
        projectInfo.text = StateManager.Instance.currentProject.name + "\n" +
                           StateManager.Instance.currentProject.description;

        StartCoroutine(FetchProjectDetails());
    }

    IEnumerator FetchProjectDetails()
    {
        string projectId = StateManager.Instance.currentProject.id;
        string url = $"{StateManager.baseUrl}/admin/projects/{projectId}";
        Debug.Log("Fetching project details from: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            if (StateManager.Instance.sessionInfo != null &&
                !string.IsNullOrEmpty(StateManager.Instance.sessionInfo.accessToken))
            {
                request.SetRequestHeader("Authorization", "Bearer " + StateManager.Instance.sessionInfo.accessToken);
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Network Error: " + request.error);
            }
            else if (request.responseCode == 200)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Project details: " + jsonResponse);

                 response = JsonUtility.FromJson<ProjectDetailsRoot>(jsonResponse);

                if (response != null && response.data != null && response.data.project != null)
                {
                    ProjectDetail proj = response.data.project;

                    // Update main project info
                    projectInfo.text =
                        $"{proj.name}\n{proj.description}\n";

                    // Clear old slots
                    foreach (var slot in slots)
                        Destroy(slot);
                    slots.Clear();

                    // Show assignments
                    foreach (var assignment in proj.assignments)
                    {
                        GameObject slot = Instantiate(prefab, parent);
                        slot.SetActive(true);
                        slot.GetComponentInChildren<TextMeshProUGUI>().text =
                            $"Assigned to: {assignment.assigned_user.email}";
                        slots.Add(slot);
                        slot.GetComponentInChildren<Button>().onClick.AddListener(() =>
                        {
                            // remove person from project
                        });
                    }

                }
            }
            else
            {
                Debug.LogError($"API Error ({request.responseCode}): {request.downloadHandler.text}");
            }
        }
    }
}
