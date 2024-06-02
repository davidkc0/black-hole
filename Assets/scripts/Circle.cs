using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    public BlackHoleController blackHole;
    public GameObject circlePrefab;
    public float attractionSpeed = 1.0f;
    public float colorAttractionFactor = 0.5f;
    public float colorAttractionRadius = 5.0f;
    public float CreationTime { get; private set; }

    private SpriteRenderer spriteRenderer;
    private List<Circle> circles;
    private Vector3 initialScale;
    private Color[] possibleColors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow
    };

    private Color originalColor; // Store the original color of the circle
    private bool isFrozen; // Track if the circle is frozen

    public Color color
    {
        get
        {
            if (spriteRenderer == null)
            {
                return Color.clear;
            }
            return spriteRenderer.color;
        }
        set
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = value;
            }
        }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        blackHole = FindObjectOfType<BlackHoleController>();
        ChangeColor();
        initialScale = transform.localScale;
        originalColor = color; // Store the original color

        float randomScale = Random.Range(0.5f, 2.0f);
        transform.localScale = initialScale * randomScale;
    }

    private void Start()
    {
        circles = new List<Circle>(FindObjectsOfType<Circle>());
        circles.Remove(this);
    }

    private void Update()
    {
        if (!isFrozen) // Only move if not frozen
        {
            Vector3 directionToBlackHole = (blackHole.transform.position - transform.position).normalized;
            transform.position += directionToBlackHole * attractionSpeed * Time.deltaTime;

            foreach (var otherCircle in circles)
            {
                if (otherCircle.color == this.color)
                {
                    float distanceToOtherCircle = Vector3.Distance(transform.position, otherCircle.transform.position);

                    if (distanceToOtherCircle < colorAttractionRadius)
                    {
                        Vector3 directionToOtherCircle = (otherCircle.transform.position - transform.position).normalized;
                        transform.position += directionToOtherCircle * colorAttractionFactor * Time.deltaTime;

                        if (distanceToOtherCircle <= Mathf.Epsilon)
                        {
                            BreakIntoSmallerCircles();
                            otherCircle.BreakIntoSmallerCircles();
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Circle otherCircle = other.GetComponent<Circle>();
        if (otherCircle != null && otherCircle != this)
        {
            Debug.Log("Collision detected between two circles");
            BreakIntoSmallerCircles();
            otherCircle.BreakIntoSmallerCircles();
        }
    }

    public void BreakIntoSmallerCircles()
    {
        Debug.Log("BreakIntoSmallerCircles called");

        int numSmallerCircles = 4;
        for (int i = 0; i < numSmallerCircles; i++)
        {
            Debug.Log("Creating new circle");

            GameObject newCircleObj = Instantiate(circlePrefab, transform.position, Quaternion.identity);

            if (newCircleObj == null)
            {
                Debug.Log("New circle object is null");
                continue;
            }

            Circle newCircle = newCircleObj.GetComponent<Circle>();

            if (newCircle == null)
            {
                Debug.Log("New circle script is null");
                continue;
            }

            newCircle.circles = circles;

            newCircle.transform.localScale = transform.localScale / 2;
            newCircle.color = this.color;

            Rigidbody2D rb = newCircleObj.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.Log("Rigidbody2D on new circle is null");
            }
            else
            {
                Vector2 forceDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                rb.AddForce(forceDirection * 5f, ForceMode2D.Impulse);
            }
        }

        Debug.Log("Destroying original circle");
        Destroy(gameObject);
    }

    private void ChangeColor()
    {
        int randomColorIndex = Random.Range(0, possibleColors.Length);
        spriteRenderer.color = possibleColors[randomColorIndex];
    }

    public void ChangeColor(Color newColor) // New method to change color
    {
        color = newColor;
    }

    public void ResetColor() // New method to reset color
    {
        color = originalColor;
    }

    public void Freeze() // New method to freeze the circle
    {
        isFrozen = true;
    }

    public void Unfreeze() // New method to unfreeze the circle
    {
        isFrozen = false;
    }
}