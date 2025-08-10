using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Upload : MonoBehaviour
{
    private string glbMimeType;
    public ProjectFiles projectFiles;

    void Start()
    {
        // Get the correct MIME type for .glb
        glbMimeType = NativeFilePicker.ConvertExtensionToFileType("glb");

        Debug.Log("GLB MIME/UTI is: " + glbMimeType);

        // Set button click listener
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        // Avoid opening picker while busy
        if (NativeFilePicker.IsFilePickerBusy())
            return;

        // Pick a .glb file
        NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                Debug.Log("Operation cancelled");
            }
            else
            {
                Debug.Log("Picked file: " + path);
                StartCoroutine(UploadFile(path));
            }
        }, new string[] { glbMimeType });
    }

    IEnumerator UploadFile(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        string fileName = Path.GetFileName(filePath);

        string url = StateManager.baseUrl + "mobile/projects/" + StateManager.Instance.currentProject.id + "/files";
        string token = StateManager.Instance.sessionInfo.accessToken;

        // Create form and add fields
        WWWForm form = new WWWForm();

        // Add file as binary
        form.AddBinaryData("file", fileData, fileName, "model/gltf-binary");  // You can change the MIME if needed

        // Add file name as a separate form field
        form.AddField("fileName", fileName);

        UnityWebRequest request = UnityWebRequest.Post(url, form);

        // Add Authorization header
        request.SetRequestHeader("Authorization", "Bearer " + token);

        // Send request
        yield return request.SendWebRequest();

        // Handle response
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Upload failed: " + request.error);
        }
        else if (request.responseCode == 201)
        {
            Debug.Log("Upload successful!");
            projectFiles.Fetch();
        }
        else
        {
            Debug.LogError("Server error: " + request.responseCode + " - " + request.downloadHandler.text);
        }
    }
}
