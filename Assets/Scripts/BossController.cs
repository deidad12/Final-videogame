using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Estadísticas del Jefe")]
    public int vidaMaxima = 3;
    private int vidaActual;
    public float velocidad = 3f;
    public float fuerzaSalto = 8f;
    public float intervaloSalto = 3.5f;

    [Header("Límites de Movimiento")]
    public Transform limiteIzquierdo;
    public Transform limiteDerecho;

    [Header("Efectos Visuales")]
    public SpriteRenderer spriteRenderer;
    public Color colorDano = Color.red;
    public float duracionInvulnerable = 1.2f;

    private Rigidbody2D rb;
    private bool moviendoDerecha = true;
    private bool invulnerable = false;
    private float tiempoSiguienteSalto;

    void Start()
    {
        vidaActual = vidaMaxima;
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        tiempoSiguienteSalto = Time.time + intervaloSalto;

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void Update()
    {
        // No actuar si el juego ya terminó
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded())
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        Mover();

        // Control del salto periódico
        if (Time.time >= tiempoSiguienteSalto)
        {
            Saltar();
            tiempoSiguienteSalto = Time.time + intervaloSalto + Random.Range(-0.5f, 0.5f);
        }
    }

    void Mover()
    {
        // Patrullar entre los límites si están definidos
        if (limiteIzquierdo != null && transform.position.x <= limiteIzquierdo.position.x)
        {
            moviendoDerecha = true;
        }
        if (limiteDerecho != null && transform.position.x >= limiteDerecho.position.x)
        {
            moviendoDerecha = false;
        }

        float direccion = moviendoDerecha ? 1f : -1f;
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !moviendoDerecha;
        }
    }

    void Saltar()
    {
        if (rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.05f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
        }
    }

    // Se llama desde PlayerMovement cuando el jugador cae en la cabeza del jefe
    public void TakeDamage()
    {
        if (invulnerable) return;

        vidaActual--;
        Debug.Log($"El Jefe ha recibido daño. Vidas restantes: {vidaActual}");

        // Aumentar velocidad y agresividad con cada golpe
        velocidad += 1.5f;
        intervaloSalto *= 0.8f;

        if (vidaActual <= 0)
        {
            DerrotarJefe();
        }
        else
        {
            StartCoroutine(EfectoInvulnerabilidad());
        }
    }

    IEnumerator EfectoInvulnerabilidad()
    {
        invulnerable = true;
        
        float tiempoTranscurrido = 0f;
        float intervaloParpadeo = 0.1f;
        
        while (tiempoTranscurrido < duracionInvulnerable)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = (spriteRenderer.color == Color.white) ? colorDano : Color.white;
            }
            yield return new WaitForSeconds(intervaloParpadeo);
            tiempoTranscurrido += intervaloParpadeo;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        
        invulnerable = false;
    }

    void DerrotarJefe()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        // Registrar victoria a través del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Victory();
        }

        Destroy(gameObject, 0.2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Invertir dirección si choca con obstáculos que no son límites sino paredes comunes
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.7f)
                {
                    moviendoDerecha = !moviendoDerecha;
                    break;
                }
            }
        }
    }
}
