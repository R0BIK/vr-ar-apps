using UnityEngine;
using TMPro;

public class AssemblyStandController : MonoBehaviour
{
    [SerializeField] private SnapZone[] sockets;
    [SerializeField] private PhysicalToggleSwitch powerToggle;
    [SerializeField] private PhysicalSlider3D voltageSlider;
    [SerializeField] private Renderer statusLampRenderer;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private float minOperatingVoltage = 10f;

    private bool _hasLamp;
    private bool _hasStatusText;
    private bool _hasToggle;
    private bool _hasSlider;
    private Material _lampMaterial;
    private bool _testPassed;
    private int _lastSnappedCount = -1;
    private bool _lastPowerState;
    private float _lastVoltage = -1f;

    private void Awake()
    {
        if (statusLampRenderer != null && statusLampRenderer.material != null)
        {
            _lampMaterial = statusLampRenderer.material;
            _hasLamp = true;
        }

        _hasStatusText = statusText != null;
        _hasToggle = powerToggle != null;
        _hasSlider = voltageSlider != null;
    }

    private void Start()
    {
        RefreshDisplay(true);
    }

    private void Update()
    {
        int snappedCount = CountSnappedSockets();
        bool powerOn = _hasToggle && powerToggle.IsOn;
        float voltage = _hasSlider ? voltageSlider.CurrentValue : 0f;

        if (snappedCount != _lastSnappedCount || powerOn != _lastPowerState || Mathf.Abs(voltage - _lastVoltage) > 0.25f)
        {
            if (snappedCount < (sockets != null ? sockets.Length : 0) || !powerOn)
            {
                _testPassed = false;
            }

            RefreshDisplay(false);
        }
    }

    public void OnAssemblyChanged()
    {
        _testPassed = false;
        RefreshDisplay(true);
    }

    public void RunDiagnosticTest()
    {
        int total = sockets != null ? sockets.Length : 0;
        int snapped = CountSnappedSockets();
        bool powerOn = _hasToggle && powerToggle.IsOn;
        float voltage = _hasSlider ? voltageSlider.CurrentValue : 0f;

        if (snapped < total)
        {
            _testPassed = false;
            SetLampColor(new Color(0.95f, 0.3f, 0.2f, 1f));
            SetStatus($"Спочатку зберіть вузол ({snapped}/{total})");
            return;
        }

        if (!powerOn)
        {
            _testPassed = false;
            SetLampColor(new Color(0.95f, 0.65f, 0.1f, 1f));
            SetStatus("Увімкніть тумблер живлення!");
            return;
        }

        if (voltage < minOperatingVoltage)
        {
            _testPassed = false;
            SetLampColor(new Color(0.95f, 0.65f, 0.1f, 1f));
            SetStatus($"Мала напруга: {voltage:F0} V (мін. {minOperatingVoltage:F0} V)");
            return;
        }

        _testPassed = true;
        SetLampColor(new Color(0.15f, 0.95f, 0.35f, 1f));
        SetStatus($"ТЕСТ ПРОЙДЕНО! Вузол працює ({voltage:F0} V)");
    }

    private void RefreshDisplay(bool force)
    {
        int total = sockets != null ? sockets.Length : 0;
        int snapped = CountSnappedSockets();
        bool powerOn = _hasToggle && powerToggle.IsOn;
        float voltage = _hasSlider ? voltageSlider.CurrentValue : 0f;

        _lastSnappedCount = snapped;
        _lastPowerState = powerOn;
        _lastVoltage = voltage;

        if (_testPassed)
        {
            return;
        }

        if (snapped < total)
        {
            SetLampColor(new Color(0.9f, 0.55f, 0.15f, 1f));
            SetStatus($"Зберіть вузол: {snapped}/{total} деталей");
        }
        else if (!powerOn)
        {
            SetLampColor(new Color(0.2f, 0.75f, 0.95f, 1f));
            SetStatus($"Вузол зібрано ({snapped}/{total})!\nУвімкніть тумблер живлення");
        }
        else
        {
            SetLampColor(new Color(0.2f, 0.85f, 0.95f, 1f));
            SetStatus($"Живлення ON ({voltage:F0} V)\nНатисніть кнопку ТЕСТ");
        }
    }

    private int CountSnappedSockets()
    {
        if (sockets == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < sockets.Length; i++)
        {
            if (sockets[i] != null && sockets[i].IsSnapped)
            {
                count++;
            }
        }
        return count;
    }

    private void SetLampColor(Color color)
    {
        if (_hasLamp)
        {
            _lampMaterial.color = color;
        }
    }

    private void SetStatus(string text)
    {
        if (_hasStatusText)
        {
            statusText.text = text;
        }
    }
}
