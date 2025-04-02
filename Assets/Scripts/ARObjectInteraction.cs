using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class ARObjectInteraction : MonoBehaviour
{
    private Transform objectTransform;
    private float initialDistance;
    private Vector3 initialScale;
    private float rotationSpeed = 100f;
    private float scaleSmoothness = 10f;

    void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    void Start()
    {
        objectTransform = gameObject.transform.parent != null ? gameObject.transform.parent : gameObject.transform;

        if (objectTransform == null)
        {
            Debug.LogError("🚨 ARObjectInteraction is not attached to any object!");
        }

        Debug.Log("🟢 AR Session State: " + ARSession.state);
        if (ARSession.state < ARSessionState.SessionTracking)
        {
            Debug.LogWarning("⚠️ AR Session has not fully started. Please check the AR setup!");
        }

        if (gameObject.GetComponent<ARAnchor>() == null)
        {
            Debug.LogWarning("⚠️ No AR Anchor found on this object. Consider adding one!");
        }
    }

    void Update()
    {
        var activeTouches = Touch.activeTouches;
        Debug.Log("🖐 Active touch count: " + activeTouches.Count);

        if (activeTouches.Count == 1) // Single finger rotation
        {
            var touch = activeTouches[0];
            Debug.Log("📌 Single touch DeltaX: " + touch.delta.x);

            if (touch.phase == TouchPhase.Moved)
            {
                Vector3 rotation = objectTransform.localEulerAngles;
                rotation.y -= touch.delta.x * rotationSpeed * Time.deltaTime;
                objectTransform.localEulerAngles = rotation;
            }
        }
        else if (activeTouches.Count == 2) // Two-finger scaling
        {
            var touch0 = activeTouches[0];
            var touch1 = activeTouches[1];

            float currentDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);
            Debug.Log($"📏 Current two-finger distance: {currentDistance}");

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                initialDistance = currentDistance > 1e-5f ? currentDistance : initialDistance;
                initialScale = objectTransform.localScale;
                Debug.Log($"🎯 Initial distance recorded: {initialDistance}, Initial scale: {initialScale}");
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                if (initialDistance > 1e-5f)
                {
                    float scaleFactor = Mathf.Clamp(currentDistance / initialDistance, 0.5f, 2f);
                    Vector3 targetScale = initialScale * scaleFactor;
                    Debug.Log($"🔍 Target scale: {targetScale}");
                    objectTransform.localScale = Vector3.Lerp(objectTransform.localScale, targetScale, Time.deltaTime * scaleSmoothness);
                }
            }
        }
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }
}
