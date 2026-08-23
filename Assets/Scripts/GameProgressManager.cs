using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    private static GameProgressManager instance;
    public static GameProgressManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameProgressManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameProgressManager");
                    instance = go.AddComponent<GameProgressManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- MÉTODOS DE RUTA DE PROGRESO ---

    public int ObtenerHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }

    public int ObtenerGamesWon()
    {
        return PlayerPrefs.GetInt("GamesWon", 0);
    }

    public float ObtenerBestTime()
    {
        // 0f significa que no hay tiempo guardado aún
        return PlayerPrefs.GetFloat("BestTime", 0f);
    }

    public void RegistrarVictoria(int puntajeFinal, float tiempoRestante)
    {
        // 1. Sumar partida ganada
        int gamesWon = ObtenerGamesWon() + 1;
        PlayerPrefs.SetInt("GamesWon", gamesWon);

        // 2. Evaluar High Score
        int currentHighScore = ObtenerHighScore();
        if (puntajeFinal > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", puntajeFinal);
            Debug.Log($"¡Nuevo High Score registrado: {puntajeFinal}!");
        }

        // 3. Evaluar mejor tiempo registrado (Best Time)
        // El tiempo restante indica cuánto tiempo sobró; si es un cronómetro inverso, más tiempo restante es mejor.
        // Si es contrarreloj, el tiempo transcurrido sería menor, pero aquí GameManager usa un tiempo que decrementa.
        // Entonces, a mayor "tiempoRestante", significa que completó el nivel más rápido (mejor tiempo).
        float currentBestTime = ObtenerBestTime();
        if (tiempoRestante > currentBestTime)
        {
            PlayerPrefs.SetFloat("BestTime", tiempoRestante);
            Debug.Log($"¡Nuevo récord de tiempo registrado: {tiempoRestante:F2}s restantes!");
        }

        PlayerPrefs.Save();

        // Si hay una UI activa, notificarle
        if (GameSettingsUI.Instance != null)
        {
            GameSettingsUI.Instance.ActualizarTextosRécord();
        }
    }

    public void ReiniciarRecords()
    {
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.DeleteKey("GamesWon");
        PlayerPrefs.DeleteKey("BestTime");
        PlayerPrefs.Save();

        if (GameSettingsUI.Instance != null)
        {
            GameSettingsUI.Instance.ActualizarTextosRécord();
        }
    }
}
