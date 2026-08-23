using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private Dictionary<string, Dictionary<string, string>> traducciones;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InicializarTraducciones();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void AlCargarEscena(Scene scene, LoadSceneMode mode)
    {
        TraducirInterfazActual();
    }

    private void InicializarTraducciones()
    {
        traducciones = new Dictionary<string, Dictionary<string, string>>();

        // --- DICCIONARIO ESPAÑOL ---
        var es = new Dictionary<string, string>();
        es["Jugar"] = "Jugar";
        es["Opciones"] = "Ajustes y Perfil";
        es["Salir"] = "Salir";
        es["Volver"] = "Volver";
        es["Vidas"] = "Vidas";
        es["Tiempo"] = "Tiempo";
        es["Objetos"] = "Objetos";
        es["Puntaje"] = "Puntaje";
        es["Estudiante:"] = "Estudiante:";
        es["Matrícula:"] = "Matrícula:";
        es["¡Te quedaste sin vidas!"] = "¡Te quedaste sin vidas!";
        es["¡Se agotó el tiempo!"] = "¡Se agotó el tiempo!";
        es["¡Has muerto!"] = "¡Has muerto!";
        es["¡Victoria!"] = "¡Victoria!";
        es["Regresando al Nivel 1..."] = "Regresando al Nivel 1...";
        es["Reiniciando Nivel..."] = "Reiniciando Nivel...";
        es["Configuración de Sonido"] = "Configuración de Sonido";
        es["Volumen Música"] = "Volumen de Música";
        es["Volumen SFX"] = "Volumen de Efectos";
        es["Guardar y Cerrar"] = "Guardar y Cerrar";
        es["Perfil de Jugador"] = "Perfil de Jugadores";
        es["Nombre P1:"] = "Nombre Jugador 1:";
        es["Nombre P2:"] = "Nombre Jugador 2:";
        es["Avatar P1:"] = "Avatar Jugador 1:";
        es["Avatar P2:"] = "Avatar Jugador 2:";
        es["Multijugador (Pantalla Dividida)"] = "Multijugador (Pantalla Dividida)";
        es["Multijugador:"] = "Multijugador:";
        es["Modo Oscuro:"] = "Modo Oscuro:";
        es["CLARO"] = "CLARO";
        es["OSCURO"] = "OSCURO";
        es["Instrucciones de Juego"] = "Instrucciones de Juego";
        es["Controles"] = "Controles";
        es["Objetivo:"] = "Objetivo:";
        es["InstruccionesContenido"] = "Jugador 1: A/D Mover, W/Espacio Saltar\nJugador 2: Flechas Mover, Flecha Arriba Saltar\n\nObjetivo: Recolecta caramelos, esquiva trampas y derrota al malvado Dr. Cocoa.";
        
        traducciones["ES"] = es;

        // --- DICCIONARIO INGLÉS ---
        var en = new Dictionary<string, string>();
        en["Jugar"] = "Play";
        en["Opciones"] = "Settings & Profile";
        en["Salir"] = "Quit";
        en["Volver"] = "Back";
        en["Vidas"] = "Lives";
        en["Tiempo"] = "Time";
        en["Objetos"] = "Items";
        en["Puntaje"] = "Score";
        en["Estudiante:"] = "Student:";
        en["Matrícula:"] = "ID:";
        en["¡Te quedaste sin vidas!"] = "No lives left!";
        en["¡Se agotó el tiempo!"] = "Time is up!";
        en["¡Has muerto!"] = "You died!";
        en["¡Victoria!"] = "Victory!";
        en["Regresando al Nivel 1..."] = "Returning to Level 1...";
        en["Reiniciando Nivel..."] = "Restarting Level...";
        en["Configuración de Sonido"] = "Sound Configuration";
        en["Volumen Música"] = "Music Volume";
        en["Volumen SFX"] = "SFX Volume";
        en["Guardar y Cerrar"] = "Save & Close";
        en["Perfil de Jugador"] = "Player Profile";
        en["Nombre P1:"] = "P1 Name:";
        en["Nombre P2:"] = "P2 Name:";
        en["Avatar P1:"] = "P1 Avatar:";
        en["Avatar P2:"] = "P2 Avatar:";
        en["Multijugador (Pantalla Dividida)"] = "Split-Screen Multiplayer";
        en["Multijugador:"] = "Multiplayer:";
        en["Modo Oscuro:"] = "Dark Mode:";
        en["CLARO"] = "LIGHT";
        en["OSCURO"] = "DARK";
        en["Instrucciones de Juego"] = "Game Instructions";
        en["Controles"] = "Controls";
        en["Objetivo:"] = "Goal:";
        en["InstruccionesContenido"] = "Player 1: A/D to Move, W/Space to Jump\nPlayer 2: Arrows to Move, Up Arrow to Jump\n\nGoal: Collect candies, avoid obstacles, and defeat the evil Dr. Cocoa.";

        traducciones["EN"] = en;
    }

    public string ObtenerTexto(string clave)
    {
        string lang = "ES";
        if (GameSettingsManager.Instance != null)
        {
            lang = GameSettingsManager.Instance.lenguaje;
        }

        if (traducciones.ContainsKey(lang) && traducciones[lang].ContainsKey(clave))
        {
            return traducciones[lang][clave];
        }

        // Retornar la clave si no hay traducción
        return clave;
    }

    // Traduce un texto buscando si coincide parcialmente o es igual a una clave en español
    public string TraducirCadena(string originalText)
    {
        if (string.IsNullOrEmpty(originalText)) return originalText;

        string lang = "ES";
        if (GameSettingsManager.Instance != null)
        {
            lang = GameSettingsManager.Instance.lenguaje;
        }

        // Si es español, devolver texto original (nuestras claves base están en español)
        if (lang == "ES") return originalText;

        // Limpiar espacios laterales
        string trimmed = originalText.Trim();

        // Buscar correspondencia exacta en el diccionario español para obtener la clave
        foreach (var par in traducciones["ES"])
        {
            if (par.Value.Equals(trimmed, System.StringComparison.OrdinalIgnoreCase) || par.Key.Equals(trimmed, System.StringComparison.OrdinalIgnoreCase))
            {
                // Devolver el valor traducido al idioma actual
                if (traducciones[lang].ContainsKey(par.Key))
                {
                    return traducciones[lang][par.Key];
                }
            }
        }

        // Si no hay correspondencia exacta, buscaremos traducciones parciales para HUDs dinámicos como "Tiempo: 60s", "Vidas: 3", "Objetos: 0"
        if (trimmed.StartsWith("Tiempo:", System.StringComparison.OrdinalIgnoreCase))
        {
            string resto = trimmed.Substring(7);
            return ObtenerTexto("Tiempo") + ":" + resto;
        }
        if (trimmed.StartsWith("Vidas:", System.StringComparison.OrdinalIgnoreCase))
        {
            string resto = trimmed.Substring(6);
            return ObtenerTexto("Vidas") + ":" + resto;
        }
        if (trimmed.StartsWith("Objetos:", System.StringComparison.OrdinalIgnoreCase))
        {
            string resto = trimmed.Substring(8);
            return ObtenerTexto("Objetos") + ":" + resto;
        }
        if (trimmed.StartsWith("Puntaje:", System.StringComparison.OrdinalIgnoreCase))
        {
            string resto = trimmed.Substring(8);
            return ObtenerTexto("Puntaje") + ":" + resto;
        }
        if (trimmed.StartsWith("Score:", System.StringComparison.OrdinalIgnoreCase))
        {
            string resto = trimmed.Substring(6);
            return ObtenerTexto("Puntaje") + ":" + resto;
        }
        if (trimmed.Contains("Regresando al Nivel 1..."))
        {
            return trimmed.Replace("Regresando al Nivel 1...", ObtenerTexto("Regresando al Nivel 1..."));
        }
        if (trimmed.Contains("Reiniciando Nivel..."))
        {
            return trimmed.Replace("Reiniciando Nivel...", ObtenerTexto("Reiniciando Nivel..."));
        }

        return originalText;
    }

    public void TraducirInterfazActual()
    {
        TextMeshProUGUI[] todosLosTextos = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI tmp in todosLosTextos)
        {
            // Ignorar textos de matrícula o alumno si están pre-definidos o no queremos traducirlos, pero traduzcamos las etiquetas
            if (tmp.text.StartsWith("Estudiante:") || tmp.text.StartsWith("Student:"))
            {
                string info = tmp.text.Contains("\n") ? tmp.text.Substring(tmp.text.IndexOf("\n")) : "";
                string label = ObtenerTexto("Estudiante:");
                tmp.text = label + " " + (GameSettingsManager.Instance != null ? GameSettingsManager.Instance.nombreP1 : "Player 1") + info;
                continue;
            }
            if (tmp.text.StartsWith("Matrícula:") || tmp.text.StartsWith("ID:"))
            {
                string label = ObtenerTexto("Matrícula:");
                tmp.text = label + " " + (GameManager.Instance != null ? GameManager.Instance.matriculaAlumno : "Plataformas 2D");
                continue;
            }

            // Traducir el texto usando nuestro diccionario
            string traducido = TraducirCadena(tmp.text);
            tmp.text = traducido;
        }
    }
}
