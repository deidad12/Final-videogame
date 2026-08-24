using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public static class GameInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InicializarJuego()
    {
        Debug.Log("Inicializando Sistemas de Configuración, Perfil y Localización...");

        // Crear un objeto persistente para albergar los managers
        GameObject managerGo = new GameObject("SystemsManager");
        Object.DontDestroyOnLoad(managerGo);

        // Añadir componentes en orden de dependencia
        managerGo.AddComponent<GameSettingsManager>();
        managerGo.AddComponent<LocalizationManager>();
        managerGo.AddComponent<GameProgressManager>();
        managerGo.AddComponent<GameSettingsUI>();

        // Escuchar la carga de escenas para inicializar el fondo y los objetos en las escenas de juego
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Garantizar que SIEMPRE exista un EventSystem en la escena.
        // Sin él, ningún Button/Slider/InputField de la UI puede recibir clics
        // (síntoma típico: "los botones no responden" o el panel de Opciones
        // parece no reaccionar al pulsar Guardar). Algunas escenas (niveles)
        // no traían uno propio, así que lo creamos aquí de forma segura e idempotente.
        AsegurarEventSystem();

        // Ignorar el menú principal
        string menuName = "MainMenu";
        if (GameManager.Instance != null)
        {
            menuName = GameManager.Instance.escenaMenu;
        }

        if (scene.name != menuName)
        {
            Debug.Log($"Generando fondo dinámico y poblando nivel para la escena: {scene.name}");

            // Crear un objeto local en la escena que contenga el fondo y el populador
            GameObject localPopulator = new GameObject("SceneDynamicPopulator");
            localPopulator.AddComponent<DynamicBackgroundManager>();
            localPopulator.AddComponent<DynamicLevelPopulator>();
        }
    }

    private static void AsegurarEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;

        GameObject eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Debug.Log("[GameInitializer] No se encontró un EventSystem en la escena: se creó uno automáticamente para que los botones respondan a los clics.");
    }
}
