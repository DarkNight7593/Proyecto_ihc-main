using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Controlador_Canasta : MonoBehaviour
{
[Header("Menú Flotante (UI)")]
    public TextMeshProUGUI textoMarcador; // Arrastra aquí el texto de tu Canvas azul
    private int puntosTotales = 0;

    [Header("Efecto de Brillo (Glow)")]
    public Light luzDestello; // Una luz Point Light colocada en el aro
    public AudioSource sonidoMalla;
    public float duracionBrillo = 0.4f; // Cuánto tiempo se queda encendida la luz

    private bool canastaMarcada = false; // Evita que una pelota sume puntos dobles por rebotar raro dentro del sensor

    void Start()
    {
        // Al iniciar, nos aseguramos de que la luz esté apagada y el marcador en 0
        if (luzDestello != null) luzDestello.enabled = false;
        ActualizarMarcadorUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("⚠️ Algo tocó el sensor del aro. Nombre del objeto: " + other.gameObject.name);
        // Detecta si lo que entró es el balón (por Tag o por Nombre)
        if ((other.CompareTag("ball") || other.name.Contains("Balon")) && !canastaMarcada)
        {
            StartCoroutine(ProcesarCanasta());
        }
    }

    IEnumerator ProcesarCanasta()
    {
        canastaMarcada = true;
        puntosTotales += 3; // ¡Puntazo! Puedes cambiarlo a 2 si quieres
        ActualizarMarcadorUI();

        // --- EFECTO DE BRILLO ---
        if (luzDestello != null)
        {
            luzDestello.enabled = true; // Enciende la luz
        }

        if (sonidoMalla != null)
        {
            sonidoMalla.Play();
        }

        // Espera el tiempo configurado
        yield return new WaitForSeconds(duracionBrillo);

        if (luzDestello != null)
        {
            luzDestello.enabled = false; // Apaga la luz
        }
        
        canastaMarcada = false; // El sensor queda listo para la siguiente jugada
    }

    void ActualizarMarcadorUI()
    {
        if (textoMarcador != null)
        {
            textoMarcador.text = "SCORE: " + puntosTotales.ToString("00");
        }
    }
}
