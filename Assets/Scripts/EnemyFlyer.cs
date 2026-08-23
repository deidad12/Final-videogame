using UnityEngine;

public class EnemyFlyer : MonoBehaviour
{
    [Header("Parámetros de Vuelo")]
    public float velocidad = 2f;
    public float amplitud = 2f; // Rango de movimiento vertical/horizontal
    public bool vertical = true; // True = vertical, False = horizontal

    private Vector3 posicionInicial;
    private float tiempoOffset;

    void Start()
    {
        posicionInicial = transform.position;
        // Agregar un offset de tiempo aleatorio para evitar que todos los enemigos vuelen sincronizados
        tiempoOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float tiempo = (Time.time * velocidad) + tiempoOffset;
        float desplazamiento = Mathf.Sin(tiempo) * amplitud;

        if (vertical)
        {
            transform.position = new Vector3(posicionInicial.x, posicionInicial.y + desplazamiento, posicionInicial.z);
        }
        else
        {
            transform.position = new Vector3(posicionInicial.x + desplazamiento, posicionInicial.y, posicionInicial.z);
        }
    }

    // Método que se llama cuando el jugador lo pisa desde arriba
    public void TakeDamage()
    {
        Destroy(gameObject);
    }
}
