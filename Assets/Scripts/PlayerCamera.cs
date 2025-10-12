using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    private int maxAngle = 85;
    private int minAngle = -85;
    // [SerializeField] private Vector3 offset;
    private InputSystem controls;

    void Awake()
    {
        controls = new InputSystem();
    }
    void OnEnable()
    {
        controls.PlayerMovement.Enable();
    }

    void OnDisable()
    {
        controls.PlayerMovement.Disable();
    }
    void Start()
    {
        controls.PlayerMovement.Camera.performed += ctx =>
        {
            Vector2 value = ctx.ReadValue<Vector2>();
            // Debug.Log(value.x + " , " + value.y);
            
            transform.RotateAround(player.position, Vector3.up, value.x);
            transform.RotateAround(player.position, transform.right, -value.y);
        };
    }

    // Update is called once per frame
    void Update()
    {
        // transform.position = player.position + offset;
    }
}
