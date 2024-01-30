using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SliderInputField : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private string _numberFormat = "0.######";
    [SerializeField] private string _prefKey;
    [SerializeField] private float _defaultValue;

    [FormerlySerializedAs("_onValueChanged")]
    public UnityEvent<float> onValueChanged;

    void Start()
    {
        if(!string.IsNullOrEmpty(_prefKey))
        {
            _slider.value = PlayerPrefs.GetFloat(_prefKey, _defaultValue);
        }
        else
        {
            _slider.value = _defaultValue;
        }

        _slider.onValueChanged.AddListener(OnSliderChanged);
        _inputField.onValueChanged.AddListener(OnTextChanged);

        OnSliderChanged(_slider.value);
    }

    private void OnSliderChanged(float value)
    {
        _inputField.SetTextWithoutNotify(value.ToString(_numberFormat));
        ValueChanged(value);
    }

    private void OnTextChanged(string text)
    {
        if (float.TryParse(text, out float value))
        {
            value = Mathf.Clamp(value, _slider.minValue, _slider.maxValue);

            _slider.SetValueWithoutNotify(value);
            ValueChanged(value);
        }
    }

    private void ValueChanged(float value)
    {
        if (!string.IsNullOrEmpty(_prefKey)) PlayerPrefs.SetFloat(_prefKey, value);
        onValueChanged?.Invoke(value);
    }

    public void Import(Dictionary<string, float> values)
    {
        _slider.value = values.GetValueOrDefault(_prefKey, _slider.value);
    }

    public float Export(out string key)
    {
        key = _prefKey;
        return _slider.value;
    }
}
