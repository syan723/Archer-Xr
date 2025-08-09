using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement; // Important for TextMeshPro

// This class will hold the data we send to the API
[Serializable]
public class LoginRequestData
{
    public string email;
    public string password;
}
[Serializable]
public class RegisterRequestData
{
    public string email;
    public string password;
    public string role;
}
[Serializable]
public class LoginApiResponse
{
    public bool success;
    public string message;
    public LoginResponseData data;
}

[Serializable]
public class LoginResponseData
{
    public User user;
    public Session session;
}

[Serializable]
public class User
{
    public string id;
    public string email;
    public string role;
    public bool emailConfirmed;
    public string createdAt;
}

[Serializable]
public class Session
{
    public string accessToken;
    public string refreshToken;
    public long expiresAt;
    public int expiresIn;
}
[Serializable]
public class UserMetadata
{
    public string role;
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

    public TMP_InputField register_emailInputField;
    public TMP_InputField register_passwordInputField;
    public TMP_Text register_statusText;
    // New: Reference to the current login panel (e.g., the Canvas or a specific panel GameObject)
    public GameObject loginPanel;
    // New: Reference to the panel you want to show after successful login
    public GameObject nextPanel;

    // The API endpoint for logging in
    private string loginUrl = StateManager.baseUrl + "auth/login";
    private string registerUrl = StateManager.baseUrl + "auth/register";

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
        statusText.text = "Logging In...";

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
                    Debug.LogError(jsonResponse);
                    LoginApiResponse apiResponse = JsonUtility.FromJson<LoginApiResponse>(jsonResponse);

                    try
                    {
                        string accessToken = apiResponse.data.session.accessToken;
                        string userRole = apiResponse.data.user.role;
                        Debug.Log("Login Successful! Access Token: " + accessToken);
                        Debug.Log("User Role: " + userRole);

                        StateManager.Instance.sessionInfo = new SessionInfo { accessToken = accessToken, userRole = userRole };
                        PlayerPrefs.SetString("Session Info", JsonUtility.ToJson(StateManager.Instance.sessionInfo));
                        statusText.text = "Login Successful! Welcome, " + email;
                    }
                    catch (Exception e)
                    {
                        statusText.text = "Something went wrong, message : " + e.Message;
                        yield break;
                    }

                    yield return new WaitForSeconds(1f);
                    statusText.text = "";
                    if (loginPanel != null)
                    {
                        loginPanel.SetActive(false);
                    }

                    SceneManager.LoadScene(1);
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
    public void OnRegisterButtonClicked()
    {
        // Clear any previous status message
        register_statusText.text = "Registering...";

        // Start the coroutine to handle the web request
        StartCoroutine(SignupLogin());
    }
    private IEnumerator SignupLogin()
    {
        // 1. Get the data from the input fields
        string email = register_emailInputField.text;
        string password = register_passwordInputField.text;

        // 2. Create the data object to be sent as JSON
        RegisterRequestData registerData = new RegisterRequestData
        {
            email = email,
            password = password,
            role = "User"
        };
        register_statusText.text = "Registering In...";

        // 3. Serialize the data object to a JSON string
        string jsonRequestBody = JsonUtility.ToJson(registerData);

        // 4. Create the web request
        using (UnityWebRequest webRequest = new UnityWebRequest(registerUrl, "POST"))
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
                register_statusText.text = "Network Error: " + webRequest.error;
            }
            else
            {
                // The request was successful, but we need to check the status code
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.LogError(webRequest.responseCode);
                if (webRequest.responseCode == 201)
                {
                    // Login was successful!
                    //Login responseData = JsonUtility.FromJson<LoginResponse>(jsonResponse);

                    // You would typically save this token for future API calls
                    //string accessToken = responseData.session.access_token;
                    //string userRole = responseData.user.user_metadata.role;

                    register_statusText.text = "Register Successful! Welcome, " + email + "\n Check Email";

                    // --- New Logic for Hiding Message and Showing Next Panel ---
                    yield return new WaitForSeconds(1f); // Wait for 2 seconds

                    // Hide the status message after the delay
                    register_statusText.text = "";

                    // Deactivate the current login panel
                    //if (loginPanel != null)
                    //{
                    //    loginPanel.SetActive(false);
                    //}
                    //else
                    //{
                    //    Debug.LogWarning("Login Panel reference is not set in the Inspector.");
                    //}

                    // Activate the next panel
                    //if (nextPanel != null)
                    //{
                    //    //nextPanel.SetActive(true);
                    //}
                    //else
                    //{
                    //    Debug.LogWarning("Next Panel reference is not set in the Inspector.");
                    //}
                    nextPanel.SetActive(false);
                    loginPanel.SetActive(true);
                    // --- End New Logic ---
                }
                else
                {
                    // An API-specific error occurred (e.g., incorrect password)
                    ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(jsonResponse);
                    Debug.LogError("Login Failed: " + errorResponse.error);
                    register_statusText.text = "Login Failed: " + errorResponse.error;
                }
            }
        }
    }
}
[Serializable]
public class SessionInfo
{
    public string accessToken;
    public string userRole;
}