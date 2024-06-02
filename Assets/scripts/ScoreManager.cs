using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text scoreText; // Reference to the Text component displaying the score
    private int currentScore;

    private void Start()
    {
        currentScore = 0;
        UpdateScoreDisplay();
    }

    public void IncreaseScore(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
    }

    public void DecreaseScore(int points)
    {
        currentScore -= points;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        scoreText.text = "Score: " + currentScore.ToString();
    }
}
