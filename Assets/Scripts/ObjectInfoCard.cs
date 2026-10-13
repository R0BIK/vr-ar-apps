using UnityEngine;
using TMPro;
using Oculus.Interaction;

public class ObjectInfoCard : MonoBehaviour
{
    [SerializeField] private RayInteractable rayInteractable;
    [SerializeField] private Canvas infoCanvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private string itemName = "Імʼя";
    [TextArea(2, 4)]
    [SerializeField] private string itemDescription = "Інформація";
    [SerializeField] private bool faceCamera = true;

    private Transform _cameraTransform;
    private bool _hasCamera;
    private bool _hasCanvas;
    private bool _hasCanvasGroup;
    private IInteractableView _interactableView;
    private bool _hasInteractable;

    private void Awake()
    {
        _hasCanvas = infoCanvas != null;
        _hasCanvasGroup = canvasGroup != null;

        var mainCam = Camera.main;
        _hasCamera = mainCam != null;
        if (_hasCamera)
        {
            _cameraTransform = mainCam.transform;
        }

        if (rayInteractable != null)
        {
            _interactableView = rayInteractable;
        }
        else
        {
            _interactableView = GetComponentInParent<RayInteractable>() ?? GetComponentInChildren<RayInteractable>() ?? GetComponent<RayInteractable>() ?? GetComponent<IInteractableView>() ?? GetComponentInParent<IInteractableView>();
        }

        _hasInteractable = _interactableView != null;

        if (titleText != null)
        {
            titleText.text = itemName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = itemDescription;
        }

        SetCardVisible(false);
    }

    private void Start()
    {
        if (!_hasCamera)
        {
            var mainCam = Camera.main;
            _hasCamera = mainCam != null;
            if (_hasCamera)
            {
                _cameraTransform = mainCam.transform;
            }
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

    private void LateUpdate()
    {
        if (faceCamera && _hasCamera && _hasCanvas && infoCanvas.enabled)
        {
            infoCanvas.transform.rotation = Quaternion.LookRotation(infoCanvas.transform.position - _cameraTransform.position);
        }
    }

    private void HandleStateChanged(InteractableStateChangeArgs args)
    {
        bool isHoveredOrSelected = args.NewState == InteractableState.Hover || args.NewState == InteractableState.Select;
        SetCardVisible(isHoveredOrSelected);
    }

    public void ShowCard() => SetCardVisible(true);
    public void HideCard() => SetCardVisible(false);

    public void SetCardVisible(bool visible)
    {
        if (_hasCanvas)
        {
            infoCanvas.enabled = visible;
        }
        else if (_hasCanvasGroup)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}
