using UnityEngine;

public class BackgroundSlider : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector2 moveSpeed = new Vector2(0.5f, 0.3f);
    [SerializeField] private Vector2 movementBounds = new Vector2(2f, 1.5f);
    [SerializeField] private bool randomTeleport = true;

    [Header("Speed Randomization")]
    [SerializeField] private float minSpeed = 0.3f;
    [SerializeField] private float maxSpeed = 0.8f;

    private Vector3 startPosition;
    private Vector2 currentDirection;
    private float currentSpeed;

    private void Start()
    {
        startPosition = transform.position;
        RandomizeMovement();
    }

    private void Update()
    {
        // Move background
        Vector3 movement = new Vector3(currentDirection.x, currentDirection.y, 0f) * currentSpeed * Time.deltaTime;
        transform.position += movement;

        // Check boundaries
        Vector3 offset = transform.position - startPosition;
        bool hitBoundary = false;

        // Check X bounds
        if (Mathf.Abs(offset.x) > movementBounds.x)
        {
            hitBoundary = true;

            if (randomTeleport)
            {
                // Teleport to random position
                offset.x = Random.Range(-movementBounds.x * 0.5f, movementBounds.x * 0.5f);
            }
            else
            {
                // Bounce back
                currentDirection.x = -currentDirection.x;
                offset.x = Mathf.Clamp(offset.x, -movementBounds.x, movementBounds.x);
            }
        }

        // Check Y bounds
        if (Mathf.Abs(offset.y) > movementBounds.y)
        {
            hitBoundary = true;

            if (randomTeleport)
            {
                // Teleport to random position
                offset.y = Random.Range(-movementBounds.y * 0.5f, movementBounds.y * 0.5f);
            }
            else
            {
                // Bounce back
                currentDirection.y = -currentDirection.y;
                offset.y = Mathf.Clamp(offset.y, -movementBounds.y, movementBounds.y);
            }
        }

        // Apply new position
        transform.position = startPosition + offset;

        // If hit boundary, randomize movement
        if (hitBoundary && randomTeleport)
        {
            RandomizeMovement();
        }
    }

    private void RandomizeMovement()
    {
        // Random direction
        float angle = Random.Range(0f, 360f);
        currentDirection = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ).normalized;

        // Random speed
        currentSpeed = Random.Range(minSpeed, maxSpeed);
    }

    // Visualize bounds in Scene view
    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, new Vector3(movementBounds.x * 2f, movementBounds.y * 2f, 0.1f));
    }
}