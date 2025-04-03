using UnityEngine;

public class DepthMaskController : MonoBehaviour
{
    public Transform cameraTransform;  // AR 相機
    public GameObject depthMaskCylinder;  // 拖入你的 Cylinder 遮罩
    public float activationDistance = 0.8f;  // 距離閾值

    void Start()
    {
        if (depthMaskCylinder == null)
        {
            Debug.LogError("Depth Mask Cylinder 未設定！");
        }
    }

    void Update()
    {
        float distanceToCamera = Vector3.Distance(cameraTransform.position, transform.position);

        // 當無人機距離相機太近時，啟動遮罩
        if (distanceToCamera < activationDistance)
        {
            depthMaskCylinder.SetActive(true);
        }
        else
        {
            depthMaskCylinder.SetActive(false);
        }
    }
}
