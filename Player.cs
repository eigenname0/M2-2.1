using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float mouseSensitivity = 3f;
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float gravity = -9.81f;

    [Header("Combat Settings")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] float attackCooldown = 0.5f;
    [SerializeField] int attackDamage = 25;

    [Header("References")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform swordTransform; 
    //[SerializeField] TrailRenderer swordTrail;


    CharacterController controller;
    Vector2 look;
    Vector3 velocity;
    float nextAttackTime;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleAttack();
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1f);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
            velocity.y = jumpForce;

        velocity.y += gravity * Time.deltaTime;

        controller.Move((move * movementSpeed + velocity) * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        look.x += mouseX;
        look.y -= mouseY;
        look.y = Mathf.Clamp(look.y, -89f, 89f);

        transform.localRotation = Quaternion.Euler(0, look.x, 0);
        cameraTransform.localRotation = Quaternion.Euler(look.y, 0, 0);
    }

    void HandleAttack()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            StartCoroutine(SwordSwing()); // visual swing
            Debug.Log("Slash!");

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, attackRange))
            {
                Debug.Log("Hit " + hit.collider.name);
                var health = hit.collider.GetComponentInParent<EnemyHealth>();
                if (health != null)
                    health.TakeDamage(attackDamage);
            }
        }
    }

    IEnumerator SwordSwing()
    {
        if (swordTransform == null) yield break;

        //if (swordTrail != null)
        //    swordTrail.emitting = true;

        Quaternion startRot = swordTransform.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(60f, 0f, 0f); // swing down

        float t = 0f;
        float speed = 10f; 

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            swordTransform.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            swordTransform.localRotation = Quaternion.Slerp(endRot, startRot, t);
            yield return null;
        }

        //if (swordTrail != null)
        //{
        //    swordTrail.emitting = false;
        //    swordTrail.Clear();
        //}
    }

}
