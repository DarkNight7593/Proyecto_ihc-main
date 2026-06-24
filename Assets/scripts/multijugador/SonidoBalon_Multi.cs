using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonidoBalon_Multi : MonoBehaviour
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

    // 🔥 NUEVO: Enlace con el script de retorno MULTI
    private RetornoBalon_Multi retornoBalon;

    void Start()
    {
        // Buscamos el nuevo script
        retornoBalon = GetComponent<RetornoBalon_Multi>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (retornoBalon != null && (Time.time - retornoBalon.tiempoUltimoRetorno) < 0.1f)
        {
            return;
        }

        string nombreObjeto = collision.gameObject.name;
        float fuerzaImpacto = collision.relativeVelocity.magnitude;

        if (nombreObjeto.Contains("Estante") || collision.gameObject.CompareTag("Estante")) return; 
        if (fuerzaImpacto < fuerzaMinimaParaSonido) return;

        float volumenCalculado = Mathf.InverseLerp(fuerzaMinimaParaSonido, fuerzaMaximaParaVolumen, fuerzaImpacto);
        AudioClip clipA_Reproducir = null;
        string tagDetectado = ObtenerTagValido(collision.gameObject);

        switch (tagDetectado)
        {
            case "rebote_madera": clipA_Reproducir = sonidoMadera; break;
            case "rebote_suelo": clipA_Reproducir = sonidoSuelo; break;
            case "rebote_aro": clipA_Reproducir = sonidoAro; break;
            default: clipA_Reproducir = null; break;
        }

        if (audioSource == null || clipA_Reproducir == null) return;

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