using UnityEngine;
        using UnityEngine.UI;
        using TMPro;
        
        // Muestra y anima la barra de vida del jefe final en el HUD.
        // Se auto-oculta si no hay jefe en la escena, y desaparece con un fade al derrotarlo.
        public class BossHealthUI : MonoBehaviour
        {
            [Header("Referencias (se auto-detectan si se dejan vacias)")]
            public BossController boss;
            public Slider barraVida;
            public Image imagenRelleno;
            public TextMeshProUGUI textoNombreJefe;
            public CanvasGroup canvasGroup;
        
            [Header("Configuracion")]
            public string nombreJefe = "Dr. Cocoa";
            public Color colorVidaAlta = new Color(0.5f, 0.85f, 0.4f);
            public Color colorVidaMedia = new Color(1f, 0.75f, 0.3f);
            public Color colorVidaBaja = new Color(0.95f, 0.35f, 0.35f);
            public float velocidadInterpolacion = 6f;
        
            private float vidaObjetivoNormalizada = 1f;
        
            void Start()
            {
                if (boss == null)
                {
                    boss = FindObjectOfType<BossController>();
                }
        
                if (canvasGroup == null)
                {
                    canvasGroup = GetComponent<CanvasGroup>();
                }
        
                if (boss == null)
                {
                    if (canvasGroup != null) canvasGroup.alpha = 0f;
                    gameObject.SetActive(false);
                    return;
                }
        
                if (textoNombreJefe != null)
                {
                    textoNombreJefe.text = nombreJefe;
                }
        
                boss.OnVidaCambiada += ManejarCambioVida;
                ManejarCambioVida(boss.VidaActual, boss.VidaMaxima);
        
                if (canvasGroup != null) canvasGroup.alpha = 1f;
            }
        
            void OnDestroy()
            {
                if (boss != null)
                {
                    boss.OnVidaCambiada -= ManejarCambioVida;
                }
            }
        
            void Update()
            {
                if (barraVida == null) return;
        
                barraVida.value = Mathf.Lerp(barraVida.value, vidaObjetivoNormalizada, Time.deltaTime * velocidadInterpolacion);
        
                if (imagenRelleno != null)
                {
                    if (barraVida.value > 0.6f) imagenRelleno.color = colorVidaAlta;
                    else if (barraVida.value > 0.3f) imagenRelleno.color = colorVidaMedia;
                    else imagenRelleno.color = colorVidaBaja;
                }
        
                if (boss == null && canvasGroup != null && canvasGroup.alpha > 0f)
                {
                    canvasGroup.alpha -= Time.deltaTime * 1.5f;
                    if (canvasGroup.alpha <= 0f)
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
        
            private void ManejarCambioVida(int vidaActual, int vidaMaxima)
            {
                vidaObjetivoNormalizada = vidaMaxima > 0 ? (float)vidaActual / vidaMaxima : 0f;
            }
        }
        