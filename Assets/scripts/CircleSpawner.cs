using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CircleSpawner : MonoBehaviour
{
    public GameObject circlePrefab;
    public BlackHoleController blackHole;
    public int numCircles = 20; // Number of circles to spawn per area
    public float spawnAreaSize = 20f; // Radius of the area in which circles can be spawned
    public float spawnInterval = 0.5f; // Interval at which new circles are spawned
    public int minCirclesRequired = 5; // Minimum number of circles required in an area for new circles not to spawn

    private Vector3 lastSpawnPosition;
    private Vector3 lastBlackHolePosition;
    private List<GameObject> spawnedCircles = new List<GameObject>(); // To keep track of spawned circles

    private void Start()
    {
        if (blackHole == null)
        {
            Debug.LogError("BlackHoleController is not set on CircleSpawner.");
            return;
        }

        lastSpawnPosition = blackHole.transform.position;
        lastBlackHolePosition = blackHole.transform.position;

        SpawnCirclesAroundPosition(lastSpawnPosition);

        StartCoroutine(CheckAndSpawnCircles());
    }

    private void SpawnCirclesAroundPosition(Vector3 position)
    {
        for (int i = 0; i < numCircles; i++)
        {
            SpawnCircle(GetRandomPositionAround(position));
        }
    }

    private void SpawnCircle(Vector3 position)
    {
        // Instantiate a new circle
        GameObject newCircle = Instantiate(circlePrefab, position, Quaternion.identity);

        // Add the new circle to the list
        spawnedCircles.Add(newCircle);

        // Get the Circle script on the new circle
        Circle circleScript = newCircle.GetComponent<Circle>();

        // Set the blackHole field on the Circle script
        circleScript.blackHole = blackHole;
    }

    private Vector3 GetRandomPositionAround(Vector3 position)
    {
        // Get a random position within the spawn area
        float x = position.x + Random.Range(-spawnAreaSize / 2, spawnAreaSize / 2);
        float y = position.y + Random.Range(-spawnAreaSize / 2, spawnAreaSize / 2);

        return new Vector3(x, y, 0);
    }

    private IEnumerator CheckAndSpawnCircles()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Compute the movement direction of the black hole
            Vector3 moveDirection = (blackHole.transform.position - lastBlackHolePosition).normalized;

            // Update the last black hole position
            lastBlackHolePosition = blackHole.transform.position;

            // Look ahead of the black hole in the direction of movement
            Vector3 lookAheadPosition = blackHole.transform.position + moveDirection * spawnAreaSize;

            // Count the number of circles within the spawn area in the look ahead direction
            int circleCount = 0;
            foreach (GameObject circle in spawnedCircles)
            {
                if (circle != null && Vector3.Distance(circle.transform.position, lookAheadPosition) <= spawnAreaSize)
                {
                    circleCount++;
                }
            }

            // If there are not enough circles in the area, spawn new ones
            if (circleCount < minCirclesRequired)
            {
                SpawnCirclesAroundPosition(lookAheadPosition);
            }
        }
    }

    public void RemoveCircle(GameObject circle)
    {
        spawnedCircles.Remove(circle);
    }
}
