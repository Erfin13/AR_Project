using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation; // 確保 AR 功能
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class ARObjectInteraction : MonoBehaviour
{
    private Transform objectTransform;
    private float initialDistance;
    private Vector3 initialScale;
    private float rotationSpeed = 100f;  // 調整旋轉速度
    private float scaleSmoothness = 10f; // 增加縮放平滑度

    void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    void Start()
    {
        // 確保物件有 Transform
        objectTransform = gameObject.transform.parent != null ? gameObject.transform.parent : gameObject.transform;

        if (objectTransform == null)
        {
            Debug.LogError("🚨 ARObjectInteraction 未掛在物件上！");
        }

        // 檢查 AR Session 是否啟動
        Debug.Log("🟢 AR Session State: " + ARSession.state);
        if (ARSession.state < ARSessionState.SessionTracking)
        {
            Debug.LogWarning("⚠️ AR Session 尚未完全啟動，請檢查 AR 設置！");
        }

        // 檢查是否有 AR Anchor
        if (gameObject.GetComponent<ARAnchor>() == null)
        {
            Debug.LogWarning("⚠️ 物件沒有 AR Anchor，建議手動添加！");
        }
    }

    void Update()
    {
        var activeTouches = Touch.activeTouches;
        Debug.Log("🖐 目前觸控數量：" + activeTouches.Count);

        if (activeTouches.Count == 1) // 單指旋轉
        {
            var touch = activeTouches[0];
            Debug.Log("📌 單指觸控 DeltaX: " + touch.delta.x);

            if (touch.phase == TouchPhase.Moved)
            {
                Vector3 rotation = objectTransform.localEulerAngles;
                rotation.y -= touch.delta.x * rotationSpeed * Time.deltaTime;
                objectTransform.localEulerAngles = rotation;
            }
        }
        else if (activeTouches.Count == 2) // 雙指縮放
        {
            var touch0 = activeTouches[0];
            var touch1 = activeTouches[1];

            float currentDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);
            Debug.Log("📏 兩指距離：" + currentDistance);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                initialDistance = currentDistance;
                initialScale = objectTransform.localScale;
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                if (initialDistance > 1e-5f) // 避免 NaN
                {
                    float scaleFactor = Mathf.Clamp(currentDistance / initialDistance, 0.5f, 2f);
                    Vector3 targetScale = initialScale * scaleFactor;
                    Debug.Log("🔍 目標縮放：" + targetScale);
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