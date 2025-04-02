using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class ARObjectInteraction : MonoBehaviour
{
    private Transform objectTransform;
    private float initialDistance;
    private Vector3 initialScale;
    private float rotationSpeed = 0.2f;  // 降低速度，減少旋轉過快問題
    private float scaleSmoothness = 10f; // 增加平滑度，確保縮放順暢

    void Start()
    {
        objectTransform = transform;
        EnhancedTouchSupport.Enable();
    }

    void Update()
    {
        var activeTouches = Touch.activeTouches;

        if (activeTouches.Count == 1) // 單指旋轉
        {
            var touch = activeTouches[0];
            if (touch.phase == TouchPhase.Moved)
            {
                // 使用 localEulerAngles 來更明顯影響旋轉
                Vector3 rotation = objectTransform.localEulerAngles;
                rotation.y -= touch.delta.x * rotationSpeed; // 左滑 = 逆時針旋轉
                objectTransform.localEulerAngles = rotation;
            }
        }
        else if (activeTouches.Count == 2) // 雙指縮放
        {
            var touch0 = activeTouches[0];
            var touch1 = activeTouches[1];

            float currentDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                initialDistance = currentDistance;
                initialScale = objectTransform.localScale;
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                if (initialDistance > 0) // 避免除以 0
                {
                    float scaleFactor = currentDistance / initialDistance;
                    Vector3 targetScale = initialScale * scaleFactor;
                    objectTransform.localScale = Vector3.Lerp(
                        objectTransform.localScale,
                        targetScale,
                        Time.deltaTime * scaleSmoothness
                    );
                }
            }
        }
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }
}
