using UnityEngine;
using System.Collections.Generic;

public class MultiplayerCamera : MonoBehaviour
{
    public List<Transform> targets = new List<Transform>();
    public Vector3 offset = new Vector3(0f, 1f, -10f);
    public float smoothTime = 0.2f;

    public float minSize = 5f;
    public float maxSize = 9f;
    public float zoomLimiter = 12f;

    private Vector3 velocity;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
        BuscarJugadores();
    }

    void LateUpdate()
    {
        // Si no hay objetivos, buscarlos activamente en la escena
        if (targets.Count == 0 || targets[0] == null)
        {
            BuscarJugadores();
            if (targets.Count == 0) return;
        }

        // Limpiar referencias muertas (si algún jugador se destruye)
        targets.RemoveAll(t => t == null);

        if (targets.Count == 0) return;

        MoverCamera();
        ZoomCamera();
    }

    public void BuscarJugadores()
    {
        targets.Clear();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.activeInHierarchy)
            {
                targets.Add(player.transform);
            }
        }
    }

    private void MoverCamera()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 targetPosition = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    private void ZoomCamera()
    {
        if (cam == null) return;
        float newSize = Mathf.Lerp(minSize, maxSize, GetGreatestDistance() / zoomLimiter);
        cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, newSize, Time.deltaTime * 3f);
    }

    private float GetGreatestDistance()
    {
        if (targets.Count <= 1) return 0f;

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        return bounds.size.x;
    }

    private Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }
}
