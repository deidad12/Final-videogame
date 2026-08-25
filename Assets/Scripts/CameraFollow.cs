using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    public Transform objetivo; // El jugador a seguir
    public float suavidad = 5f; // Velocidad del suavizado de la cámara
    public Vector3 offset = new Vector3(0f, 1f, -10f); // Desplazamiento de la cámara respecto al jugador
    public int playerId = 1; // ID del jugador a seguir (1 para Player 1, 2 para Player 2)

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        BuscarObjetivo();
    }

    void LateUpdate()
    {
        if (objetivo == null)
        {
            BuscarObjetivo();
            if (objetivo == null) return;
        }

        // Posición ideal a la que debe ir la cámara
        Vector3 posicionDeseada = objetivo.position + offset;

        // Transición suave entre la posición actual y la deseada
        Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, suavidad * Time.deltaTime);

        // Aseguramos que la cámara mantenga su distancia en el eje Z
        posicionSuavizada.z = offset.z;

        // Recortamos la posición a los límites del nivel actual para que la cámara nunca
        // muestre vacío más allá de las paredes/techo/suelo generados para el nivel.
        ClampDentroDeLimites(ref posicionSuavizada);

        // Actualizamos la posición de la cámara
        transform.position = posicionSuavizada;
    }

    private void ClampDentroDeLimites(ref Vector3 posicion)
    {
        if (!LevelBounds.Valid) return;
        if (cam == null || !cam.orthographic) return;

        float mediaAltura = cam.orthographicSize;
        float mediaAnchura = mediaAltura * cam.aspect;

        float minX = LevelBounds.MinX + mediaAnchura;
        float maxX = LevelBounds.MaxX - mediaAnchura;
        if (minX <= maxX)
        {
            posicion.x = Mathf.Clamp(posicion.x, minX, maxX);
        }

        float minY = LevelBounds.MinY + mediaAltura;
        float maxY = LevelBounds.MaxY - mediaAltura;
        if (minY <= maxY)
        {
            posicion.y = Mathf.Clamp(posicion.y, minY, maxY);
        }
    }

    private void BuscarObjetivo()
    {
        // Buscar el movimiento del jugador correspondiente por ID
        PlayerMovement[] jugadores = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (PlayerMovement pm in jugadores)
        {
            if (pm.playerId == playerId)
            {
                objetivo = pm.transform;
                return;
            }
        }

        // Fallback genérico si no se encuentra
        GameObject jugadorObj = GameObject.FindWithTag("Player");
        if (jugadorObj == null)
        {
            PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null) jugadorObj = pm.gameObject;
        }

        if (jugadorObj != null)
        {
            objetivo = jugadorObj.transform;
        }
    }
}
