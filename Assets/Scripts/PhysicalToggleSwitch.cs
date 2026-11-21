using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class PhysicalToggleSwitch : MonoBehaviour
{
    [SerializeField] private PokeInteractable pokeInteractable;
    [SerializeField] private Transform leverTransform;
    [SerializeField] private Vector3 offEulerAngles = new Vector3(-30f, 0f, 0f);
    [SerializeField] private Vector3 onEulerAngles = new Vector3(30f, 0f, 0f);
    [SerializeField] private float switchSpeed = 14f;
    [SerializeField] private Renderer indicatorRenderer;
    [SerializeField] private Color offColor = new Color(0.85f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color onColor = new Color(0.2f, 0.9f, 0.35f, 1f);
    [SerializeField] private bool startOn;
    [SerializeField] private UnityEvent<bool> onToggled;
    [SerializeField] private UnityEvent onTurnedOn;
    [SerializeField] private UnityEvent onTurnedOff;

    public bool IsOn { get; private set; }

    private IInteractableView _interactableView;
    private bool _hasInteractable;
    private bool _hasLever;
    private bool _hasMaterial;
    private Material _indicatorMaterial;

    private void Awake()
    {
        if (pokeInteractable != null)
        {
            _interactableView = pokeInteractable;
        }
        else
        {
            _interactableView = GetComponentInChildren<PokeInteractable>()
                ?? GetComponent<PokeInteractable>()
                ?? GetComponentInParent<PokeInteractable>();
        }

        _hasInteractable = _interactableView != null;
        _hasLever = leverTransform != null;

        if (indicatorRenderer != null && indicatorRenderer.material != null)
        {
            _indicatorMaterial = indicatorRenderer.material;
            _hasMaterial = true;
        }

        IsOn = startOn;
        ApplyVisualStateImmediate();
    }

    private void OnEnable()
    {
        if (_hasInteractable)
        {
            _interactableView.WhenStateChanged += HandleStateChanged;
        }
    }

    private void OnDisable()
    {
        if (_hasInteractable)
        {
            _interactableView.WhenStateChanged -= HandleStateChanged;
        }
    }

    private void Update()
    {
        if (!_hasLever)
        {
            return;
        }

        Quaternion targetRot = Quaternion.Euler(IsOn ? onEulerAngles : offEulerAngles);
        leverTransform.localRotation = Quaternion.Lerp(leverTransform.localRotation, targetRot, Time.deltaTime * switchSpeed);
    }

    public void Toggle()
    {
        SetState(!IsOn);
    }

    public void SetState(bool value)
    {
        if (IsOn == value)
        {
            return;
        }

        IsOn = value;
        UpdateIndicatorColor();
        onToggled?.Invoke(IsOn);

        if (IsOn)
        {
            onTurnedOn?.Invoke();
        }
        else
        {
            onTurnedOff?.Invoke();
        }
    }

    private void HandleStateChanged(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Select && args.PreviousState != InteractableState.Select)
        {
            Toggle();
        }
    }

    private void ApplyVisualStateImmediate()
    {
        if (_hasLever)
        {
            leverTransform.localRotation = Quaternion.Euler(IsOn ? onEulerAngles : offEulerAngles);
        }

        UpdateIndicatorColor();
    }

    private void UpdateIndicatorColor()
    {
        if (_hasMaterial)
        {
            _indicatorMaterial.color = IsOn ? onColor : offColor;
        }
    }
}
