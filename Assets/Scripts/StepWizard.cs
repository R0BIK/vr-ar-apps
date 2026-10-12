using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class StepWizard : MonoBehaviour
{
    [Header("Головна панель планшета")]
    [SerializeField] private GameObject tabletRoot;

    [Header("Екрани візарда (Кроки)")]
    [SerializeField] private GameObject step1InstructionPanel;
    [SerializeField] private GameObject step2FormPanel;
    [SerializeField] private GameObject step3ResultPanel;

    [Header("Елементи форми (Крок 2)")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private TextMeshProUGUI powerValueText;
    [SerializeField] private Toggle safetyToggle;
    [SerializeField] private Toggle autoCalibrationToggle;

    [Header("Текст результату (Крок 3)")]
    [SerializeField] private TextMeshProUGUI summaryResultText;

    private bool _hasTablet;
    private bool _hasStep1;
    private bool _hasStep2;
    private bool _hasStep3;
    private bool _hasSlider;
    private bool _hasPowerText;
    private bool _hasSafetyToggle;
    private bool _hasAutoToggle;
    private bool _hasSummaryText;

    private float _savedPower = 50f;
    private bool _savedSafety = true;
    private bool _savedAutoCalibration = false;

    private int _currentStep = 1;
    
    private Canvas _canvas;
    private bool _hasCanvas;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _hasCanvas = _canvas != null;
        _hasTablet = tabletRoot != null;
        _hasStep1 = step1InstructionPanel != null;
        _hasStep2 = step2FormPanel != null;
        _hasStep3 = step3ResultPanel != null;
        _hasSlider = powerSlider != null;
        _hasPowerText = powerValueText != null;
        _hasSafetyToggle = safetyToggle != null;
        _hasAutoToggle = autoCalibrationToggle != null;
        _hasSummaryText = summaryResultText != null;
    }

    private void Start()
    {
        if (_hasSlider)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = 100f;
            powerSlider.value = _savedPower;
            powerSlider.onValueChanged.AddListener(OnPowerSliderChanged);
            UpdatePowerText(_savedPower);
        }

        if (_hasSafetyToggle) safetyToggle.isOn = _savedSafety;
        if (_hasAutoToggle) autoCalibrationToggle.isOn = _savedAutoCalibration;

        GoToStep(1);
    }

    private void Update()
    {
        bool toggleRequested = false;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.mKey.wasPressedThisFrame)
        {
            toggleRequested = true;
        }

        if (OVRInput.GetDown(OVRInput.Button.Start) || OVRInput.GetDown(OVRInput.Button.Four))
        {
            toggleRequested = true;
        }

        if (toggleRequested)
        {
            ToggleTablet();
        }
    }

    public void ToggleTablet()
    {
        if (_hasCanvas)
        {
            _canvas.enabled = !_canvas.enabled;
        }
    }

    public void GoToStep(int step)
    {
        _currentStep = Mathf.Clamp(step, 1, 3);

        if (_hasStep1) step1InstructionPanel.SetActive(_currentStep == 1);
        if (_hasStep2) step2FormPanel.SetActive(_currentStep == 2);
        if (_hasStep3) step3ResultPanel.SetActive(_currentStep == 3);

        if (_currentStep == 3)
        {
            GenerateSummary();
        }
    }

    public void NextStep() => GoToStep(_currentStep + 1);
    public void PrevStep() => GoToStep(_currentStep - 1);

    private void OnPowerSliderChanged(float value)
    {
        UpdatePowerText(value);
    }

    private void UpdatePowerText(float value)
    {
        if (_hasPowerText)
        {
            powerValueText.text = $"{Mathf.RoundToInt(value)} %";
        }
    }

    public void SaveFormValues()
    {
        if (_hasSlider) _savedPower = powerSlider.value;
        if (_hasSafetyToggle) _savedSafety = safetyToggle.isOn;
        if (_hasAutoToggle) _savedAutoCalibration = autoCalibrationToggle.isOn;

        Debug.Log($"[StepWizard] Збережено: Потужність={_savedPower}%, Безпека={_savedSafety}, Автокалібрування={_savedAutoCalibration}");
        NextStep();
    }

    public void ResetFormValues()
    {
        _savedPower = 50f;
        _savedSafety = true;
        _savedAutoCalibration = false;

        if (_hasSlider) powerSlider.value = _savedPower;
        if (_hasSafetyToggle) safetyToggle.isOn = _savedSafety;
        if (_hasAutoToggle) autoCalibrationToggle.isOn = _savedAutoCalibration;

        UpdatePowerText(_savedPower);
        Debug.Log("[StepWizard] Значення скинуто.");
    }

    private void GenerateSummary()
    {
        if (_hasSummaryText)
        {
            string safetyStr = _savedSafety ? "<color=green>УВІМКНЕНО</color>" : "<color=red>ВИМКНЕНО</color>";
            string autoStr = _savedAutoCalibration ? "<color=green>АКТИВНА</color>" : "<color=yellow>РУЧНА</color>";

            summaryResultText.text = $"<b>ПІДСУМОК НАЛАШТУВАННЯ:</b>\n\n" +
                                     $"• Рівень потужності: <b>{Mathf.RoundToInt(_savedPower)}%</b>\n" +
                                     $"• Контур безпеки: {safetyStr}\n" +
                                     $"• Автокалібрування: {autoStr}\n\n" +
                                     $"<color=#00ffc8>Система готова до роботи!</color>";
        }
    }
}