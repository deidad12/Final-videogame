using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    private const int MAX_RECORDS = 10;
    private const string CLAVE_TOP_SCORES = "TopScoresList";

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

    // Devuelve TODAS las mejores puntuaciones guardadas (no solo la más alta), de mayor a menor.
    public List<int> ObtenerTopScores()
    {
        string raw = PlayerPrefs.GetString(CLAVE_TOP_SCORES, "");
        List<int> lista = new List<int>();
        if (!string.IsNullOrEmpty(raw))
        {
            foreach (string parte in raw.Split(','))
            {
                if (int.TryParse(parte, out int valor))
                {
                    lista.Add(valor);
                }
            }
        }
        return lista.OrderByDescending(v => v).ToList();
    }

    private void GuardarTopScores(List<int> lista)
    {
        var ordenada = lista.OrderByDescending(v => v).Take(MAX_RECORDS).ToList();
        PlayerPrefs.SetString(CLAVE_TOP_SCORES, string.Join(",", ordenada));
    }

    public void RegistrarVictoria(int puntajeFinal, float tiempoRestante)
    {
        // 1. Sumar partida ganada
        int gamesWon = ObtenerGamesWon() + 1;
        PlayerPrefs.SetInt("GamesWon", gamesWon);

        // 2. Evaluar High Score (compatibilidad con el sistema anterior de un solo valor)
        int currentHighScore = ObtenerHighScore();
        if (puntajeFinal > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", puntajeFinal);
            Debug.Log($"¡Nuevo High Score registrado: {puntajeFinal}!");
        }

        // 2b. Guardar TODAS las puntuaciones en la lista de mejores puntuaciones
        List<int> topScores = ObtenerTopScores();
        topScores.Add(puntajeFinal);
        GuardarTopScores(topScores);

        // 3. Evaluar mejor tiempo registrado (Best Time)
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
        PlayerPrefs.DeleteKey(CLAVE_TOP_SCORES);
        PlayerPrefs.Save();

        if (GameSettingsUI.Instance != null)
        {
            GameSettingsUI.Instance.ActualizarTextosRécord();
        }
    }
}
