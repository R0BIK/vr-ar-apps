using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class PhysicalPokeButton : MonoBehaviour
{
    [SerializeField] private PokeInteractable pokeInteractable;
    [SerializeField] private Transform buttonCap;
    [SerializeField] private Vector3 pressedLocalOffset = new Vector3(0f, -0.015f, 0f);
    [SerializeField] private float moveSpeed = 16f;
    [SerializeField] private Renderer capRenderer;
    [SerializeField] private Color normalColor = new Color(0.2f, 0.55f, 0.9f, 1f);
    [SerializeField] private Color hoverColor = new Color(0.35f, 0.75f, 1f, 1f);
    [SerializeField] private Color pressedColor = new Color(0.15f, 0.9f, 0.35f, 1f);
    [SerializeField] private UnityEvent onPressed;

    private IInteractableView _interactableView;
    private bool _hasInteractable;
    private bool _hasCap;
    private bool _hasMaterial;
    private Material _capMaterial;
    private Vector3 _initialLocalPos;
    private bool _isPressed;

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
        _hasCap = buttonCap != null;
        if (_hasCap)
        {
            _initialLocalPos = buttonCap.localPosition;
        }

        if (capRenderer != null && capRenderer.material != null)
        {
            _capMaterial = capRenderer.material;
            _hasMaterial = true;
            _capMaterial.color = normalColor;
        }
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
        if (!_hasCap)
        {
            return;
        }

        Vector3 targetPos = _isPressed ? _initialLocalPos + pressedLocalOffset : _initialLocalPos;
        buttonCap.localPosition = Vector3.Lerp(buttonCap.localPosition, targetPos, Time.deltaTime * moveSpeed);
    }

    private void HandleStateChanged(InteractableStateChangeArgs args)
    {
        _isPressed = args.NewState == InteractableState.Select;

        if (_hasMaterial)
        {
            if (args.NewState == InteractableState.Select)
            {
                _capMaterial.color = pressedColor;
            }
            else if (args.NewState == InteractableState.Hover)
            {
                _capMaterial.color = hoverColor;
            }
            else
            {
                _capMaterial.color = normalColor;
            }
        }

        if (args.NewState == InteractableState.Select && args.PreviousState != InteractableState.Select)
        {
            onPressed?.Invoke();
        }
    }
}
