using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointFollower : MonoBehaviour
{
    [SerializeField] private GameObject[] waypoints;
    public int currentWaypointIndex = 0;
    [SerializeField] private float speed = 2f;

    private float effectiveSpeed;

    private void Start()
    {
        // Apply difficulty multiplier for enemies
        if (SettingsManager.Instance != null)
        {
            effectiveSpeed = speed * SettingsManager.Instance.GetEnemySpeedMultiplier();
        }
        else
        {
            effectiveSpeed = speed;
        }
    }

    private void Update()
    {
        if (Vector2.Distance(waypoints[currentWaypointIndex].transform.position, transform.position) < .1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
            currentWaypointIndex = 0;
            }
        }
        transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypointIndex].transform.position, effectiveSpeed * Time.deltaTime);

    }
}