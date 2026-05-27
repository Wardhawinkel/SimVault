using UnityEngine;
using System.Runtime.InteropServices;

public class KeyboardDetector : MonoBehaviour
{
    [SerializeField] private GameObject stepPanel;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void InitKeyboardDetection();
#endif

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        InitKeyboardDetection();
#endif
    }

    public void OnKeyboardShown(string value)
    {
        Debug.Log("[Keyboard] Toetsenbord zichtbaar → tutorial verbergen");
        stepPanel?.SetActive(false);
    }

    public void OnKeyboardHidden(string value)
    {
        Debug.Log("[Keyboard] Toetsenbord verborgen → tutorial tonen");
        stepPanel?.SetActive(true);
    }
}