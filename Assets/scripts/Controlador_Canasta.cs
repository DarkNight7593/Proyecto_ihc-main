using System.Collections;
using UnityEngine;

public class Controlador_Canasta : MonoBehaviour
{
    [Header("Efecto de Brillo (Glow)")]
    public Light luzDestello;
    public AudioSource sonidoMalla;
    public float duracionBrillo = 0.4f;

    [Header("Textos Flotantes")]
    public GameObject prefabTextoPuntos;

    [Header("Ajustes para Aro Dinámico")]
    [Tooltip("Tiempo en segundos que el aro ignorará el balón tras anotar")]
    public float tiempoBloqueoCanasta = 1.8f; 

    private bool canastaMarcada = false;

    void Start()
    {
        if (luzDestello != null) luzDestello.enabled = false;
    }

    // 🌟 TRUCO DE SEGURIDAD PARA MENÚS DE PAUSA 🌟
    // Cada vez que la canasta se encienda (al cambiar entre Fijo/Dinámico),
    // nos aseguramos de resetear el candado para que no se quede bloqueada.
    void OnEnable()
    {
        canastaMarcada = false;
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

        // 1. LE AVISAMOS AL CEREBRO
        if (ControladorJuego.Instancia != null)
        {
            ControladorJuego.Instancia.RegistrarCanasta();
        }

        // 2. REPRODUCIMOS EFECTOS Y LANZAMOS TEXTO FLOTANTE
        if (luzDestello != null) luzDestello.enabled = true;
        if (sonidoMalla != null) sonidoMalla.Play();

        if (prefabTextoPuntos != null)
        {
            // Aparece a 0.3m del aro (más bajito, como pediste)
            GameObject txt = Instantiate(prefabTextoPuntos, transform.position + (Vector3.up * 0.3f), Quaternion.identity);
            TextoFlotante scriptTxt = txt.GetComponent<TextoFlotante>();

            if (scriptTxt != null)
            {
                if (ControladorJuego.Instancia.modoActual == ModoJuego.Normal)
                {
                    Color amarilloDorado = new Color(1f, 0.7f, 0f);
                    scriptTxt.Configurar("+3", amarilloDorado); 
                }
                else
                {
                    scriptTxt.Configurar("+1", Color.green); 
                }
            }
        }

        // 3. ESPERAMOS LA DURACIÓN DEL BRILLO
        yield return new WaitForSeconds(duracionBrillo);
        if (luzDestello != null) luzDestello.enabled = false;

        // 4. TRUCO DE SEGURIDAD
        float tiempoRestanteDeBloqueo = tiempoBloqueoCanasta - duracionBrillo;
        if (tiempoRestanteDeBloqueo > 0)
        {
            yield return new WaitForSeconds(tiempoRestanteDeBloqueo);
        }

        // 5. REINICIAMOS EL SENSOR
        canastaMarcada = false; 
    }
}