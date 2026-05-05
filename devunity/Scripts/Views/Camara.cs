using UnityEngine;
using System.Collections.Generic;

public class Camara : MonoBehaviour
{
    [Header("Jugadores a seguir")]
    public List<Transform> targets;

    [Header("Offset de la cámara")]
    public Vector3 offset = new Vector3(0, 10, -10);

    [Header("Suavizado")]
    public float smoothTime = 0.5f;
    private Vector3 velocity;

    [Header("Zoom dinámico")]
    public float minZoom = 8f;
    public float maxZoom = 20f;
    public float zoomLimiter = 50f;
    public float zoomSmoothTime = 0.5f;

    private Camera cam;
    private float zoomVelocity;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        bool hasTargets = targets.Count > 0;
        if (hasTargets)
        {
            Move();
            Zoom();
        }
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            newPosition,
            ref velocity,
            smoothTime
        );

        transform.LookAt(centerPoint);
    }

    void Zoom()
    {
        float greatestDistance = GetGreatestDistance();
        float newZoom = Mathf.Lerp(maxZoom, minZoom, greatestDistance / zoomLimiter);

        cam.fieldOfView = Mathf.SmoothDamp(
            cam.fieldOfView,
            newZoom,
            ref zoomVelocity,
            zoomSmoothTime
        );
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
            return targets[0].position;

        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);

        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }

    float GetGreatestDistance()
    {
        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);

        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.size.magnitude;
    }
}
