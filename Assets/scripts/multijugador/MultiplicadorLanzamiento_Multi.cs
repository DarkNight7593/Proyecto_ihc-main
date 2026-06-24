using System.Collections;
using UnityEngine;
using Oculus.Interaction; 
using Unity.Netcode; // 🔥 NUEVO: Importamos Netcode

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Grabbable))] 
public class MultiplicadorLanzamiento_Multi : NetworkBehaviour // 🔥 NUEVO: Ahora es de red
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

        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised += ManejarEventosOculus;
            Debug.Log("Script de multiplicador conectado correctamente al Grabbable.");
        }
    }

    private void ManejarEventosOculus(PointerEvent evt)
    {
        // 🔥 MAGIA DE RED: Si no soy el dueño de esta pelota, no le aplico fuerzas
        if (!IsOwner) return;

        if (evt.Type == PointerEventType.Unselect)
        {
            StartCoroutine(MultiplicarConRetraso());
        }
    }

    private IEnumerator MultiplicarConRetraso()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        if (Physics.gravity.y > -9.0f) 
        {
            Debug.Log("🚀 Gravedad Lunar detectada. Se desactiva el multiplicador.");
            yield break; 
        }

        rb.velocity = rb.velocity * multiplicadorVelocidad;
        rb.angularVelocity = rb.angularVelocity * 1.2f; 
        
        Debug.Log("¡Velocidad multiplicada! Nueva velocidad Y: " + rb.velocity.y);
    }

    private void OnDestroy()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= ManejarEventosOculus;
        }
    }
}