using TMPro;
using UnityEngine;

public class DropdownMenu : MonoBehaviour
{
    public GameObject menuPanel; 

    private bool isMenuOpen = true;

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        menuPanel.SetActive(isMenuOpen);
    }
}
