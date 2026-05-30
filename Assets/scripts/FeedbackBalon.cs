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
        // Controller.Active detecta automáticamente el último mando que presionó el botón de agarre
        OVRInput.SetControllerVibration(frecuencia, amplitud, OVRInput.Controller.Active);
        Invoke("ApagarVibracion", duracion);
    }

    private void ApagarVibracion()
    {
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.Active);
    }
}
