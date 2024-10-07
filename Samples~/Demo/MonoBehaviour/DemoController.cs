using UnityEngine;
using ScriptableObjectArchitecture;
using System.Collections;

[DisallowMultipleComponent]
public class DemoController : MonoBehaviour
{
    public Vector2Variable moveDirection;
    public GenericReference<float, FloatVariable> moveSpeed = new GenericReference<float, FloatVariable>();
    private Rigidbody2D rb;
    private Vector2 movement;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        moveDirection.Subscribe(this.gameObject, OnMoveEvent);
    }

    private void OnDisable()
    {
        moveDirection.UnSubscribe(this.gameObject, OnMoveEvent);
    }

    private void OnMoveEvent(Vector2 moveInput)
    {
        movement = moveInput;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
