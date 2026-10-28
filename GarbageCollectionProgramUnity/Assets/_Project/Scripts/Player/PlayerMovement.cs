using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("参考相机")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // 自动获取主相机
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // 获取 WASD 输入
        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            input.y += 1f;

        if (Keyboard.current.sKey.isPressed)
            input.y -= 1f;

        if (Keyboard.current.aKey.isPressed)
            input.x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            input.x += 1f;

        // 避免斜向移动更快
        input = Vector2.ClampMagnitude(input, 1f);

        // 根据当前相机方向计算移动方向
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();
        }

        Vector3 move =
            forward * input.y +
            right * input.x;

        // 让角色朝移动方向转身
        if (move.sqrMagnitude > 0.001f)
        {
            transform.forward = move;
        }

        // 执行移动
        controller.Move(
            move * moveSpeed * Time.deltaTime
        );
    }
}