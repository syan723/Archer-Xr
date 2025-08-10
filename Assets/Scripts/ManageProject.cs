using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[Serializable]
public class ProjectAssignmentsResponse
{
    public List<UserAssignment> assignments;
}

[Serializable]
public class UserAssignment
{
    public string id; // assignment ID
    public string assigned_at;
    public AssignedUser assigned_user;
}

[Serializable]
public class AssignedUser
{
    public string id; // user ID
    public string role;
    public string email;
}

[Serializable]
public class AddUserRequest
{
    public string email;
    public AddUserRequest(string email)
    {
        this.email = email;
    }
}

public class ManageProject : MonoBehaviour
{
    public TextMeshProUGUI projectInfo;
    public TMP_InputField newPersonInput;
    public Button addPerson;
    public Transform parent;
    public GameObject prefab;
    public List<GameObject> slots = new List<GameObject>();
    

    private void OnEnable()
    {
        projectInfo.text = StateManager.Instance.currentProject.name + "\n" +
                           StateManager.Instance.currentProject.description;

        addPerson.onClick.RemoveAllListeners();
        addPerson.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(newPersonInput.text))
            {
                StartCoroutine(AddUserToProject(newPersonInput.text));
            }
        });

        StartCoroutine(FetchProjectUsers());
    }

    IEnumerator FetchProjectUsers()
    {
        string projectId = StateManager.Instance.currentProject.id;
        string url = $"{StateManager.baseUrl}projects/{projectId}/users";
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

                var response = JsonUtility.FromJson<ProjectAssignmentsResponse>(jsonResponse);

                if (response != null && response.assignments != null)
                {
                    foreach (var slot in slots)
                        Destroy(slot);
                    slots.Clear();

                    foreach (var assignment in response.assignments)
                    {
                        GameObject slot = Instantiate(prefab, parent);
                        slot.SetActive(true);
                        slot.GetComponentInChildren<TextMeshProUGUI>().text = $"{assignment.assigned_user.email}";
                        slots.Add(slot);

                        slot.GetComponentInChildren<Button>().onClick.AddListener(() =>
                        {
                            StartCoroutine(RemoveUserFromProject(assignment.assigned_user.email));
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

    IEnumerator AddUserToProject(string email)
    {
        string projectId = StateManager.Instance.currentProject.id;
        string url = $"{StateManager.baseUrl}mobile/projects/{projectId}/users";
        Debug.LogError(url);
        var body = new AddUserRequest(email);
        string jsonBody = JsonUtility.ToJson(body);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (StateManager.Instance.sessionInfo != null &&
                !string.IsNullOrEmpty(StateManager.Instance.sessionInfo.accessToken))
            {
                request.SetRequestHeader("Authorization", "Bearer " + StateManager.Instance.sessionInfo.accessToken);
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Add User Error: " + request.error);
            }
            else
            {
                Debug.Log("User added successfully");
                newPersonInput.text = "";
                StartCoroutine(FetchProjectUsers()); // Refresh list
            }
        }
    }

    IEnumerator RemoveUserFromProject(string email)
    {
        string projectId = StateManager.Instance.currentProject.id;
        string url = $"{StateManager.baseUrl}mobile/projects/{projectId}/users";

        var body = new AddUserRequest(email);
        string jsonBody = JsonUtility.ToJson(body);
        Debug.LogError("Delete URL: " + url);
        Debug.LogError("Delete Body: " + jsonBody);

        // Create UnityWebRequest with method DELETE and body
        using (UnityWebRequest request = new UnityWebRequest(url, "DELETE"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            if (StateManager.Instance.sessionInfo != null &&
                !string.IsNullOrEmpty(StateManager.Instance.sessionInfo.accessToken))
            {
                request.SetRequestHeader("Authorization", "Bearer " + StateManager.Instance.sessionInfo.accessToken);
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Remove User Error: " + request.error);
            }
            else
            {
                Debug.Log("User removed successfully");
                StartCoroutine(FetchProjectUsers()); // Refresh list
            }
        }
    }

}
