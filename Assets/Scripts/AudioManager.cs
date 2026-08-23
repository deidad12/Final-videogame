using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips de Audio")]
    [Tooltip("Música de fondo que se repetirá en bucle.")]
    public AudioClip musicaFondo;

    [Tooltip("Música de fondo especial para el nivel del Jefe.")]
    public AudioClip musicaBoss;
    
    [Tooltip("Efecto de sonido al saltar.")]
    public AudioClip sonidoSalto;
    
    [Tooltip("Efecto de sonido al pisar un enemigo.")]
    public AudioClip sonidoStomp;

    [Tooltip("Efecto de sonido al recibir daño.")]
    public AudioClip sonidoDano;

    [Tooltip("Efecto de sonido al recoger un coleccionable.")]
    public AudioClip sonidoColeccionable;

    [Tooltip("Efecto de sonido al golpear un bloque.")]
    public AudioClip sonidoBloque;

    [Tooltip("Efecto de sonido cuando el jugador gana.")]
    public AudioClip sonidoVictoria;
    
    [Tooltip("Efecto de sonido al perder.")]
    public AudioClip sonidoDerrota;

    private AudioSource fuenteMusica;
    private AudioSource fuenteSFX;

    void Awake()
    {
        // Patrón Singleton para evitar duplicados al cambiar de escena
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Crear y configurar los componentes AudioSource de forma dinámica
            fuenteMusica = gameObject.AddComponent<AudioSource>();
            fuenteSFX = gameObject.AddComponent<AudioSource>();

            fuenteMusica.loop = true;
            fuenteMusica.playOnAwake = false;

            PlayBGM();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        string bossSceneName = "Level3";
        if (GameManager.Instance != null)
        {
            bossSceneName = GameManager.Instance.escenaNivel3;
        }

        if (scene.name == bossSceneName)
        {
            PlayBossBGM();
        }
        else if (scene.name != "MainMenu" && scene.name != GameManager.Instance?.escenaMenu)
        {
            PlayBGM();
        }
    }

    // Reproduce la música de fondo estándar
    public void PlayBGM()
    {
        if (musicaFondo != null && fuenteMusica != null)
        {
            if (fuenteMusica.clip == musicaFondo && fuenteMusica.isPlaying)
            {
                ActualizarVolumenAjustes();
                return;
            }
            fuenteMusica.clip = musicaFondo;
            ActualizarVolumenAjustes();
            fuenteMusica.Play();
        }
    }

    // Reproduce la música del Jefe
    public void PlayBossBGM()
    {
        if (musicaBoss != null && fuenteMusica != null)
        {
            if (fuenteMusica.clip == musicaBoss && fuenteMusica.isPlaying)
            {
                ActualizarVolumenAjustes();
                return;
            }
            fuenteMusica.clip = musicaBoss;
            ActualizarVolumenAjustes();
            fuenteMusica.Play();
        }
        else
        {
            PlayBGM();
        }
    }

    // Detiene la música de fondo
    public void StopBGM()
    {
        if (fuenteMusica != null)
        {
            fuenteMusica.Stop();
        }
    }

    // Reproduce sonido de salto
    public void PlayJump()
    {
        PlaySFX(sonidoSalto, 0.6f);
    }

    // Reproduce sonido al pisar enemigo
    public void PlayStomp()
    {
        PlaySFX(sonidoStomp, 0.7f);
    }

    // Reproduce sonido al recibir daño
    public void PlayDamage()
    {
        PlaySFX(sonidoDano, 0.7f);
    }

    // Reproduce sonido al recoger coleccionable
    public void PlayCollectible()
    {
        PlaySFX(sonidoColeccionable, 0.5f);
    }

    // Reproduce sonido al golpear un bloque
    public void PlayBlock()
    {
        PlaySFX(sonidoBloque, 0.6f);
    }

    // Reproduce sonido de victoria (detiene la música de fondo primero)
    public void PlayVictory()
    {
        StopBGM();
        PlaySFX(sonidoVictoria, 0.8f);
    }

    // Reproduce sonido de derrota (detiene la música de fondo primero)
    public void PlayDefeat()
    {
        StopBGM();
        PlaySFX(sonidoDerrota, 0.8f);
    }

    // Método para actualizar volumen desde ajustes
    public void ActualizarVolumenAjustes()
    {
        if (fuenteMusica != null)
        {
            float vol = 0.4f;
            if (GameSettingsManager.Instance != null)
            {
                vol = GameSettingsManager.Instance.volumenMusica;
            }
            fuenteMusica.volume = vol;
        }
    }

    // Método genérico para reproducir un efecto de sonido una sola vez
    private void PlaySFX(AudioClip clip, float volumen = 1.0f)
    {
        if (clip != null && fuenteSFX != null)
        {
            float volAjustado = volumen;
            if (GameSettingsManager.Instance != null)
            {
                volAjustado = volumen * GameSettingsManager.Instance.volumenSFX;
            }
            fuenteSFX.PlayOneShot(clip, volAjustado);
        }
    }
}
