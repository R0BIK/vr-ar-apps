using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Oculus.Interaction;

public class PhysicalSlider3D : MonoBehaviour
{
    [SerializeField] private PokeInteractable trackPokeInteractable;
    [SerializeField] private Transform handleTransform;
    [SerializeField] private float minLocalX = -0.1f;
    [SerializeField] private float maxLocalX = 0.1f;
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 100f;
    [SerializeField] private float initialNormalizedValue = 0.5f;
    [SerializeField] private TextMeshProUGUI valueLabel;
    [SerializeField] private string valueFormat = "{0:F0} V";
    [SerializeField] private UnityEvent<float> onValueChanged;

    public float CurrentValue { get; private set; }

    private bool _hasTrackPoke;
    private bool _hasHandle;
    private bool _hasHandleParent;
    private bool _hasLabel;
    private Vector3 _baseLocalPos;
    private Transform _handleParent;

    private void Awake()
    {
        if (trackPokeInteractable == null)
        {
            trackPokeInteractable = GetComponentInChildren<PokeInteractable>();
        }

        if (handleTransform == null)
        {
            handleTransform = transform;
        }

        _hasTrackPoke = trackPokeInteractable != null;
        _hasHandle = handleTransform != null;
        _hasLabel = valueLabel != null;

        if (_hasHandle)
        {
            _baseLocalPos = handleTransform.localPosition;
            _handleParent = handleTransform.parent;
            _hasHandleParent = _handleParent != null;
        }
    }

    private void Start()
    {
        if (!_hasHandle)
        {
            return;
        }

        float clampedT = Mathf.Clamp01(initialNormalizedValue);
        Vector3 pos = _baseLocalPos;
        pos.x = Mathf.Lerp(minLocalX, maxLocalX, clampedT);
        handleTransform.localPosition = pos;

        CurrentValue = Mathf.Lerp(minValue, maxValue, clampedT);
        UpdateLabel();
        onValueChanged?.Invoke(CurrentValue);
    }

    private void Update()
    {
        if (!_hasHandle)
        {
            return;
        }

        if (_hasTrackPoke)
        {
            foreach (PokeInteractor interactor in trackPokeInteractable.SelectingInteractors)
            {
                Vector3 targetLocal = _hasHandleParent
                    ? _handleParent.InverseTransformPoint(interactor.Origin)
                    : interactor.Origin;

                Vector3 pos = _baseLocalPos;
                pos.x = Mathf.Clamp(targetLocal.x, minLocalX, maxLocalX);
                handleTransform.localPosition = pos;
                break;
            }
        }

        Vector3 clampedPos = _baseLocalPos;
        clampedPos.x = Mathf.Clamp(handleTransform.localPosition.x, minLocalX, maxLocalX);
        handleTransform.localPosition = clampedPos;

        float t = Mathf.InverseLerp(minLocalX, maxLocalX, clampedPos.x);
        float newValue = Mathf.Lerp(minValue, maxValue, t);

        if (Mathf.Abs(newValue - CurrentValue) > 0.05f)
        {
            CurrentValue = newValue;
            UpdateLabel();
            onValueChanged?.Invoke(CurrentValue);
        }
    }

    private void UpdateLabel()
    {
        if (_hasLabel)
        {
            valueLabel.text = string.Format(valueFormat, CurrentValue);
        }
    }
}
