using UnityEngine;

public class ObstacleThwomp : MonoBehaviour
{
    [Header("Parámetros de Caída")]
    public float velocidadCaida = 12f;
    public float velocidadRetorno = 2f;
    public float distanciaDeteccion = 8f;
    public float anchoDeteccion = 1.5f;
    public float tiempoEsperaAbajo = 1f;
    public LayerMask capaJugador;

    private Vector3 posicionInicial;
    private enum Estado { Esperando, Cayendo, EsperandoAbajo, Subiendo }
    private Estado estadoActual = Estado.Esperando;
    private float tiempoEsperaContador;
    private Rigidbody2D rb;

    void Start()
    {
        posicionInicial = transform.position;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // Queremos controlar el movimiento por script
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void Update()
    {
        // No actuar si el juego ya terminó
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded()) return;

        switch (estadoActual)
        {
            case Estado.Esperando:
                // Lanzar un BoxCast hacia abajo para detectar al jugador en su rango
                RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(anchoDeteccion, 0.1f), 0f, Vector2.down, distanciaDeteccion, capaJugador);
                if (hit.collider != null)
                {
                    estadoActual = Estado.Cayendo;
                }
                break;

            case Estado.Cayendo:
                // Desplazar hacia abajo
                transform.Translate(Vector3.down * velocidadCaida * Time.deltaTime, Space.World);
                break;

            case Estado.EsperandoAbajo:
                tiempoEsperaContador -= Time.deltaTime;
                if (tiempoEsperaContador <= 0f)
                {
                    estadoActual = Estado.Subiendo;
                }
                break;

            case Estado.Subiendo:
                // Retornar a la posición original lentamente
                transform.position = Vector3.MoveTowards(transform.position, posicionInicial, velocidadRetorno * Time.deltaTime);
                if (Vector3.Distance(transform.position, posicionInicial) < 0.05f)
                {
                    transform.position = posicionInicial;
                    estadoActual = Estado.Esperando;
                }
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si golpeamos al jugador
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerMovement>() != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RespawnPlayer(collision.gameObject);
            }
            
            // Volver tras golpear al jugador
            IniciarEsperaAbajo();
        }
        else if (estadoActual == Estado.Cayendo)
        {
            // Al chocar con el suelo o plataformas
            IniciarEsperaAbajo();
        }
    }

    private void IniciarEsperaAbajo()
    {
        estadoActual = Estado.EsperandoAbajo;
        tiempoEsperaContador = tiempoEsperaAbajo;
        
        // Detener velocidad residual si existe un Rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Dibujar el área de detección en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.down * (distanciaDeteccion / 2f), new Vector3(anchoDeteccion, distanciaDeteccion, 1f));
    }
}
