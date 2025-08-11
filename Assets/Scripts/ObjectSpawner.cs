using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
[System.Serializable]
public class Paths
{
    public int index;
    public string path;
}
[System.Serializable]
public class Glbs
{
    public int index;
    public GameObject go;
}
public class ObjectSpawner : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public Camera arCamera;
    public List<Paths> glbsFilePaths;
    public List<Glbs> instantiatedGlbs;

    private int selectedPrefabIndex = -1;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private LocationInfo deviceLocation;
    private bool locationReady = false;

    private void Awake()
    {
        glbsFilePaths = new();
        instantiatedGlbs = new();
    }
    private void Start()
    {
        StartCoroutine(StartLocationService());
    }

    IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("Location service not enabled by user.");
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
            Debug.LogWarning("Timed out initializing location service.");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogWarning("Unable to determine device location.");
            yield break;
        }
        else
        {
            locationReady = true;
            deviceLocation = Input.location.lastData;
            Debug.Log($"Device location acquired: Lat {deviceLocation.latitude}, Lon {deviceLocation.longitude}");
        }
    }

    public void SelectObjectToSpawn(int index)
    {
        selectedPrefabIndex = index;
    }

    void Update()
    {
        if (selectedPrefabIndex == -1) return;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (raycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;

                if (instantiatedGlbs.Exists(x => x.index == selectedPrefabIndex))
                {
                    instantiatedGlbs.Find(x => x.index == selectedPrefabIndex).go.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                    StartCoroutine(UpdateGeoLocationAPI(selectedPrefabIndex, hitPose.position));
                    return;
                }

                var newObj = ProjectFiles.Instance.ImportGLTF(glbsFilePaths.Find(x => x.index == selectedPrefabIndex).path, hitPose.position, hitPose.rotation);
                instantiatedGlbs.Add(new Glbs() { index = selectedPrefabIndex, go = newObj });

                StartCoroutine(UpdateGeoLocationAPI(selectedPrefabIndex, hitPose.position));
            }
        }
    }

    IEnumerator UpdateGeoLocationAPI(int fileIndex, Vector3 arPosition)
    {
        if (!ProjectFiles.Instance.fileList.data.files.Exists(f => f.id == fileIndex))
        {
            Debug.LogWarning($"File with index {fileIndex} not found in file list.");
            yield break;
        }

        if (!locationReady)
        {
            Debug.LogWarning("Device location not ready. Cannot update geo location.");
            yield break;
        }

        var file = ProjectFiles.Instance.fileList.data.files.Find(f => f.id == fileIndex);

        string projectId = StateManager.Instance.currentProject.id;
        string url = $"{StateManager.baseUrl}mobile/projects/{projectId}/files/{file.id}/location";

        // Calculate lat/lon offset based on AR position (meters)
        // Approximation:
        // 1 degree latitude ~= 111,000 meters
        // 1 degree longitude ~= 111,000 * cos(latitude) meters

        double latOffset = arPosition.z / 111000.0;
        double lonOffset = arPosition.x / (111000.0 * Mathf.Cos((float)deviceLocation.latitude * Mathf.Deg2Rad));

        double adjustedLat = deviceLocation.latitude + latOffset;
        double adjustedLon = deviceLocation.longitude + lonOffset;

        var geoData = new GeoLocationPayload
        {
            latitude = adjustedLat,
            longitude = adjustedLon
        };

        string jsonBody = JsonUtility.ToJson(geoData);

        using (UnityWebRequest request = UnityWebRequest.Put(url, jsonBody))
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
                Debug.LogError($"Geo location update failed: {request.error}");
            }
            else
            {
                Debug.Log($"Geo location updated: Lat {adjustedLat}, Lon {adjustedLon}");
            }
        }
    }

    [Serializable]
    public class GeoLocationPayload
    {
        public double latitude;
        public double longitude;
    }
}
