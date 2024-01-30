using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonEvents : MonoBehaviour
{
    [SerializeField] private GameObject[] _hideInStandalone;
    [SerializeField] private GameObject[] _hideInWebPlayer;
    [SerializeField] private GameObject[] _hideInFullscreenWebPlayer;
    [SerializeField] private SliderInputField[] _sliderInputFields;
    [SerializeField] private TextMeshProUGUI _exportedText;
    [SerializeField] private Image _exportedTextPanel;
    [SerializeField] private TMP_InputField _settingsField;

    private float _exportedTextFadeTime = float.MinValue;
    private bool _importingSettings = false;

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void CopyToClipboardAndShare(string textToCopy);

    [DllImport("__Internal")]
    public static extern void CopyPasteReader(string gObj, string vName);
#endif

    void Awake()
    {
#if UNITY_WEBGL
        foreach (var item in _hideInWebPlayer) item.SetActive(false);
        foreach (var slider in _sliderInputFields) slider.onValueChanged.AddListener(OnValueChanged);
#else
        foreach (var item in _hideInStandalone) item.SetActive(false);
#endif
    }

    void Update()
    {
        var exportedTextFade = Mathf.InverseLerp(1.0f, 0.0f, Time.unscaledTime - _exportedTextFadeTime);
        _exportedText.color = new Color(1.0f, 1.0f, 1.0f, exportedTextFade);
        _exportedTextPanel.color = new Color(0.0f, 0.0f, 0.0f, exportedTextFade * 0.5f);

#if UNITY_WEBGL
        var isFullScreen = Screen.fullScreen;
        foreach (var item in _hideInFullscreenWebPlayer) item.SetActive(!isFullScreen);
#endif
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Export()
    {
        var pairs = new string[_sliderInputFields.Length];
        for(int i = 0; i < pairs.Length; i++)
        {
            var value = _sliderInputFields[i].Export(out var key);
            pairs[i] = $"{key}:{value.ToString("0.######", CultureInfo.InvariantCulture)}";
        }

        var settings = string.Join(",", pairs);
#if UNITY_WEBGL
        CopyToClipboardAndShare(settings);
#else
        GUIUtility.systemCopyBuffer = settings;
#endif
        _exportedTextFadeTime = Time.unscaledTime + 1.0f;
    }

    public void Import()
    {
#if UNITY_WEBGL
        CopyPasteReader(name, "DoImport");
#else
        DoImport(GUIUtility.systemCopyBuffer);
#endif
    }

    public void DoImport(string settings)
    {
        var pairs = settings.Split(",");
        var values = new Dictionary<string, float>();
        foreach (var pair in pairs)
        {
            var kv = pair.Split(":");
            if (kv.Length == 2)
            {
                if (kv[0].Length > 0 && float.TryParse(kv[1], out float value))
                {
                    values[kv[0]] = value;
                }
            }
        }

        foreach (var slider in _sliderInputFields)
        {
            slider.Import(values);
        }
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void SettingsChanged()
    {
        var pairs = _settingsField.text.Split(",");
        var values = new Dictionary<string, float>();
        foreach (var pair in pairs)
        {
            var kv = pair.Split(":");
            if (kv.Length == 2)
            {
                if (kv[0].Length > 0 && float.TryParse(kv[1], out float value))
                {
                    values[kv[0]] = value;
                }
            }
        }

        _importingSettings = true;
        foreach (var slider in _sliderInputFields)
        {
            slider.Import(values);
        }
        _importingSettings = false;
    }

    private void OnValueChanged(float _)
    {
        if (_importingSettings) return;

        var pairs = new string[_sliderInputFields.Length];
        for (int i = 0; i < pairs.Length; i++)
        {
            var value = _sliderInputFields[i].Export(out var key);
            pairs[i] = $"{key}:{value.ToString("0.######", CultureInfo.InvariantCulture)}";
        }

        _settingsField.text = string.Join(",", pairs);
    }
}
