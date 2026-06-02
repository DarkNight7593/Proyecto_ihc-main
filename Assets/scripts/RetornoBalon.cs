using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetornoBalon : MonoBehaviour
{
    [Header("Configuración de Retorno")]
    [Tooltip("El objeto vacío donde reaparecerá el balón")]
    public Transform puntoDeRetorno; 
    public float tiempoParaRetorno = 3f; // Tiempo en segundos

    [Header("Superficies Válidas (Tags)")]
    [Tooltip("Añade aquí los tags que se consideran suelo para que el balón cuente el tiempo")]
    public List<string> tagsConsideradosSuelo = new List<string> { "rebote_suelo", "rebote_madera" };

    [Header("Configuración de Audio")]
    [Tooltip("El componente AudioSource que reproducirá el sonido")]
    public AudioSource audioSource;
    [Tooltip("El clip de sonido que sonará al reaparecer")]
    public AudioClip sonidoRespawn;
    [Range(0f, 2f)] public float volumenRespawn = 1.5f;

    private float tiempoEnElSuelo = 0f;
    private bool estaEnElSuelo = false;
    
    // NUEVO: Contador para saber cuántos objetos de suelo estamos tocando al mismo tiempo
    private int contactosSuelo = 0; 
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        // PREVENCIÓN: Si el jugador agarra el balón, Meta XR suele poner el Rigidbody en Kinematic.
        // Si es Kinematic (fuera de nuestro teletransporte), detenemos el contador para que no desaparezca de tus manos.
        if (rb != null && rb.isKinematic)
        {
            ResetearContadores();
            return;
        }

        if (estaEnElSuelo)
        {
            tiempoEnElSuelo += Time.deltaTime;

            if (tiempoEnElSuelo >= tiempoParaRetorno)
            {
                TeletransportarBalon();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (EsUnaSuperficieValida(collision.gameObject))
        {
            contactosSuelo++; // Sumamos un contacto válido
            estaEnElSuelo = true;
        }
    }

    // NUEVO: Failsafe (Salvavidas) 
    // Si Unity por algún motivo "pierde" el evento Enter, el Stay nos asegura que sepa que está en el suelo.
    void OnCollisionStay(Collision collision)
    {
        if (!estaEnElSuelo && EsUnaSuperficieValida(collision.gameObject))
        {
            contactosSuelo = 1;
            estaEnElSuelo = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (EsUnaSuperficieValida(collision.gameObject))
        {
            contactosSuelo--; // Restamos un contacto

            // Solo si dejamos de tocar TODOS los suelos válidos, detenemos el reloj
            if (contactosSuelo <= 0)
            {
                ResetearContadores();
            }
        }
    }

    private bool EsUnaSuperficieValida(GameObject objeto)
    {
        if (tagsConsideradosSuelo.Contains(objeto.tag))
        {
            return true;
        }
        
        if (objeto.transform.parent != null)
        {
            GameObject padre = objeto.transform.parent.gameObject;
            if (tagsConsideradosSuelo.Contains(padre.tag))
            {
                return true;
            }
        }

        return false;
    }

    private void TeletransportarBalon()
    {
        if (rb != null)
        {
            rb.isKinematic = true; 
            rb.velocity = Vector3.zero;         
            rb.angularVelocity = Vector3.zero;  
        }

        transform.position = puntoDeRetorno.position;
        transform.rotation = puntoDeRetorno.rotation;

        if (audioSource != null && sonidoRespawn != null)
        {
            audioSource.volume = volumenRespawn;
            audioSource.PlayOneShot(sonidoRespawn);
        }

        if (rb != null)
        {
            rb.isKinematic = false; 
        }

        ResetearContadores();
    }

    // NUEVO: Función auxiliar para limpiar variables de forma segura
    private void ResetearContadores()
    {
        contactosSuelo = 0;
        estaEnElSuelo = false;
        tiempoEnElSuelo = 0f;
    }
}