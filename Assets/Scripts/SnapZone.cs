using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class SnapZone : MonoBehaviour
{
    [Header("Точка фіксації")]
    public Transform snapPoint;

    [Header("Перевірка деталі")]
    [SerializeField] private GameObject requiredPart;

    [Header("Кольори")]
    public Color hoverColor = Color.yellow;
    public Color snappedColor = Color.cyan;
    public Color wrongPartColor = Color.red;

    [Header("Події")]
    [SerializeField] private UnityEvent onSnapped;
    [SerializeField] private UnityEvent onUnsnapped;

    public bool IsSnapped => _isSnapped;

    private Material _mat;
    private bool _hasMaterial;
    private Color _originalColor;

    private Grabbable _currentGrabbable;
    private Grabbable _wrongGrabbable;
    private Rigidbody _currentRigidbody;
    private bool _isSnapped;
    private bool _isInsideZone;

    private void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null && rend.material != null)
        {
            _mat = rend.material;
            _hasMaterial = true;
            _originalColor = _mat.color;
        }

        if (snapPoint == null)
        {
            snapPoint = transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Grabbable grabbable = other.GetComponentInParent<Grabbable>();
        if (grabbable == null) return;

        if (requiredPart != null && grabbable.gameObject != requiredPart)
        {
            if (!_isSnapped)
            {
                _wrongGrabbable = grabbable;
                SetColor(wrongPartColor);
            }
            return;
        }

        _isInsideZone = true;

        if (_isSnapped) return;

        _currentGrabbable = grabbable;
        _currentRigidbody = grabbable.GetComponent<Rigidbody>();

        _currentGrabbable.WhenPointerEventRaised -= OnPointerEvent;
        _currentGrabbable.WhenPointerEventRaised += OnPointerEvent;

        if (_currentGrabbable.SelectingPointsCount == 0)
        {
            SnapObject();
            return;
        }

        SetColor(hoverColor);
    }

    private void OnTriggerExit(Collider other)
    {
        Grabbable grabbable = other.GetComponentInParent<Grabbable>();
        if (grabbable == null) return;

        if (grabbable == _wrongGrabbable)
        {
            _wrongGrabbable = null;
            if (!_isSnapped)
            {
                SetColor(_isInsideZone ? hoverColor : _originalColor);
            }
            return;
        }

        if (grabbable == _currentGrabbable)
        {
            _isInsideZone = false;

            if (!_isSnapped)
            {
                ReleaseCurrent();
            }
        }
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Unselect && !_isSnapped && _isInsideZone)
        {
            SnapObject();
        }
        else if (evt.Type == PointerEventType.Select && _isSnapped)
        {
            UnsnapObject();
        }
    }

    private void SnapObject()
    {
        _isSnapped = true;

        if (_currentRigidbody != null)
        {
            _currentRigidbody.linearVelocity = Vector3.zero;
            _currentRigidbody.angularVelocity = Vector3.zero;
            _currentRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        if (_currentGrabbable != null)
        {
            _currentGrabbable.transform.position = snapPoint.position;
            _currentGrabbable.transform.rotation = snapPoint.rotation;
        }

        SetColor(snappedColor);
        onSnapped?.Invoke();
    }

    private void UnsnapObject()
    {
        _isSnapped = false;

        if (_currentRigidbody != null)
        {
            _currentRigidbody.constraints = RigidbodyConstraints.None;
        }

        SetColor(hoverColor);
        onUnsnapped?.Invoke();
    }

    private void ReleaseCurrent()
    {
        if (_currentGrabbable != null)
        {
            _currentGrabbable.WhenPointerEventRaised -= OnPointerEvent;
            _currentGrabbable = null;
        }
        _currentRigidbody = null;
        SetColor(_originalColor);
    }

    private void SetColor(Color color)
    {
        if (_hasMaterial)
        {
            _mat.color = color;
        }
    }

    private void OnDisable()
    {
        ReleaseCurrent();
    }
}