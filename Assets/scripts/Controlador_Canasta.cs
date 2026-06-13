using System.Collections;
using UnityEngine;

public class Controlador_Canasta : MonoBehaviour
{
    [Header("Efecto de Brillo (Glow)")]
    public Light luzDestello;
    public AudioSource sonidoMalla;
    public float duracionBrillo = 0.4f;

    private bool canastaMarcada = false;

    void Start()
    {
        if (luzDestello != null) luzDestello.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("⚠️ Algo tocó el sensor del aro: " + other.gameObject.name);
        
        if ((other.CompareTag("ball") || other.name.Contains("Balon")) && !canastaMarcada)
        {
            StartCoroutine(ProcesarCanasta());
        }
    }

    IEnumerator ProcesarCanasta()
    {
        canastaMarcada = true;

        // 1. LE AVISAMOS AL CEREBRO QUE ANOTAMOS (usando el Singleton)
        if (ControladorJuego.Instancia != null)
        {
            ControladorJuego.Instancia.RegistrarCanasta();
        }

        // 2. REPRODUCIMOS EFECTOS LOCALES
        if (luzDestello != null) luzDestello.enabled = true;
        if (sonidoMalla != null) sonidoMalla.Play();

        // 3. ESPERAMOS
        yield return new WaitForSeconds(duracionBrillo);

        // 4. APAGAMOS LUZ Y REINICIAMOS
        if (luzDestello != null) luzDestello.enabled = false;
        canastaMarcada = false; 
    }
}