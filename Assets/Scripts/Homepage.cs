using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Startpagina ("Nieuwe tab") van de fictieve browser.
/// Toont snelkoppelingen naar de gesimuleerde websites
/// en een welkomstbericht.
/// </summary>
public class HomePage : MonoBehaviour
{
    [Header("Snelkoppeling-knoppen")]
    [SerializeField] private Button emailButton;
    [SerializeField] private Button shopButton;

    [Header("Welkomstbericht")]
    [SerializeField] private TextMeshProUGUI welcomeText;

    [Header("Referenties")]
    [SerializeField] private BrowserController browser;
    [SerializeField] private TutorialManager   tutorialManager;

    private void Awake()
    {
        emailButton.onClick.AddListener(() =>
        {
            browser.NavigateTo("sim://mail");
            tutorialManager?.TriggerStep("mailsim_clicked"); 
        });

        shopButton.onClick.AddListener(() =>
        {
            browser.NavigateTo("sim://shop");
            tutorialManager?.TriggerStep("shopsim_clicked"); 
        });
    }

    private void OnEnable()
    {
        welcomeText.text =
            "Welkom in de SimBrowser!";
    }
}
