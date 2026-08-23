using UnityEngine;

public class ObstacleShooter : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject prefabProyectil;
    public Transform puntoDisparo;
    public float intervaloDisparo = 2f;
    public Vector2 direccionDisparo = Vector2.left;
    public float velocidadProyectil = 5f;

    private float tiempoSiguienteDisparo;

    void Start()
    {
        tiempoSiguienteDisparo = Time.time + intervaloDisparo + Random.Range(0f, 1f); // Desfase inicial
    }

    void Update()
    {
        // No disparar si el juego ya terminó
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded()) return;

        if (Time.time >= tiempoSiguienteDisparo)
        {
            Disparar();
            tiempoSiguienteDisparo = Time.time + intervaloDisparo;
        }
    }

    void Disparar()
    {
        if (prefabProyectil == null) return;

        Vector3 spawnPos = puntoDisparo != null ? puntoDisparo.position : transform.position;
        GameObject proyectilGo = Instantiate(prefabProyectil, spawnPos, Quaternion.identity);
        
        Projectile proj = proyectilGo.GetComponent<Projectile>();
        if (proj == null)
        {
            proj = proyectilGo.AddComponent<Projectile>();
        }
        
        proj.Inicializar(direccionDisparo, velocidadProyectil);
    }
}
