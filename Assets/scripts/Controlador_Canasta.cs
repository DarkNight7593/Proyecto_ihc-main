using System.Collections;
using UnityEngine;

public class Controlador_Canasta : MonoBehaviour
{
    [Header("Efecto de Brillo (Glow)")]
    public Light luzDestello;
    public AudioSource sonidoMalla;
    public float duracionBrillo = 0.4f;

    [Header("Ajustes para Aro Dinámico")]
    [Tooltip("Tiempo en segundos que el aro ignorará el balón tras anotar para evitar dobles canastas (Recomendado: 1.5 a 2 segundos)")]
    public float tiempoBloqueoCanasta = 1.8f; 

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

        // 3. ESPERAMOS LA DURACIÓN DEL BRILLO PARA APAGAR LA LUZ
        yield return new WaitForSeconds(duracionBrillo);
        if (luzDestello != null) luzDestello.enabled = false;

        // 4. TRUCO DE SEGURIDAD: Esperamos un tiempo extra total antes de permitir otra canasta.
        // Esto le da tiempo de sobra al balón para caer por completo y alejarse del aro dinámico.
        float tiempoRestanteDeBloqueo = tiempoBloqueoCanasta - duracionBrillo;
        if (tiempoRestanteDeBloqueo > 0)
        {
            yield return new WaitForSeconds(tiempoRestanteDeBloqueo);
        }

        // 5. REINICIAMOS EL SENSOR
        canastaMarcada = false; 
    }
}