using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackBalon : MonoBehaviour
{
    [Header("Configuración de Vibración")]
    [Range(0f, 1f)] public float frecuencia = 1f;   
    [Range(0f, 1f)] public float amplitud = 0.5f;   
    public float duracion = 0.15f;                  

    public void ActivarVibracion()
    {
        // 1. Guardamos el mando activo en una variable para imprimirlo en consola
        OVRInput.Controller mandoActivo = OVRInput.Controller.Active;

        // 2. Printeamos que la función fue llamada correctamente
        Debug.Log($"📳 [VIBRACIÓN] Iniciando... Mando detectado: [{mandoActivo}] | Amplitud: {amplitud} | Duración: {duracion}s");

        // 3. Failsafe para el simulador: Si el simulador no detecta un mando físico, te avisa
        if (mandoActivo == OVRInput.Controller.None)
        {
            Debug.LogWarning("⚠️ [VIBRACIÓN] El sistema detectó 'None' como mando. Si estás en el Simulador o usando Manos, es normal. La orden se ejecutó, pero no vibrará nada físico.");
        }

        // 4. Activamos la vibración
        OVRInput.SetControllerVibration(frecuencia, amplitud, mandoActivo);
        
        // 5. Programamos el apagado
        Invoke("ApagarVibracion", duracion);
    }

    private void ApagarVibracion()
    {
        OVRInput.Controller mandoActivo = OVRInput.Controller.Active;
        
        // Printeamos que el tiempo pasó y se está apagando
        Debug.Log($"🛑 [VIBRACIÓN] Tiempo agotado ({duracion}s). Apagando vibración del mando: [{mandoActivo}]");
        
        OVRInput.SetControllerVibration(0f, 0f, mandoActivo);
    }
}