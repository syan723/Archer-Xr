using UnityEngine;
using UnityEngine.UI;

public class Mainmenu : MonoBehaviour
{
    public Button createProject, editExistingProjects;
    private void Start()
    {
        bool isAdmin = (StateManager.Instance.sessionInfo.userRole == "Admin");
        createProject.gameObject.SetActive(isAdmin);
        editExistingProjects.gameObject.SetActive(isAdmin);
    }
}
