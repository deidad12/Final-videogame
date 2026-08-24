using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    public enum BossState { Patrulla, FaseGravedad, Vulnerable, Muerte }

    [Header("Estadísticas del Jefe")]
    public int vidaMaxima = 3;
    private int vidaActual;
    public float velocidad = 3f;
    public float fuerzaSalto = 8f;
    public float intervaloSalto = 3.5f;

    // Propiedades y Eventos para la UI
    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    public event System.Action<int, int> OnVidaCambiada;

    [Header("Límites de Movimiento")]
    public Transform limiteIzquierdo;
    public Transform limiteDerecho;

    [Header("Fase de Alteración de Gravedad")]
    public float intervaloFaseGravedad = 8f;
    public float duracionFaseGravedad = 3f;
    public float duracionVulnerable = 2.5f;
    public Color colorFaseGravedad = new Color(0.6f, 0.4f, 1f);
    public Color colorVulnerable = new Color(1f, 0.95f, 0.3f);
    private float tiempoSiguienteFaseGravedad;
    private float gravedadOriginalY;
    private bool gravedadGuardada = false;

    [Header("Efectos Visuales")]
    public SpriteRenderer spriteRenderer;
    public Color colorDano = Color.red;
    public float duracionInvulnerable = 1.2f;
    private bool invulnerablePostGolpe = false;

    private Rigidbody2D rb;
    private bool moviendoDerecha = true;
    private float tiempoSiguienteSalto;

    public BossState EstadoActual { get; private set; } = BossState.Patrulla;

    void Start()
    {
        vidaActual = vidaMaxima;
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        tiempoSiguienteSalto = Time.time + intervaloSalto;
        tiempoSiguienteFaseGravedad = Time.time + intervaloFaseGravedad;

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        gravedadOriginalY = Physics2D.gravity.y;
        gravedadGuardada = true;

        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);
        CargarImagenBoss();
    }

    void CargarImagenBoss()
    {
        string filePath = System.IO.Path.Combine(Application.dataPath, "Materials/boss final.jpg");
        if (System.IO.File.Exists(filePath))
        {
            byte[] fileData = System.IO.File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
            {
                Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = newSprite;
                    Debug.Log("Imagen del boss final cargada con éxito desde Assets/Materials/boss final.jpg.");
                }
            }
        }
        else
        {
            Debug.LogWarning("No se encontró la imagen del boss en: " + filePath);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded())
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        if (EstadoActual == BossState.Muerte) return;

        if (EstadoActual == BossState.Patrulla)
        {
            Mover();

            if (Time.time >= tiempoSiguienteSalto)
            {
                Saltar();
                tiempoSiguienteSalto = Time.time + intervaloSalto + Random.Range(-0.5f, 0.5f);
            }

            if (Time.time >= tiempoSiguienteFaseGravedad)
            {
                StartCoroutine(EjecutarFaseGravedad());
            }
        }
    }

    void Mover()
    {
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

    IEnumerator EjecutarFaseGravedad()
    {
        EstadoActual = BossState.FaseGravedad;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f; // Flotar inmóvil
        }
        if (spriteRenderer != null) spriteRenderer.color = colorFaseGravedad;

        // Invertir gravedad global (afecta al jefe y a los jugadores)
        Physics2D.gravity = new Vector2(Physics2D.gravity.x, -gravedadOriginalY);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SendMessage("PlayGravityFlip", SendMessageOptions.DontRequireReceiver);
        }

        float t = 0f;
        while (t < duracionFaseGravedad)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Restaurar gravedad normal
        Physics2D.gravity = new Vector2(Physics2D.gravity.x, gravedadOriginalY);

        if (rb != null)
        {
            rb.gravityScale = 1f;
        }

        // Entrar en estado Vulnerable
        EstadoActual = BossState.Vulnerable;
        if (spriteRenderer != null) spriteRenderer.color = colorVulnerable;

        float tv = 0f;
        while (tv < duracionVulnerable)
        {
            tv += Time.deltaTime;
            yield return null;
        }

        // Volver a patrullar si sigue vivo
        if (EstadoActual != BossState.Muerte)
        {
            EstadoActual = BossState.Patrulla;
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            tiempoSiguienteFaseGravedad = Time.time + intervaloFaseGravedad;
            tiempoSiguienteSalto = Time.time + intervaloSalto;
        }
    }

    public void TakeDamage()
    {
        if (EstadoActual == BossState.Muerte) return;
        if (invulnerablePostGolpe) return;

        // En estado Vulnerable el golpe hace daño extra
        int danoAplicado = (EstadoActual == BossState.Vulnerable) ? 2 : 1;
        vidaActual -= danoAplicado;

        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);
        Debug.Log($"El Jefe ha recibido daño ({danoAplicado}). Vidas restantes: {vidaActual}");

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
        invulnerablePostGolpe = true;
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

        if (spriteRenderer != null && EstadoActual == BossState.Patrulla)
        {
            spriteRenderer.color = Color.white;
        }

        invulnerablePostGolpe = false;
    }

    void DerrotarJefe()
    {
        EstadoActual = BossState.Muerte;

        // Asegurar gravedad normal al morir
        Physics2D.gravity = new Vector2(Physics2D.gravity.x, gravedadOriginalY);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 1f;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Victory();
        }

        OnVidaCambiada?.Invoke(0, vidaMaxima); // Actualizar barra de vida al morir
        Destroy(gameObject, 0.2f);
    }

    void OnDestroy()
    {
        // Salvaguarda gravedad
        Physics2D.gravity = new Vector2(Physics2D.gravity.x, gravedadOriginalY);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
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
