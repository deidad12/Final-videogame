using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración del Jugador")]
    public GameObject jugador;
    public Transform spawnPoint;
    public int vidasMaximas = 3;
    private int vidasActuales;

    [Header("Configuración del Temporizador")]
    public float tiempoInicial = 60f;
    private float tiempoRestante;

    [Header("Nombres de las Escenas")]
    public string escenaNivel1 = "Level1";
    public string escenaNivel2 = "Level2";
    public string escenaNivel3 = "Level3";
    public string escenaMenu = "MainMenu";

    [Header("Interfaz de Usuario (UI)")]
    public TextMeshProUGUI textoTemporizador;
    public TextMeshProUGUI textoVidas;
    public TextMeshProUGUI textoObjetos;
    public TextMeshProUGUI textoDerrotaMensaje; // Mensaje para indicar por qué murió
    public GameObject panelVictoria;
    public GameObject panelDerrota;
    
    [Header("Datos del Alumno")]
    public TextMeshProUGUI textoAlumno;
    public string nombreAlumno = "Diseño Pastel";
    public string matriculaAlumno = "1-22-2809";

    private int objetosColectados = 0;
    private int puntaje = 0;
    private bool juegoTerminado = false;

    private GameObject player2Instance;
    private Camera camP1;
    private Camera camP2;

    void Awake()
    {
        // Singleton para acceso global fácil
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        vidasActuales = vidasMaximas;
        tiempoRestante = tiempoInicial;
        objetosColectados = 0;
        puntaje = 0;

        // Desactivar paneles de fin de juego al comenzar
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        // Inicializar multijugador si está activo en los ajustes
        InicializarJugadoresYCameras();

        // Configurar los datos personales y HUD
        ActualizarDatosAlumno();
        ActualizarHUD();

        // Asegurarse de que el AudioManager reproduzca la música correcta
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM();
        }

        // Traducir interfaz
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.TraducirInterfazActual();
        }

        // Configurar botones de UI de fin de juego y pausa de forma automática para seguridad
        ConfigurarBotonesJuego();
    }

    private void ConfigurarBotonesJuego()
    {
        Button[] botones = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in botones)
        {
            if (btn.gameObject.name == "BotonAjustesFlotante") continue;

            string nombre = btn.gameObject.name.ToLower();
            
            if (nombre.Contains("reiniciar") || nombre.Contains("restart") || nombre.Contains("reintentar") || nombre.Contains("retry"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ReiniciarJuego);
                Debug.Log($"[GameManager] Botón '{btn.gameObject.name}' configurado para Reiniciar.");
            }
            else if (nombre.Contains("menu") || nombre.Contains("volver") || nombre.Contains("principal") || nombre.Contains("salir") || nombre.Contains("home") || nombre.Contains("regresar") || nombre.Contains("regreso") || nombre.Contains("return") || nombre.Contains("inicio"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(IrAlMenuPrincipal);
                Debug.Log($"[GameManager] Botón '{btn.gameObject.name}' configurado para Ir al Menú Principal.");
            }
        }
    }

    private void InicializarJugadoresYCameras()
    {
        bool multi = false;
        if (GameSettingsManager.Instance != null)
        {
            multi = GameSettingsManager.Instance.multijugadorActivo;
        }

        // Jugador 1 setup
        if (jugador != null)
        {
            PlayerMovement pm1 = jugador.GetComponent<PlayerMovement>();
            if (pm1 != null)
            {
                pm1.playerId = 1;
                pm1.ActualizarColorBaseDesdeAjustes();
            }

            // Asegurar que la cámara principal siga a Jugador 1
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                CameraFollow cf = mainCam.GetComponent<CameraFollow>();
                if (cf == null)
                {
                    cf = mainCam.gameObject.AddComponent<CameraFollow>();
                }
                cf.playerId = 1;
                cf.objetivo = jugador.transform;
            }
        }

        if (multi && jugador != null && spawnPoint != null)
        {
            // Spawnear Jugador 2
            player2Instance = Instantiate(jugador, spawnPoint.position + Vector3.right * 1.5f, Quaternion.identity);
            player2Instance.name = "Player 2";
            
            PlayerMovement pm2 = player2Instance.GetComponent<PlayerMovement>();
            if (pm2 != null)
            {
                pm2.playerId = 2;
                pm2.ActualizarColorBaseDesdeAjustes();
            }

            // Dividir pantalla (cámaras)
            camP1 = Camera.main;
            if (camP1 != null)
            {
                camP1.rect = new Rect(0f, 0f, 0.5f, 1f); // Mitad izquierda

                // Clonar cámara para Jugador 2
                GameObject cam2Obj = Instantiate(camP1.gameObject);
                cam2Obj.name = "CameraPlayer2";
                
                // Remover AudioListener sobrante
                AudioListener al = cam2Obj.GetComponent<AudioListener>();
                if (al != null) Destroy(al);

                camP2 = cam2Obj.GetComponent<Camera>();
                camP2.rect = new Rect(0.5f, 0f, 0.5f, 1f); // Mitad derecha

                // Configurar seguimiento de cámara para P2
                CameraFollow cf2 = cam2Obj.GetComponent<CameraFollow>();
                if (cf2 == null)
                {
                    cf2 = cam2Obj.AddComponent<CameraFollow>();
                }
                cf2.playerId = 2;
                cf2.objetivo = player2Instance.transform;
            }
        }
    }

    void Update()
    {
        if (juegoTerminado) return;

        // Gestión del temporizador
        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            ActualizarHUD();

            if (tiempoRestante <= 0)
            {
                tiempoRestante = 0;
                ActualizarHUD();
                string msg = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("¡Se agotó el tiempo!") : "¡Se agotó el tiempo!";
                Defeat(msg);
            }
        }
    }

    // Gestiona el respawn del jugador al caer o recibir daño
    public void RespawnPlayer(GameObject playerThatDied)
    {
        if (juegoTerminado) return;

        vidasActuales--;
        ActualizarHUD();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDamage();
        }

        if (vidasActuales > 0)
        {
            GameObject target = playerThatDied != null ? playerThatDied : jugador;
            if (target != null && spawnPoint != null)
            {
                // Si es el Jugador 2, spawnear con un leve offset
                PlayerMovement pm = target.GetComponent<PlayerMovement>();
                Vector3 offset = (pm != null && pm.playerId == 2) ? Vector3.right * 1.5f : Vector3.zero;

                target.transform.position = spawnPoint.position + offset;
                
                // Detener cualquier velocidad que tuviera acumulada
                Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
        else
        {
            string msg = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("¡Te quedaste sin vidas!") : "¡Te quedaste sin vidas!";
            Defeat(msg);
        }
    }

    public void RespawnPlayer()
    {
        RespawnPlayer(jugador);
    }

    // Sumar un objeto coleccionado
    public void AddCollectible(int cantidad = 1)
    {
        if (juegoTerminado) return;
        objetosColectados += cantidad;
        AddScore(cantidad * 100); // 100 puntos por caramelo
        ActualizarHUD();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCollectible();
        }
    }

    public void AddScore(int puntos)
    {
        if (juegoTerminado) return;
        puntaje += puntos;
        ActualizarHUD();
        if (textoObjetos != null)
        {
            StopCoroutine("PunzarPuntaje");
            StartCoroutine(PunzarPuntaje());
        }
    }

    // Pequeña animacion de "punch" en el texto de puntaje al sumar puntos, para un look mas vivo y moderno.
    private System.Collections.IEnumerator PunzarPuntaje()
    {
        Transform t = textoObjetos.transform;
        Vector3 escalaBase = Vector3.one;
        Vector3 escalaPico = escalaBase * 1.15f;
        float duracion = 0.12f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            t.localScale = Vector3.Lerp(escalaBase, escalaPico, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        tiempo = 0f;
        while (tiempo < duracion)
        {
            t.localScale = Vector3.Lerp(escalaPico, escalaBase, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        t.localScale = escalaBase;
    }

    public void AddLife()
    {
        if (juegoTerminado) return;
        vidasActuales++;
        ActualizarHUD();
    }

    // Condición de Victoria (Al cruzar la meta)
    public void Victory()
    {
        if (juegoTerminado) return;

        string escenaActual = SceneManager.GetActiveScene().name;

        // Sumar bonificación de tiempo
        int bonusTiempo = Mathf.CeilToInt(tiempoRestante) * 10;
        AddScore(bonusTiempo);

        // Registrar progreso y récords
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.RegistrarVictoria(puntaje, tiempoRestante);
        }

        // Si estamos en el Nivel 1, vamos al Nivel 2
        if (escenaActual == escenaNivel1 || escenaActual == "SampleScene")
        {
            juegoTerminado = true;
            DetenerJugador();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayVictory();
            StartCoroutine(CargarEscenaConRetraso(escenaNivel2, 2.0f));
        }
        // Si estamos en el Nivel 2, vamos al Nivel 3 (Boss)
        else if (escenaActual == escenaNivel2)
        {
            juegoTerminado = true;
            DetenerJugador();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayVictory();
            StartCoroutine(CargarEscenaConRetraso(escenaNivel3, 2.0f));
        }
        // Si estamos en el Nivel 3 (Boss) y ganamos, es la victoria final del juego
        else if (escenaActual == escenaNivel3)
        {
            juegoTerminado = true;
            if (panelVictoria != null)
            {
                panelVictoria.SetActive(true);
            }
            DetenerJugador();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayVictory();
        }
    }

    // Condición de Derrota (por tiempo o vidas)
    public void Defeat(string mensaje = "¡Has muerto!")
    {
        if (juegoTerminado) return;
        juegoTerminado = true;

        if (textoDerrotaMensaje != null)
        {
            textoDerrotaMensaje.text = mensaje;
        }
        else if (panelDerrota != null)
        {
            // Intentar buscar dinámicamente un texto si no está asignado
            TextMeshProUGUI tmpText = panelDerrota.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null && tmpText != textoVidas && tmpText != textoTemporizador)
            {
                tmpText.text = mensaje;
            }
        }

        if (panelDerrota != null)
        {
            panelDerrota.SetActive(true);
        }

        DetenerJugador();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDefeat();
        }

        // Si el panel de derrota existe, dejamos que el usuario interactúe con los botones del panel.
        // Solo recargamos automáticamente de forma silenciosa si no hay un panel visible.
        if (panelDerrota == null)
        {
            string escenaActual = SceneManager.GetActiveScene().name;
            string regMsg = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Regresando al Nivel 1...") : "Regresando al Nivel 1...";
            string reinMsg = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Reiniciando Nivel...") : "Reiniciando Nivel...";

            if (escenaActual == escenaNivel2 || escenaActual == escenaNivel3)
            {
                if (textoDerrotaMensaje != null)
                {
                    textoDerrotaMensaje.text = $"{mensaje}\n{regMsg}";
                }
                StartCoroutine(CargarEscenaConRetraso(escenaNivel1, 2.5f));
            }
            else
            {
                if (textoDerrotaMensaje != null)
                {
                    textoDerrotaMensaje.text = $"{mensaje}\n{reinMsg}";
                }
                StartCoroutine(CargarEscenaConRetraso(escenaActual, 2.5f));
            }
        }
    }

    private System.Collections.IEnumerator CargarEscenaConRetraso(string nombreEscena, float retraso)
    {
        yield return new WaitForSecondsRealtime(retraso);
        SceneManager.LoadScene(nombreEscena);
    }

    // Detiene el movimiento físico de todos los jugadores
    private void DetenerJugador()
    {
        PlayerMovement[] jugadores = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement pm in jugadores)
        {
            Rigidbody2D rb = pm.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    // Actualiza los textos del HUD
    private void ActualizarHUD()
    {
        string txtTiempo = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Tiempo") : "Tiempo";
        string txtVidas = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Vidas") : "Vidas";
        string txtObjetos = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Objetos") : "Objetos";
        string txtPuntaje = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Puntaje") : "Puntaje";

        if (textoTemporizador != null)
        {
            textoTemporizador.text = $"{txtTiempo}: {Mathf.CeilToInt(tiempoRestante)}s";
        }

        if (textoVidas != null)
        {
            textoVidas.text = $"{txtVidas}: {vidasActuales}";
        }

        if (textoObjetos != null)
        {
            string numFormateado = puntaje.ToString("N0");
            textoObjetos.text = $"<size=80%><color=#FFFFFFB3>{txtObjetos} {objetosColectados}</color></size>   <color=#FF6FA5><b>{numFormateado}</b></color> <size=70%><color=#FFFFFFB3>{txtPuntaje.ToUpper()}</color></size>";
        }
    }

    // Actualiza la visualización de datos del alumno / jugador
    private void ActualizarDatosAlumno()
    {
        if (textoAlumno != null)
        {
            string txtEstudiante = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Estudiante:") : "Estudiante:";

            string p1Name = GameSettingsManager.Instance != null ? GameSettingsManager.Instance.nombreP1 : nombreAlumno;
            string p2Name = GameSettingsManager.Instance != null ? GameSettingsManager.Instance.nombreP2 : "";
            bool multi = GameSettingsManager.Instance != null && GameSettingsManager.Instance.multijugadorActivo;

            string jugadorNombres = multi ? $"{p1Name} & {p2Name}" : p1Name;

            textoAlumno.text = $"{txtEstudiante} {jugadorNombres}\nMatrícula: 1-22-2809";
        }
    }

    public bool IsGameEnded()
    {
        return juegoTerminado;
    }

    // Métodos para botones de la interfaz
    public void ReiniciarJuego()
    {
        Time.timeScale = 1f; // Asegurar tiempo normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f; // Asegurar tiempo normal
        SceneManager.LoadScene(escenaMenu);
    }
}
