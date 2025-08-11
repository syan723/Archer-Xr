using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Siccity.GLTFUtility;
using System.Linq;

public class ProjectFiles : MonoBehaviour
{
    public static ProjectFiles Instance;
    public GameObject prefab;
    public List<GameObject> slots = new List<GameObject>();
    public FileListResponse fileList;
    public ObjectSpawner spawner; // Assumed to have: public List<GameObject> glbs = new List<GameObject>();
    private string cachePath;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Setup cache directory
        cachePath = Path.Combine(Application.persistentDataPath, "CachedGLBs");
        if (!Directory.Exists(cachePath))
            Directory.CreateDirectory(cachePath);

        Fetch();
    }

    public void Fetch()
    {
        StartCoroutine(FetchFiles());
    }

    IEnumerator FetchFiles()
    {
        string projectId = StateManager.Instance.currentProject.id;
        string url = StateManager.baseUrl + $"mobile/projects/{projectId}/files";
        Debug.Log("Fetching files from: " + url);

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
                Debug.Log("File list JSON: " + json);
                fileList = JsonUtility.FromJson<FileListResponse>(json);

                // Clear old UI slots
                foreach (var slot in slots.ToArray())
                    Destroy(slot);
                slots.Clear();

                // Clear old cached GLB GameObjects in spawner
                foreach (var glbObj in spawner.instantiatedGlbs.ToList())
                    Destroy(glbObj.go);
                spawner.glbsFilePaths.Clear();
                spawner.instantiatedGlbs.Clear();

                // Get current device location first
                yield return StartCoroutine(GetCurrentLocation((currentLocation) =>
                {
                    if (currentLocation == null)
                    {
                        Debug.LogWarning("Current location not available. Defaulting to zero offset.");
                        currentLocation = new Location { latitude = 0, longitude = 0 };
                    }

                    int index = 0;
                    foreach (var file in fileList.data.files)
                    {
                        // Create UI slot
                        GameObject slot = Instantiate(prefab, transform);
                        slots.Add(slot);
                        slot.SetActive(true);
                        slot.GetComponentInChildren<TextMeshProUGUI>().text = file.name.Split('.')[0];

                        if (StateManager.Instance.sessionInfo.userRole == "Admin")
                        {
                            // Capture the file in closure
                            ProjectFile capturedFile = file;
                            slot.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
                            {
                                StartCoroutine(DeleteFile(capturedFile));
                            });
                        }
                        else
                        {
                            slot.transform.GetChild(1).gameObject.SetActive(false);
                        }

                        if (file.name.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
                        {
                            StartCoroutine(DownloadCacheAndSpawnGLB(index, file, currentLocation));
                        }
                        index++;
                    }
                }));
            }
            else
            {
                Debug.LogError($"API Error: {request.responseCode}: {request.downloadHandler.text}");
            }
        }
    }

    IEnumerator GetCurrentLocation(Action<Location> onLocationReady)
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("Location not enabled by user");
            onLocationReady(null);
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            Debug.LogWarning("Timed out waiting for location services");
            onLocationReady(null);
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogWarning("Unable to determine device location");
            onLocationReady(null);
        }
        else
        {
            Location currentLocation = new Location
            {
                latitude = Input.location.lastData.latitude,
                longitude = Input.location.lastData.longitude
            };
            onLocationReady(currentLocation);
        }

        Input.location.Stop();
    }

    IEnumerator DeleteFile(ProjectFile file)
    {
        string projectId = StateManager.Instance.currentProject.id;
        string url = $"{StateManager.baseUrl}mobile/projects/{projectId}/files/{file.id}";
        Debug.Log($"Deleting file at: {url}");

        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            string token = StateManager.Instance.sessionInfo.accessToken;
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Delete File Error: {request.error}");
            }
            else if (request.responseCode == 200 || request.responseCode == 204)
            {
                Debug.Log("File deleted successfully.");

                // Delete cached file locally if exists
                string cachedFilePath = Path.Combine(cachePath, file.name);
                if (File.Exists(cachedFilePath))
                {
                    try
                    {
                        File.Delete(cachedFilePath);
                        Debug.Log("Deleted cached file: " + cachedFilePath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Failed to delete cached file: " + e.Message);
                    }
                }

                // Refresh UI and spawner
                Fetch();
            }
            else
            {
                Debug.LogError($"Failed to delete file. Server responded: {request.responseCode} - {request.downloadHandler.text}");
            }
        }
    }

    IEnumerator DownloadCacheAndSpawnGLB(int index, ProjectFile file, Location currentLocation)
    {
        string filePath = Path.Combine(cachePath, file.name);

        // Load from cache if exists
        if (File.Exists(filePath))
        {
            Debug.Log($"Loading cached GLB: {filePath}");
            SpawnGLBWithOffset(filePath, file, currentLocation);
            yield break;
        }

        string downloadUrl = file.signedUrl;  // Use signedUrl directly!
        Debug.Log("Downloading GLB from: " + downloadUrl);

        using (UnityWebRequest request = UnityWebRequest.Get(downloadUrl))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("GLB Download Error: " + request.error);
            }
            else
            {
                byte[] glbData = request.downloadHandler.data;
                Debug.Log($"Downloaded GLB size: {glbData.Length} bytes");

                try
                {
                    File.WriteAllBytes(filePath, glbData);
                    Debug.Log($"Saved GLB to cache: {filePath}");
                    SpawnGLBWithOffset(filePath, file, currentLocation);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to save or import GLB: {e.Message}");
                }
            }
        }
    }

    void SpawnGLBWithOffset(string filepath, ProjectFile file, Location currentLocation)
    {
        Vector3 positionOffset = Vector3.zero;

        if (file.location != null &&
            (file.location.latitude != 0 || file.location.longitude != 0) &&
            (currentLocation.latitude != 0 || currentLocation.longitude != 0))
        {
            // Calculate approximate offsets in meters
            float latDiff = (float)(file.location.latitude - currentLocation.latitude);
            float lonDiff = (float)(file.location.longitude - currentLocation.longitude);

            // Approximate meters per degree latitude & longitude
            float metersPerLat = 111000f;
            float metersPerLon = 111000f * Mathf.Cos((float)currentLocation.latitude * Mathf.Deg2Rad);

            float xOffset = lonDiff * metersPerLon;
            float zOffset = latDiff * metersPerLat;

            // In Unity, assume X = East/West (longitude), Z = North/South (latitude)
            positionOffset = new Vector3(xOffset, 0, zOffset);
        }

        Debug.Log($"Spawning GLB '{file.name}' at offset position: {positionOffset}");

        GameObject model = ImportGLTF(filepath, positionOffset, Quaternion.identity);
        if (model != null)
        {
            model.transform.position = positionOffset;
            spawner.instantiatedGlbs.Add(new() { go = model, index = fileList.data.files.IndexOf(file) });
        }
    }

    public GameObject ImportGLTF(string filepath, Vector3 position, Quaternion rotation)
    {
        Debug.Log("Importing GLTF: " + filepath);
        GameObject result = Importer.LoadFromFile(filepath);
        if (result != null)
        {
            result.transform.position = position;
            result.transform.rotation = rotation;
        }
        else
        {
            Debug.LogError("Failed to load GLB model from file.");
        }
        return result;
    }

}

[Serializable]
public class FileListResponse
{
    public bool success;
    public FileListData data;
}

[Serializable]
public class FileListData
{
    public List<ProjectFile> files;
    public int totalCount;
    public int totalSize;
}

[Serializable]
public class ProjectFile
{
    public int id;
    public string name;
    public int size;
    public string type;
    public string url;
    public string signedUrl;  // NEW
    public Location location; // NEW (nullable)
    public string uploadedAt;
    public UploadedBy uploadedBy;
}

[Serializable]
public class Location
{
    public double latitude;
    public double longitude;
}

[Serializable]
public class UploadedBy
{
    public string email;
}
