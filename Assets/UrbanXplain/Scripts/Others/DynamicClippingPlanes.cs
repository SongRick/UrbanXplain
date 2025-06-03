using UnityEngine;

public class DynamicClippingPlanes : MonoBehaviour
{
    public Transform playerTransform; // 将玩家的 Transform 拖拽到这里
    private Camera cam;

    [Header("Ground Settings")]
    public float groundNearPlane = 0.1f;
    public float groundFarPlane = 1000f;
    public float groundPlayerHeight = 10f; // 玩家在地面时的大致高度

    [Header("Air Settings")]
    public float airNearPlane = 10f;
    public float airFarPlane = 2000f;
    public float airMaxPlayerHeight = 200f; // 玩家达到这个高度时，完全应用空中设置

    [Header("Transition Smoothing")]
    public float transitionSmoothTime = 0.1f; // 平滑过渡的时间

    private float currentNearVelocity;
    private float currentFarVelocity;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("DynamicClippingPlanes script needs to be attached to a Camera object.");
            enabled = false;
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError("Player Transform not assigned in DynamicClippingPlanes script.");
            // 尝试自动查找玩家 (如果你的玩家有 "Player" 标签)
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                Debug.Log("Player Transform automatically found.");
            }
            else
            {
                enabled = false;
                return;
            }
        }
    }

    void Update()
    {
        if (playerTransform == null || cam == null) return;

        float playerY = playerTransform.position.y;

        // 计算插值因子 t (0 代表地面设置, 1 代表空中设置)
        // Mathf.InverseLerp: 如果 playerY 在 groundPlayerHeight, 返回 0; 如果在 airMaxPlayerHeight, 返回 1
        // Mathf.Clamp01: 确保 t 在 0 和 1 之间
        float t = Mathf.Clamp01(Mathf.InverseLerp(groundPlayerHeight, airMaxPlayerHeight, playerY));

        // 根据 t 在地面和空中设置之间进行插值
        float targetNear = Mathf.Lerp(groundNearPlane, airNearPlane, t);
        float targetFar = Mathf.Lerp(groundFarPlane, airFarPlane, t);

        // 平滑地更新相机的剪裁平面值
        cam.nearClipPlane = Mathf.SmoothDamp(cam.nearClipPlane, targetNear, ref currentNearVelocity, transitionSmoothTime);
        cam.farClipPlane = Mathf.SmoothDamp(cam.farClipPlane, targetFar, ref currentFarVelocity, transitionSmoothTime);

        // (可选) 调试输出，观察数值变化
         //Debug.Log($"PlayerY: {playerY:F2}, t: {t:F2}, Near: {cam.nearClipPlane:F2}, Far: {cam.farClipPlane:F2}");
    }
}