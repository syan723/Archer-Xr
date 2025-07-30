using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public Camera arCamera;
    public GameObject[] objectPrefabs;

    private GameObject selectedPrefab;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public void SelectObjectToSpawn(int index)
    {
        selectedPrefab = objectPrefabs[index];
    }

    void Update()
    {
        if (selectedPrefab != null && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (raycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                Instantiate(selectedPrefab, hitPose.position, hitPose.rotation);
            }
        }
    }
}
