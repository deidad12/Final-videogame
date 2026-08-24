using UnityEngine;

public class VolverMenu : MonoBehaviour
{
    public GameObject menuActual;
    public GameObject menuAnterior;

    public void Volver()
    {
        menuActual.SetActive(false);
        menuAnterior.SetActive(true);
    }
}