using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Parámetros de Movimiento")]
    public float velocidad = 2f;
    public bool empiezaMoviendoDerecha = true;

    [Header("Detección de Obstáculos y Bordes")]
    public Transform detectorSuelo;
    public Transform detectorPared;
    public LayerMask capaSuelo;
    public float distanciaDeteccion = 0.5f;

    private Rigidbody2D rb;
    private bool moviendoDerecha;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        moviendoDerecha = empiezaMoviendoDerecha;
    }

    void Update()
    {
        // Movimiento horizontal continuo
        float direccion = moviendoDerecha ? 1f : -1f;
        rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);

        // Voltear sprite según la dirección
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !moviendoDerecha;
        }

        // Si tenemos detectores asignados, evitar caídas y chocar contra paredes
        if (detectorSuelo != null)
        {
            // Lanzar raycast hacia abajo para ver si hay suelo adelante
            RaycastHit2D infoSuelo = Physics2D.Raycast(detectorSuelo.position, Vector2.down, distanciaDeteccion, capaSuelo);
            if (infoSuelo.collider == null)
            {
                Girar();
            }
        }

        if (detectorPared != null)
        {
            // Lanzar raycast horizontal para detectar paredes
            Vector2 dirRaycast = moviendoDerecha ? Vector2.right : Vector2.left;
            RaycastHit2D infoPared = Physics2D.Raycast(detectorPared.position, dirRaycast, distanciaDeteccion, capaSuelo);
            if (infoPared.collider != null)
            {
                Girar();
            }
        }
    }

    private void Girar()
    {
        moviendoDerecha = !moviendoDerecha;
    }

    // Método que se llama cuando el jugador lo pisa desde arriba
    public void TakeDamage()
    {
        // Animaciones opcionales, efectos de partículas, etc.
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // En caso de que no se usen detectores por raycast, girar al chocar lateralmente
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Si chocamos de lado (normal horizontal alta)
                if (Mathf.Abs(contact.normal.x) > 0.7f)
                {
                    Girar();
                    break;
                }
            }
        }
    }
}
