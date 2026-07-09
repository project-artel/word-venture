using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RemoteControlPoCController : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text outputText;

    private void Awake()
    {
        submitButton.onClick.RemoveListener(CopyInputToOutput);
        submitButton.onClick.AddListener(CopyInputToOutput);
    }

    private void CopyInputToOutput()
    {
        outputText.text = inputField.text;
    }
}
