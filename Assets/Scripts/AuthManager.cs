using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro; // Important for TextMeshPro

// This class will hold the data we send to the API
[Serializable]
public class LoginRequestData
{
    public string email;
    public string password;
}

// This class will hold the data we get back from a successful login
[Serializable]
public class LoginResponse
{
    public User user;
    public Session session;
}

[Serializable]
public class User
{
    public string id;
    public string email;
    public UserMetadata user_metadata;
}

[Serializable]
public class UserMetadata
{
    public string role;
}

[Serializable]
public class Session
{
    public string access_token;
    public string refresh_token;
}

// This class will hold the data we get back from an API error
[Serializable]
public class ErrorResponse
{
    public string error;
}

public class AuthManager : MonoBehaviour
{
    // Public fields to drag and drop your UI elements in the Inspector
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text statusText;

    // New: Reference to the current login panel (e.g., the Canvas or a specific panel GameObject)
    public GameObject loginPanel;
    // New: Reference to the panel you want to show after successful login
    public GameObject nextPanel;

    // The API endpoint for logging in
    private string loginUrl = "https://xrarchy.vercel.app/api/auth/login";

    // Call this method from a button's OnClick() event
    public void OnLoginButtonClicked()
    {
        // Clear any previous status message
        statusText.text = "Logging in...";

        // Start the coroutine to handle the web request
        StartCoroutine(AttemptLogin());
    }

    private IEnumerator AttemptLogin()
    {
        // 1. Get the data from the input fields
        string email = emailInputField.text;
        string password = passwordInputField.text;

        // 2. Create the data object to be sent as JSON
        LoginRequestData loginData = new LoginRequestData
        {
            email = email,
            password = password
        };

        // 3. Serialize the data object to a JSON string
        string jsonRequestBody = JsonUtility.ToJson(loginData);

        // 4. Create the web request
        using (UnityWebRequest webRequest = new UnityWebRequest(loginUrl, "POST"))
        {
            // Attach the JSON data to the request
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequestBody);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);

            // Set the content type header to tell the server we're sending JSON
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Set the download handler to receive the response
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            // 5. Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            // 6. Handle the response
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                // A network or other critical error occurred
                Debug.LogError("Network Error: " + webRequest.error);
                statusText.text = "Network Error: " + webRequest.error;
            }
            else
            {
                // The request was successful, but we need to check the status code
                string jsonResponse = webRequest.downloadHandler.text;

                if (webRequest.responseCode == 200)
                {
                    // Login was successful!
                    LoginResponse responseData = JsonUtility.FromJson<LoginResponse>(jsonResponse);

                    // You would typically save this token for future API calls
                    string accessToken = responseData.session.access_token;
                    string userRole = responseData.user.user_metadata.role;

                    Debug.Log("Login Successful! Access Token: " + accessToken);
                    Debug.Log("User Role: " + userRole);

                    statusText.text = "Login Successful! Welcome, " + email ;

                    // --- New Logic for Hiding Message and Showing Next Panel ---
                    yield return new WaitForSeconds(2f); // Wait for 2 seconds

                    // Hide the status message after the delay
                    statusText.text = "";

                    // Deactivate the current login panel
                    if (loginPanel != null)
                    {
                        loginPanel.SetActive(false);
                    }
                    else
                    {
                        Debug.LogWarning("Login Panel reference is not set in the Inspector.");
                    }

                    // Activate the next panel
                    if (nextPanel != null)
                    {
                        nextPanel.SetActive(true);
                    }
                    else
                    {
                        Debug.LogWarning("Next Panel reference is not set in the Inspector.");
                    }
                    // --- End New Logic ---
                }
                else
                {
                    // An API-specific error occurred (e.g., incorrect password)
                    ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(jsonResponse);
                    Debug.LogError("Login Failed: " + errorResponse.error);
                    statusText.text = "Login Failed: " + errorResponse.error;
                }
            }
        }
    }
}