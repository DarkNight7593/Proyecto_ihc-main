using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode; // 🔥 NUEVO

public class FeedbackBalon_Multi : NetworkBehaviour // 🔥 NUEVO
{
    [Header("Configuración de Vibración")]
    [Range(0f, 1f)] public float frecuencia = 1f;   
    [Range(0f, 1f)] public float amplitud = 0.5f;   
    public float duracion = 0.15f;                  

    public void ActivarVibracion()
    {
        if (!IsOwner) return; // 🔥 MAGIA DE RED: Solo vibra el mando del dueño

        OVRInput.Controller mandoActivo = OVRInput.Controller.Active;
        Debug.Log($"📳 [VIBRACIÓN] Iniciando... Mando detectado: [{mandoActivo}]");

        if (mandoActivo == OVRInput.Controller.None)
        {
            Debug.LogWarning("⚠️ [VIBRACIÓN] Simulador o Manos detectadas.");
        }

        OVRInput.SetControllerVibration(frecuencia, amplitud, mandoActivo);
        Invoke("ApagarVibracion", duracion);
    }

    private void ApagarVibracion()
    {
        if (!IsOwner) return; // 🔥 MAGIA DE RED

        OVRInput.Controller mandoActivo = OVRInput.Controller.Active;
        OVRInput.SetControllerVibration(0f, 0f, mandoActivo);
    }
}