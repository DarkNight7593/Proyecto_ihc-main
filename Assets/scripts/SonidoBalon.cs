using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SonidoBalon : MonoBehaviour
{
    [Header("Configuración de Audio")]
    public AudioSource audioSource; // El componente que reproducirá los sonidos

    [Header("Biblioteca de Sonidos (Clips)")]
    public AudioClip sonidoMadera; // Para el tablero o estructuras de madera
    public AudioClip sonidoSuelo;   // Para el piso exterior o asfalto
    public AudioClip sonidoAro;     // Para el metal del aro

    [Header("Ajustes del Bote")]
    public float fuerzaMinimaParaSonido = 0.5f;
    public float fuerzaMaximaParaVolumen = 8.0f;

    private void OnCollisionEnter(Collision collision)
    {
        // === DETECTIVE DE CONSOLA (PASO 1: ¿Hay colisión?) ===
        string nombreObjeto = collision.gameObject.name;
        string tagObjeto = collision.gameObject.tag;
        string tagPadre = (collision.transform.parent != null) ? collision.transform.parent.gameObject.tag : "No tiene padre";
        float fuerzaImpacto = collision.relativeVelocity.magnitude;

        Debug.Log($"💥 ¡COLISIÓN DETECTADA! -> Objeto: [{nombreObjeto}] | Tag Propio: [{tagObjeto}] | Tag del Padre: [{tagPadre}] | Fuerza: {fuerzaImpacto}");

        // 1. FILTRO DEL ESTANTE
        if (collision.gameObject.name.Contains("Estante") || collision.gameObject.CompareTag("Estante"))
        {
            Debug.Log("🚫 Colisión ignorada porque es el Estante.");
            return; 
        }

        // 2. FILTRO DE FUERZA MÍNIMA
        if (fuerzaImpacto < fuerzaMinimaParaSonido) 
        {
            Debug.Log($"🤫 Colisión muy débil ({fuerzaImpacto} < {fuerzaMinimaParaSonido}). No hace sonido.");
            return;
        }

        // 3. Calcular volumen dinámico
        float volumenCalculado = Mathf.InverseLerp(fuerzaMinimaParaSonido, fuerzaMaximaParaVolumen, fuerzaImpacto);

        // 4. DETECTAR EL TAG Y ASIGNAR EL SONIDO CORRECTO
        AudioClip clipA_Reproducir = null;
        string tagDetectado = ObtenerTagValido(collision.gameObject);

        Debug.Log($"🔍 El script determinó que el Tag válido final es: [{tagDetectado}]");

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
                clipA_Reproducir = null; // Cambiado a null para saber si cae en el default y no suena
                break;
        }

        // === PASO 5: VERIFICACIÓN FINAL DEL AUDIO ===
        if (audioSource == null)
        {
            Debug.LogError("❌ ERROR: ¡No has arrastrado el AudioSource a la casilla del script en el Inspector!");
            return;
        }

        if (clipA_Reproducir == null)
        {
            Debug.LogWarning($"⚠️ El Tag [{tagDetectado}] no coincide con ningún caso o cayó en 'default'. No se asignó ningún audio.");
            return;
        }

        // Si pasa todas las pruebas, reproduce
        audioSource.volume = volumenCalculado;
        audioSource.PlayOneShot(clipA_Reproducir);
        Debug.Log($"🎵 ¡REPRODUCIENDO AUDIO! Clip: [{clipA_Reproducir.name}] | Volumen: {volumenCalculado}");
    }

    private string ObtenerTagValido(GameObject objeto)
    {
        if (objeto.CompareTag("rebote_madera") || objeto.CompareTag("rebote_suelo") || objeto.CompareTag("rebote_aro"))
        {
            return objeto.tag;
        }
        
        if (objeto.transform.parent != null)
        {
            GameObject padre = objeto.transform.parent.gameObject;
            if (padre.CompareTag("rebote_madera") || padre.CompareTag("rebote_suelo") || padre.CompareTag("rebote_aro"))
            {
                return padre.tag;
            }
        }

        return "Ninguno";
    }
}
