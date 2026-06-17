using System.Collections;
using UnityEngine;
using Oculus.Interaction; // Importamos la librería de Oculus

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Grabbable))] // Aseguramos que tenga el Grabbable de Meta
public class MultiplicadorLanzamiento : MonoBehaviour
{
    private Rigidbody rb;
    private Grabbable grabbable;
    
    [Header("Ajustes de Lanzamiento")]
    [Tooltip("Multiplica la velocidad al soltar el balón. Prueba valores entre 1.5 y 3.0")]
    public float multiplicadorVelocidad = 2.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabbable = GetComponent<Grabbable>();

        // Nos suscribimos a los eventos nativos del Grabbable de Oculus
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised += ManejarEventosOculus;
            Debug.Log("Script de multiplicador conectado correctamente al Grabbable.");
        }
    }

    // Esta función escucha todo lo que le pasa al balón
    private void ManejarEventosOculus(PointerEvent evt)
    {
        // Si el evento es "Unselect" (soltar el objeto)
        if (evt.Type == PointerEventType.Unselect)
        {
            StartCoroutine(MultiplicarConRetraso());
        }
    }

    private IEnumerator MultiplicarConRetraso()
    {
        // Esperamos a que Oculus suelte el objeto y asigne la velocidad física
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // 🌟 VALIDACIÓN DE GRAVEDAD 🌟
        // Dependiendo de cómo definiste tu script de Gravedad en el menú de pausa:
        // Si usas un booleano, un Enum en el ControladorJuego, o lees la gravedad física del proyecto.
        // Aquí le decimos: Si la gravedad global es menor a la normal, NO multipliques (evitamos el misil).
        if (Physics.gravity.y > -9.0f) 
        {
            Debug.Log("🚀 Gravedad Lunar detectada. Se desactiva el multiplicador para evitar efecto misil.");
            yield break; // Cancela el resto de la corutina inmediatamente
        }

        // Si la gravedad está normal (-9.81), se ejecuta el código original perfectamente:
        rb.velocity = rb.velocity * multiplicadorVelocidad;
        rb.angularVelocity = rb.angularVelocity * 1.2f; 
        
        Debug.Log("¡Velocidad multiplicada! Nueva velocidad Y: " + rb.velocity.y);
    }

    // Buena práctica: desconectar el evento si el objeto se destruye
    private void OnDestroy()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= ManejarEventosOculus;
        }
    }
}