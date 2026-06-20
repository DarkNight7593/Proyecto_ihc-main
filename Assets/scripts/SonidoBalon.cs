using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonidoBalon : MonoBehaviour
{
    [Header("Configuración de Audio")]
    public AudioSource audioSource; 

    [Header("Biblioteca de Sonidos (Clips)")]
    public AudioClip sonidoMadera; 
    public AudioClip sonidoSuelo;   
    public AudioClip sonidoAro;     

    [Header("Ajustes del Bote")]
    public float fuerzaMinimaParaSonido = 0.5f;
    public float fuerzaMaximaParaVolumen = 8.0f;

    // 🔥 NUEVO: Enlace con el script de retorno
    private RetornoBalon retornoBalon;

    void Start()
    {
        // Buscamos el script de retorno en el mismo balón
        retornoBalon = GetComponent<RetornoBalon>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 🔥 FILTRO MAESTRO: Si el balón se acaba de teletransportar (hace menos de 0.1 segundos), 
        // bloqueamos CUALQUIER sonido de impacto para que no te explote en la oreja.
        if (retornoBalon != null && (Time.time - retornoBalon.tiempoUltimoRetorno) < 0.1f)
        {
            Debug.Log("🤫 Silenciando bote porque el balón está haciendo respawn.");
            return;
        }

        // === DETECTIVE DE CONSOLA ===
        string nombreObjeto = collision.gameObject.name;
        string tagObjeto = collision.gameObject.tag;
        string tagPadre = (collision.transform.parent != null) ? collision.transform.parent.gameObject.tag : "No tiene padre";
        float fuerzaImpacto = collision.relativeVelocity.magnitude;

        // 1. FILTRO DEL ESTANTE
        if (collision.gameObject.name.Contains("Estante") || collision.gameObject.CompareTag("Estante"))
        {
            return; 
        }

        // 2. FILTRO DE FUERZA MÍNIMA
        if (fuerzaImpacto < fuerzaMinimaParaSonido) 
        {
            return;
        }

        // 3. Calcular volumen dinámico
        float volumenCalculado = Mathf.InverseLerp(fuerzaMinimaParaSonido, fuerzaMaximaParaVolumen, fuerzaImpacto);

        // 4. DETECTAR EL TAG Y ASIGNAR EL SONIDO CORRECTO
        AudioClip clipA_Reproducir = null;
        string tagDetectado = ObtenerTagValido(collision.gameObject);

        switch (tagDetectado)
        {
            case "rebote_madera":
                clipA_Reproducir = sonidoMadera;
                break;
            case "rebote_suelo":
                clipA_Reproducir = sonidoSuelo;
                break;
            case "rebote_aro":
                clipA_Reproducir = sonidoAro;
                break;
            default:
                clipA_Reproducir = null; 
                break;
        }

        // 5. VERIFICACIÓN FINAL DEL AUDIO
        if (audioSource == null || clipA_Reproducir == null) return;

        // Si pasa todas las pruebas, reproduce
        audioSource.volume = volumenCalculado;
        audioSource.PlayOneShot(clipA_Reproducir);
    }

    private string ObtenerTagValido(GameObject objeto)
    {
        if (objeto.CompareTag("rebote_madera") || objeto.CompareTag("rebote_suelo") || objeto.CompareTag("rebote_aro"))
            return objeto.tag;
        
        if (objeto.transform.parent != null)
        {
            GameObject padre = objeto.transform.parent.gameObject;
            if (padre.CompareTag("rebote_madera") || padre.CompareTag("rebote_suelo") || padre.CompareTag("rebote_aro"))
                return padre.tag;
        }

        return "Ninguno";
    }
}