using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidad = 6f;
    public float fuerzaSalto = 12f;
    
    [Header("Configuración de Multijugador")]
    public int playerId = 1; // 1 para Jugador 1, 2 para Jugador 2

    [Header("Detección de Suelo")]
    [SerializeField] private bool enSuelo;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // Powerup variables
    private float velocidadMultiplicador = 1f;
    private float saltoMultiplicador = 1f;
    private float speedTimer = 0f;
    private float jumpTimer = 0f;
    private Color baseColor = Color.white;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // Asegurar que tenga el componente de personalización
        if (GetComponent<CharacterCustomizer>() == null)
        {
            gameObject.AddComponent<CharacterCustomizer>();
        }

        // Definir color base según perfil
        ActualizarColorBaseDesdeAjustes();
    }

    public void ActualizarColorBaseDesdeAjustes()
    {
        CharacterCustomizer customizer = GetComponent<CharacterCustomizer>();
        if (customizer != null)
        {
            customizer.AplicarPersonalizacion();
            if (spriteRenderer != null)
            {
                baseColor = spriteRenderer.color;
            }
            return;
        }

        if (GameSettingsManager.Instance != null)
        {
            if (playerId == 1)
            {
                baseColor = GameSettingsManager.Instance.GetAvatarColor(GameSettingsManager.Instance.avatarP1);
            }
            else
            {
                baseColor = GameSettingsManager.Instance.GetAvatarColor(GameSettingsManager.Instance.avatarP2);
            }
        }
        else
        {
            baseColor = (playerId == 1) ? Color.white : new Color(0.7f, 0.85f, 1f);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = baseColor;
        }
    }

    public void ActivarSpeedBoost(float duracion, float multi)
    {
        velocidadMultiplicador = multi;
        speedTimer = duracion;
    }

    public void ActivarJumpBoost(float duracion, float multi)
    {
        saltoMultiplicador = multi;
        jumpTimer = duracion;
    }

    void Update()
    {
        // Si el juego ha terminado o ganado, detenemos el movimiento
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded())
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
                anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
                anim.SetBool("IsGrounded", enSuelo);
            }
            return;
        }

        // Manejar temporizadores de mejoras
        if (speedTimer > 0)
        {
            speedTimer -= Time.deltaTime;
            if (speedTimer <= 0) velocidadMultiplicador = 1f;
        }
        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0) saltoMultiplicador = 1f;
        }

        // Aplicar tintado de color de powerup
        if (spriteRenderer != null)
        {
            if (speedTimer > 0 && jumpTimer > 0)
            {
                spriteRenderer.color = new Color(0.9f, 0.7f, 0.3f); // Naranja/Lima
            }
            else if (speedTimer > 0)
            {
                spriteRenderer.color = new Color(0.4f, 1.0f, 0.4f); // Verde
            }
            else if (jumpTimer > 0)
            {
                spriteRenderer.color = new Color(1.0f, 0.9f, 0.3f); // Amarillo
            }
            else
            {
                spriteRenderer.color = baseColor;
            }
        }

        // Movimiento horizontal según jugador
        float movimiento = 0f;
        if (playerId == 1)
        {
            // Jugador 1 usa A/D
            if (Input.GetKey(KeyCode.A)) movimiento = -1f;
            else if (Input.GetKey(KeyCode.D)) movimiento = 1f;
        }
        else if (playerId == 2)
        {
            // Jugador 2 usa Flecha Izquierda/Derecha
            if (Input.GetKey(KeyCode.LeftArrow)) movimiento = -1f;
            else if (Input.GetKey(KeyCode.RightArrow)) movimiento = 1f;
        }

        float velocidadFinal = velocidad * velocidadMultiplicador;

        rb.linearVelocity = new Vector2(
            movimiento * velocidadFinal,
            rb.linearVelocity.y
        );

        // Voltear personaje según la dirección sin deformar
        if (spriteRenderer != null)
        {
            if (movimiento > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (movimiento < 0)
            {
                spriteRenderer.flipX = true;
            }
            // Voltear verticalmente si la gravedad está invertida
            spriteRenderer.flipY = Physics2D.gravity.y > 0f;
        }
        else
        {
            // Fallback en caso de no tener SpriteRenderer
            if (movimiento > 0)
            {
                transform.localScale = new Vector3(
                    Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
            else if (movimiento < 0)
            {
                transform.localScale = new Vector3(
                    -Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }

        // Saltar según jugador
        bool quiereSaltar = false;
        if (playerId == 1)
        {
            quiereSaltar = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W);
        }
        else if (playerId == 2)
        {
            quiereSaltar = Input.GetKeyDown(KeyCode.UpArrow);
        }

        if (quiereSaltar && enSuelo)
        {
            float saltoFinal = fuerzaSalto * saltoMultiplicador;
            if (Physics2D.gravity.y > 0f)
            {
                saltoFinal = -saltoFinal; // Saltar hacia abajo (hacia el techo)
            }
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                saltoFinal
            );

            enSuelo = false;

            // Reproducir sonido de salto
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayJump();
            }
        }

        // Actualizar parámetros del Animator si existe
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(movimiento));
            anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
            anim.SetBool("IsGrounded", enSuelo);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            bool gInvertida = Physics2D.gravity.y > 0f;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if ((gInvertida && contact.normal.y < -0.5f) || (!gInvertida && contact.normal.y > 0.5f))
                {
                    enSuelo = true;
                    break;
                }
            }
        }

        // Colisión con enemigos
        if (collision.gameObject.CompareTag("Enemy"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Si caemos encima del enemigo (normal apunta hacia arriba para el jugador)
                if (contact.normal.y > 0.5f)
                {
                    EnemyPatrol patrol = collision.gameObject.GetComponent<EnemyPatrol>();
                    if (patrol != null)
                    {
                        patrol.TakeDamage();
                        StompBounce();
                        break;
                    }
                    
                    EnemyFlyer flyer = collision.gameObject.GetComponent<EnemyFlyer>();
                    if (flyer != null)
                    {
                        flyer.TakeDamage();
                        StompBounce();
                        break;
                    }
                    
                    BossController boss = collision.gameObject.GetComponent<BossController>();
                    if (boss != null)
                    {
                        boss.TakeDamage();
                        StompBounce();
                        break;
                    }
                }
                else
                {
                    // Golpe lateral o inferior: el jugador recibe daño
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.RespawnPlayer();
                    }
                    break;
                }
            }
        }

        // Golpear bloques desde abajo
        if (collision.gameObject.CompareTag("Block"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Si golpeamos desde abajo (la normal apunta hacia abajo respecto al bloque)
                if (contact.normal.y < -0.5f)
                {
                    DestructibleBlock block = collision.gameObject.GetComponent<DestructibleBlock>();
                    if (block != null)
                    {
                        block.HitBlock();
                        break;
                    }
                }
            }
        }
    }

    private void StompBounce()
    {
        float bounceForce = fuerzaSalto * 0.8f;
        if (Physics2D.gravity.y > 0f)
        {
            bounceForce = -bounceForce; // Rebotar hacia abajo (hacia el techo)
        }
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
        enSuelo = false;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayStomp();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            bool gInvertida = Physics2D.gravity.y > 0f;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if ((gInvertida && contact.normal.y < -0.5f) || (!gInvertida && contact.normal.y > 0.5f))
                {
                    enSuelo = true;
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            enSuelo = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hazard") || collision.gameObject.name.Contains("Spike") || collision.gameObject.name.Contains("Hazard"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RespawnPlayer(gameObject);
            }
        }
    }
}