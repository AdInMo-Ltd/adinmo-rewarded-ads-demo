using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbitOscillate : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;

    [Header("Movement Settings")]
    [Tooltip("How fast the camera swings back and forth")]
    public float swaySpeed = 0.5f;

    [Tooltip("The angle in degrees for the half-circle (e.g., 90 creates a 180-degree span)")]
    public float swayAngle = 90f;

    [Header("Zoom Settings")]
    public float minDistance = 2.0f;     // Closest distance allowed
    public float maxDistance = 20.0f;    // Furthest distance allowed
    public float zoomSpeedMouse = 5.0f;  // Speed for mouse scroll
    public float zoomSpeedTouch = 0.5f;  // Speed for touch pinch
    public float zoomSmoothness = 10.0f; // How quickly the zoom settles

    [Header("Handheld Noise Settings")]
    [Tooltip("How fast the hand shakes")]
    public float noiseFrequency = 1.0f;

    [Tooltip("How far the camera moves up/down due to noise")]
    public float noiseAmplitude = 0.5f;

    // Internal state variables
    private Vector3 initialOffsetFlat; // Distance on the ground plane (X, Z)
    private float initialHeight;       // Distance in height (Y)
    private float currentSwayTime;

    // Zoom state
    private float initialDistanceMagnitude; // The total distance at Start
    private float targetZoomDistance;       // The target distance we want to reach
    private float currentZoomDistance;      // The current interpolated distance

    void Start()
    {
        if (target != null)
        {
            // 1. Get the full vector from target to camera
            Vector3 fullOffset = transform.position - target.position;

            // 2. Store the initial magnitude (hypotenuse) for zoom calculations
            initialDistanceMagnitude = fullOffset.magnitude;
            
            // Set initial zoom values to current position
            targetZoomDistance = initialDistanceMagnitude;
            currentZoomDistance = initialDistanceMagnitude;

            // 3. Separate the Height (Y) from the Horizontal Distance (X, Z)
            initialHeight = fullOffset.y;

            // 4. Create a "flat" offset for the circular rotation math
            fullOffset.y = 0;
            initialOffsetFlat = fullOffset;
        }
    }

    void Update()
    {
        if (target == null) return;

        // --- STEP 1: Handle Input & Zoom Logic ---
        HandleZoomInput();

        // Smoothly move current zoom towards target zoom
        currentZoomDistance = Mathf.Lerp(currentZoomDistance, targetZoomDistance, Time.deltaTime * zoomSmoothness);

        // Calculate the ratio of (Current Distance / Start Distance)
        // Example: If we are at distance 5 and started at 10, scale is 0.5
        float zoomScale = currentZoomDistance / initialDistanceMagnitude;


        // --- STEP 2: Calculate the Horizontal Sway (Scaled by Zoom) ---
        currentSwayTime += Time.deltaTime * swaySpeed;

        // Get the angle for this frame (Sine wave)
        float currentAngle = Mathf.Sin(currentSwayTime) * swayAngle;

        // Rotate the flat offset around the UP axis
        Quaternion rotation = Quaternion.AngleAxis(currentAngle, Vector3.up);

        // Determine X/Z position: Take original flat offset, Scale it by Zoom, then Rotate it
        Vector3 desiredPosition = target.position + (rotation * (initialOffsetFlat * zoomScale));


        // --- STEP 3: Calculate Height (Scaled by Zoom + Noise) ---
        
        // Scale the height so we maintain the same camera angle (pitch)
        float scaledHeight = initialHeight * zoomScale;

        // Get smooth noise value
        float noiseVal = Mathf.PerlinNoise(Time.time * noiseFrequency, 0f);
        
        // Remap noise to -1..1 range
        float noiseOffset = (noiseVal - 0.5f) * 2 * noiseAmplitude;

        // Set the final height: Target Y + Scaled Initial Height + Noise
        desiredPosition.y = target.position.y + scaledHeight + noiseOffset;


        // --- STEP 4: Apply ---
        transform.position = desiredPosition;
        transform.LookAt(target);
    }

    void HandleZoomInput()
    {
        // Mouse Scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetZoomDistance -= scroll * zoomSpeedMouse;
        }

        // Touch Pinch (Mobile)
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            targetZoomDistance += deltaMagnitudeDiff * zoomSpeedTouch;
        }

        // Clamp the zoom distance so we don't go too close or too far
        targetZoomDistance = Mathf.Clamp(targetZoomDistance, minDistance, maxDistance);
    }
}