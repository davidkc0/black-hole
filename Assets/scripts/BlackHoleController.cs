using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleController : MonoBehaviour
{
    public SpriteRenderer halo;
    public float growthFactor;
    public float slowdownFactor;
    public float speedupFactor;
    public float minimumSpeed;
    public float maximumSpeed;
    public float colorChangeInterval;
    public GameManagerScript gameManager;
    public ScoreManager scoreManager;

    public float attractionSpeed;
    public float powerUpDuration = 10f;
    public bool isChangeColorActive;
    public bool isFreezeActive;

    private float colorChangeTimer;
    private bool isDead;
    private Vector3 initialScale;
    private Color[] possibleColors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow
    };
    private float changeColorTimer;
    private float freezeTimer;

    private void Start()
    {
        colorChangeTimer = colorChangeInterval;
        initialScale = transform.localScale;
        ChangeHaloColor();
    }

    private void Update()
    {
        HandleMovement();
        HandleColorChange();
        HandlePowerUps();
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = transform.position;

        if (Input.GetMouseButton(0))
        {
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition.z = transform.position.z;
        }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        targetPosition += new Vector3(moveHorizontal, moveVertical, 0) * attractionSpeed * Time.deltaTime;

        transform.position = Vector3.Lerp(transform.position, targetPosition, attractionSpeed * Time.deltaTime);
    }

    private void HandleColorChange()
    {
        colorChangeTimer -= Time.deltaTime;

        if (colorChangeTimer <= 0)
        {
            ChangeHaloColor();
            colorChangeTimer = colorChangeInterval;
        }
    }

    private void ChangeHaloColor()
    {
        int randomColorIndex = Random.Range(0, possibleColors.Length);
        halo.color = possibleColors[randomColorIndex];
    }

    private void ResetBlackHole()
    {
        transform.localScale = initialScale;
        slowdownFactor = 1;
    }

    private void HandlePowerUps()
    {
        if (isChangeColorActive)
        {
            changeColorTimer -= Time.deltaTime;
            if (changeColorTimer <= 0)
            {
                isChangeColorActive = false;
                ResetCircleColors();
            }
        }

        if (isFreezeActive)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0)
            {
                isFreezeActive = false;
                UnfreezeCircles();
            }
        }
    }

    private void ResetCircleColors()
    {
        Circle[] circles = FindObjectsOfType<Circle>();
        foreach (Circle circle in circles)
        {
            circle.ResetColor();
        }
    }

    private void UnfreezeCircles()
    {
        Circle[] circles = FindObjectsOfType<Circle>();
        foreach (Circle circle in circles)
        {
            circle.Unfreeze();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Circle circle = other.GetComponent<Circle>();
        if (circle != null)
        {
            if (circle.color == halo.color)
            {
                transform.localScale *= growthFactor;
                scoreManager.IncreaseScore(1);
            }
            else
            {
                transform.localScale -= Vector3.one * slowdownFactor;
                scoreManager.DecreaseScore(1);
                if (transform.localScale.x < initialScale.x / 3 && !isDead)
                {
                    isDead = true;
                    gameManager.gameOver();
                    Debug.Log("Dead");
                }
            }

            attractionSpeed = Mathf.Clamp(attractionSpeed, minimumSpeed, maximumSpeed);

            Destroy(circle.gameObject);
        }
        
        PowerUp powerUp = other.GetComponent<PowerUp>();
        if (powerUp != null)
        {
            if (powerUp.type == PowerUp.PowerUpType.ChangeColor)
            {
                isChangeColorActive = true;
                changeColorTimer = powerUpDuration;
                ChangeCircleColors();
            }
            else if (powerUp.type == PowerUp.PowerUpType.Freeze)
            {
                isFreezeActive = true;
                freezeTimer = powerUpDuration;
                FreezeCircles();
            }
            Destroy(powerUp.gameObject);
        }
    }

    private void ChangeCircleColors()
    {
        Circle[] circles = FindObjectsOfType<Circle>();
        foreach (Circle circle in circles)
        {
            circle.ChangeColor(halo.color);
        }
    }

    private void FreezeCircles()
    {
        Circle[] circles = FindObjectsOfType<Circle>();
        foreach (Circle circle in circles)
        {
            circle.Freeze();
        }
    }
}