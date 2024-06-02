using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PowerUpSpawner : MonoBehaviour
{
    public GameObject powerUpPrefab;
    public float minSpawnInterval = 30f;
    public float maxSpawnInterval = 120f;
    public float powerUpDuration = 10f;

    private void Start()
    {
        StartCoroutine(SpawnPowerUps());
    }

    private IEnumerator SpawnPowerUps()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));

            Vector3 randomPosition = GetRandomPosition();
            GameObject powerUp = Instantiate(powerUpPrefab, randomPosition, Quaternion.identity);

            PowerUp powerUpScript = powerUp.GetComponent<PowerUp>();
            powerUpScript.type = (PowerUp.PowerUpType)Random.Range(0, 2);

            yield return new WaitForSeconds(powerUpDuration);

            if (powerUp != null)
            {
                Destroy(powerUp);
            }
        }
    }

    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(-10f, 10f);
        float y = Random.Range(-5f, 5f);
        return new Vector3(x, y, 0f);
    }
}