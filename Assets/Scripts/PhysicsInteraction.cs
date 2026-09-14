using System.Collections;
using UnityEngine;

public class PhysicsInteraction : MonoBehaviour
{
    [Header("Налаштування підсвічування")]
    [Tooltip("Колір спалаху при фізичному ударі")]
    public Color hitFlashColor = Color.red;

    [Tooltip("Колір тригерної зони, коли гравець всередині")]
    public Color triggerActiveColor = Color.green;

    [Tooltip("Тривалість спалаху від удару (секунди)")]
    public float flashDuration = 0.5f;

    private Renderer _rend;
    private Color _originalColor;
    private Coroutine _flashRoutine;

    private void Start()
    {
        _rend = GetComponent<Renderer>();
        if (_rend != null && _rend.material != null)
        {
            _originalColor = _rend.material.color;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 0.5f)
        {
            Debug.Log($"[OnCollisionEnter] '{gameObject.name}' отримав удар від '{collision.gameObject.name}' із силою {collision.relativeVelocity.magnitude:F1}");

            if (_rend != null)
            {
                if (_flashRoutine != null) StopCoroutine(_flashRoutine);
                _flashRoutine = StartCoroutine(FlashColor());
            }
        }
    }

    private IEnumerator FlashColor()
    {
        _rend.material.color = hitFlashColor;
        yield return new WaitForSeconds(flashDuration);
        _rend.material.color = _originalColor;
        _flashRoutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[OnTriggerEnter] '{other.gameObject.name}' увійшов у зону '{gameObject.name}'!");

        if (_rend != null)
        {
            _rend.material.color = triggerActiveColor;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[OnTriggerExit] '{other.gameObject.name}' вийшов із зони '{gameObject.name}'!");

        if (_rend != null)
        {
            _rend.material.color = _originalColor;
        }
    }
}