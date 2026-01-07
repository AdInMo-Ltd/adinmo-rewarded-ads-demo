using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonRandomPatrol : MonoBehaviour
{
    [Header("Waypoints")]
    [Tooltip("Drag your 4 points (A, B, C, D) here")]
    public Transform[] points; // Array to hold A, B, C, D

    [Header("Movement Settings")]
    [Tooltip("Speed in meters per second")]
    public float speed = 2.0f;
    
    [Tooltip("How close we need to get before picking a new target")]
    public float reachThreshold = 0.5f;

    [Header("Balloon Physics (Noise)")]
    public float turbulenceFrequency = 0.5f;
    public float turbulenceStrength = 2.0f;

    // Internal state
    private Vector3 currentStartPos;
    private Vector3 currentEndPos;
    private float journeyLength;
    private float startTime;
    private int currentTargetIndex = -1;

    void Start()
    {
        if (points.Length == 0) return;

        // Start physically at the first point
        transform.position = points[0].position;
        currentStartPos = points[0].position;

        // Pick the first random destination
        PickNewDestination();
    }

    void Update()
    {
        if (points.Length < 2) return;

        // 1. Calculate how far we have traveled based on speed and time
        float distCovered = (Time.time - startTime) * speed;
        
        // Calculate the fraction of the journey (0.0 to 1.0)
        float fractionOfJourney = distCovered / journeyLength;

        // 2. Determine Base Position (Smooth Linear Movement)
        // We use SmoothStep to make it accelerate/decelerate at the stops
        float smoothFrac = Mathf.SmoothStep(0f, 1f, fractionOfJourney);
        Vector3 basePosition = Vector3.Lerp(currentStartPos, currentEndPos, smoothFrac);

        // 3. Add 3D "Turbulence" (The Floating Effect)
        float noiseX = (Mathf.PerlinNoise(Time.time * turbulenceFrequency, 0f) - 0.5f) * 2;
        float noiseY = (Mathf.PerlinNoise(Time.time * turbulenceFrequency, 10f) - 0.5f) * 2;
        float noiseZ = (Mathf.PerlinNoise(Time.time * turbulenceFrequency, 20f) - 0.5f) * 2;

        Vector3 turbulence = new Vector3(noiseX, noiseY, noiseZ) * turbulenceStrength;

        // Apply Position
        transform.position = basePosition + turbulence;

        // 4. Check if we have reached the destination
        if (fractionOfJourney >= 1f)
        {
            PickNewDestination();
        }
    }

    void PickNewDestination()
    {
        // The current end becomes the new start
        currentStartPos = transform.position; // Use actual current position to prevent snapping
        // Or strictly: currentStartPos = points[currentTargetIndex].position; 

        // Pick a random index
        int newIndex = Random.Range(0, points.Length);

        // Ensure we don't pick the same point we are currently at
        // (If we picked the same one, loop until we find a different one)
        while (newIndex == currentTargetIndex)
        {
            newIndex = Random.Range(0, points.Length);
        }

        currentTargetIndex = newIndex;
        currentEndPos = points[currentTargetIndex].position;

        // Reset journey calculations
        journeyLength = Vector3.Distance(currentStartPos, currentEndPos);
        startTime = Time.time;
    }

    // Visualize the points in the Scene View
    void OnDrawGizmos()
    {
        if (points == null) return;
        
        Gizmos.color = Color.yellow;
        foreach (Transform t in points)
        {
            if (t != null)
                Gizmos.DrawWireSphere(t.position, 0.5f);
        }
    }
}