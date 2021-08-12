using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimLook : MonoBehaviour
{
    [SerializeField] float mouseSenstivity = 100f;
    [SerializeField] Transform player;

    float xRotation = 0f;
    Vector2 mouseInput;
    public Vector2 MouseInput { set{ mouseInput = value; } }

    private void Update()
    {
        float mouseX = mouseInput.x * mouseSenstivity * Time.deltaTime;
        float mouseY = mouseInput.y * mouseSenstivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        player.Rotate(Vector3.up * mouseX);
    }
}
