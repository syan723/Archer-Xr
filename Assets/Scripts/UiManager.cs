using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Serialization;

public class UiManager : MonoBehaviour
{
    public GameObject bgScreen;
    public GameObject welcomeScreen;
    public GameObject loginScreen;
    public GameObject menuScreen;
    public GameObject historicalMenuScreen;
    public GameObject editExistingProjectScreen;
    public GameObject saveProjectScreen;
    public GameObject objectspawnScreen;

    public void HideAllPanels()
    {
        welcomeScreen.SetActive(false);
        loginScreen.SetActive(false);
        menuScreen.SetActive(false);
        historicalMenuScreen.SetActive(false);
        editExistingProjectScreen.SetActive(false);
        saveProjectScreen.SetActive(false);
        objectspawnScreen.SetActive(false);
    }

    public void OpenAR()
    {
        bgScreen.SetActive(false);
        menuScreen.SetActive(false);
        objectspawnScreen.SetActive(true);
    }
}
