using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Google;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoogleAuthManager : MonoBehaviour
{
    // === IMPORTANT: PASTE YOUR WEB CLIENT ID HERE ===
    // You get this from the Firebase Console -> Authentication -> Sign-in method -> Google -> Web SDK configuration
    public string webClientId = "510989075004-kjscj9hv7qnqudnpgqrokqcn2n3vcfpu.apps.googleusercontent.com";

    // Firebase and Google Sign-in instances
    private FirebaseAuth auth;
    private GoogleSignInConfiguration googleConfig;

    [Header("UI Elements")]
    public TMP_Text statusText;
    public Button signInButton;
    public Button signOutButton;

    void Start()
    {
        // 1. Initialize Firebase Auth and Google Sign-in Configuration
        InitializeFirebase();
        InitializeGoogleSignIn();

        // 2. Set up button click listeners
        signInButton.onClick.AddListener(OnSignInClicked);
        signOutButton.onClick.AddListener(OnSignOutClicked);

        // 3. Update the UI based on the current auth state
        UpdateUI();
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        // Listen for authentication state changes (good practice)
        auth.StateChanged += OnAuthStateChanged;
        OnAuthStateChanged(this, null);
    }

    private void InitializeGoogleSignIn()
    {
        googleConfig = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true,
            RequestEmail = true
        };
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            // User is signed in
            statusText.text = $"Signed in as: {user.DisplayName} ({user.Email})";
            signInButton.gameObject.SetActive(false);
            signOutButton.gameObject.SetActive(true);
        }
        else
        {
            // User is signed out
            statusText.text = "Signed out. Please sign in.";
            signInButton.gameObject.SetActive(true);
            signOutButton.gameObject.SetActive(false);
        }
    }

    public void OnSignInClicked()
    {
        statusText.text = "Attempting to sign in with Google...";

        // Step 1: Start the Google Sign-in process
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(HandleGoogleSignInResult);
    }

    private async Task HandleGoogleSignInResult(Task<GoogleSignInUser> task)
    {
        if (task.IsCanceled)
        {
            statusText.text = "Google Sign-in was canceled.";
            Debug.LogError("Google Sign-in was canceled.");
            return;
        }
        if (task.IsFaulted)
        {
            statusText.text = "Google Sign-in failed.";
            Debug.LogError("Google Sign-in failed: " + task.Exception);
            return;
        }

        GoogleSignInUser googleUser = task.Result;
        statusText.text = "Signed in with Google. Authenticating with Firebase...";

        try
        {
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);

            FirebaseUser user = await auth.SignInWithCredentialAsync(credential);  // ✅ Correct

            Debug.Log("Signed in successfully with Firebase! User ID: " + user.UserId);
            UpdateUI();
        }
        catch (Exception e)
        {
            statusText.text = "Firebase Sign-in failed.";
            Debug.LogError("Firebase Sign-in failed: " + e.Message);
        }
    }


    private void HandleFirebaseSignInResult(Task<AuthResult> task)
    {
        if (task.IsCanceled)
        {
            statusText.text = "Firebase Sign-in canceled.";
            Debug.LogError("Firebase Sign-in canceled.");
            return;
        }
        if (task.IsFaulted)
        {
            statusText.text = "Firebase Sign-in failed.";
            Debug.LogError("Firebase Sign-in failed: " + task.Exception);
            return;
        }

        FirebaseUser user = task.Result.User;
        Debug.Log("Signed in successfully with Firebase! User ID: " + user.UserId);
        UpdateUI();
    }

    public void OnSignOutClicked()
    {
        auth.SignOut();
        GoogleSignIn.DefaultInstance.SignOut();
        UpdateUI();
        Debug.Log("User signed out.");
    }
}