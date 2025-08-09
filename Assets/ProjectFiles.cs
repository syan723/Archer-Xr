using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectFiles : MonoBehaviour
{
    public GameObject prefab;
    public List<GameObject> slots;
    public FileListResponse fileList;
    private void Start()
    {
        StartCoroutine(FetchFiles());
    }

    IEnumerator FetchFiles()
    {
        string projectId = StateManager.Instance.currentProject.id;
        string url = StateManager.baseUrl + "projects/" + projectId + "/files";
        Debug.LogError(url);
        string token = StateManager.Instance.sessionInfo.accessToken;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("File Fetch Error: " + request.error);
            }
            else if (request.responseCode == 200)
            {
                string json = request.downloadHandler.text;
                Debug.LogError(json);
                fileList = JsonUtility.FromJson<FileListResponse>(json);

                foreach (var item in slots.ToList())
                {
                    Destroy(item);
                }
                foreach (var file in fileList.data.files)
                {
                    GameObject slot = Instantiate(prefab, transform);
                    slots.Add(slot);
                    slot.gameObject.SetActive(true);

                    slot.GetComponentInChildren<TextMeshProUGUI>().text = (file.name.Split('.')[0]);

                }
            }
            else
            {
                Debug.LogError("API Error: " + request.responseCode + ": " + request.downloadHandler.text);
            }
        }
    }
}
[System.Serializable]
public class FileListResponse
{
    public bool success;
    public FileListData data;
}

[System.Serializable]
public class FileListData
{
    public List<ProjectFile> files;
}

[System.Serializable]
public class ProjectFile
{
    public string id;
    public string name;
    public string type;
    public string createdAt;
}
