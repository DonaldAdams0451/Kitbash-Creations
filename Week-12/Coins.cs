using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinDriftDrop : MonoBehaviour
{
    [Header("Coin Movement Settings")]
    public float fallSpeed = 2f;                 // How fast the coin falls
    public float spawnOffsetY = 1f;              // How far above the screen it starts
    public float xSpawnRange = 5f;               // Range of horizontal spawn positions
    public float stopY = -5f;                    // How deep it falls before vanishing

    [Header("Drift Settings")]
    public float driftSpeed = 1f;                // Max sideways movement speed
    public float driftChangeInterval = 2f;       // Time between drift direction changes
    public float driftSmoothTime = 1f;           // How long it takes to ease into new direction

    [Header("Score Settings")]
    public int coinValue = 10;                   // Points awarded for collecting the coin

    private float targetDriftDirection = 0f;     // Desired drift direction (-1, 0, or 1)
    private float currentDriftDirection = 0f;    // Current sideways movement direction
    private float driftVelocity = 0f;            // Used by SmoothDamp for easing

    private GameManager gameManager;             // Reference to GameManager

    void Start()
    {
        // Find GameManager in scene
        gameManager = FindObjectOfType<GameManager>();

        // Determine top of screen in world coordinates
        float topY = Camera.main.orthographicSize + spawnOffsetY;

        // Random horizontal spawn position
        float randomX = Random.Range(-xSpawnRange, xSpawnRange);

        // Start coin slightly above the screen
        transform.position = new Vector3(randomX, topY, 0f);

        // Begin drift coroutine
        StartCoroutine(ChangeDriftDirection());
    }

    void Update()
    {
        // Smoothly transition between drift directions
        currentDriftDirection = Mathf.SmoothDamp(
            currentDriftDirection,
            targetDriftDirection,
            ref driftVelocity,
            driftSmoothTime
        );

        // Combine downward and sideways movement
        Vector3 move = Vector3.down * fallSpeed * Time.deltaTime;
        move += Vector3.right * currentDriftDirection * driftSpeed * Time.deltaTime;
        transform.Translate(move);

        // Destroy when coin falls below stopY
        if (transform.position.y <= stopY)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator ChangeDriftDirection()
    {
        while (true)
        {
            // Randomly choose drift direction: left, right, or none
            int choice = Random.Range(0, 3);
            if (choice == 0) targetDriftDirection = -1f;
            else if (choice == 1) targetDriftDirection = 1f;
            else targetDriftDirection = 0f;

            yield return new WaitForSeconds(driftChangeInterval);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detect player collection
        if (other.CompareTag("Player"))
        {
            // Add score via GameManager
            if (gameManager != null)
            {
                gameManager.AddScore(coinValue);
            }

            // Destroy the coin after collection
            Destroy(gameObject);
        }
    }
}
