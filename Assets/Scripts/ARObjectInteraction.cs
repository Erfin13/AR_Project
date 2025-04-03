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
    private Vector2 initialTouch0Position;
    private Vector2 initialTouch1Position;
    private Vector3 initialObjectPosition;
    private float rotationSpeed = 100f;
    private float scaleSmoothness = 10f;

    void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    void Start()
    {
        objectTransform = GameObject.Find("drone_costum")?.transform;

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
        else if (activeTouches.Count == 2) // 兩指縮放與移動
        {
            var touch0 = activeTouches[0];
            var touch1 = activeTouches[1];

            float currentDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);

            // 處理縮放邏輯
            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                initialDistance = currentDistance;
                initialScale = objectTransform.localScale;

                initialTouch0Position = touch0.screenPosition;
                initialTouch1Position = touch1.screenPosition;

                initialObjectPosition = objectTransform.position; // 儲存初始位置
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                // 縮放
                if (initialDistance > 1e-5f)
                {
                    float scaleFactor = Mathf.Clamp(currentDistance / initialDistance, 0.5f, 2f);
                    objectTransform.localScale = Vector3.Lerp(objectTransform.localScale, initialScale * scaleFactor, Time.deltaTime * scaleSmoothness);
                }

                // 移動物件
                Vector2 currentTouch0Position = touch0.screenPosition;
                Vector2 currentTouch1Position = touch1.screenPosition;

                Vector2 touchDelta = (currentTouch0Position + currentTouch1Position) / 2 - (initialTouch0Position + initialTouch1Position) / 2;
                Vector3 move = new Vector3(touchDelta.x * 0.01f, 0, touchDelta.y * 0.01f);  // 計算移動方向

                objectTransform.position = initialObjectPosition + move; // 移動物件並更新位置
            }
        }
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }
}
