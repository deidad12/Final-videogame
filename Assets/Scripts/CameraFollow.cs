using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    public Transform objetivo; // El jugador a seguir
    public float suavidad = 5f; // Velocidad del suavizado de la cámara
    public Vector3 offset = new Vector3(0f, 1f, -10f); // Desplazamiento de la cámara respecto al jugador

    void LateUpdate()
    {
        if (objetivo == null) return;

        // Posición ideal a la que debe ir la cámara
        Vector3 posicionDeseada = objetivo.position + offset;

        // Transición suave entre la posición actual y la deseada
        Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, suavidad * Time.deltaTime);

        // Aseguramos que la cámara mantenga su distancia en el eje Z
        posicionSuavizada.z = offset.z;

        // Actualizamos la posición de la cámara
        transform.position = posicionSuavizada;
    }
}
