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
    private Rigidbody rb;

    void Start()
    {
        // Buscamos el Rigidbody en el objeto actual
        rb = GetComponent<Rigidbody>();

        // (Opcional) Si olvidaste asignar el AudioSource en el Inspector, intentamos buscarlo automáticamente
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
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
            estaEnElSuelo = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (EsUnaSuperficieValida(collision.gameObject))
        {
            estaEnElSuelo = false;
            tiempoEnElSuelo = 0f;
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
        // 1. Frenamos las físicas ANTES de moverlo para un teletransporte limpio
        if (rb != null)
        {
            rb.isKinematic = true; 
            rb.velocity = Vector3.zero;         
            rb.angularVelocity = Vector3.zero;  
        }

        // 2. Movemos el balón a la posición de retorno
        transform.position = puntoDeRetorno.position;
        transform.rotation = puntoDeRetorno.rotation;

        // 3. ¡REPRODUCIMOS EL SONIDO DE RESPAWN!
        if (audioSource != null && sonidoRespawn != null)
        {
            // PlayOneShot permite que el sonido suene de principio a fin sin cortarse
            audioSource.volume=volumenRespawn;
            audioSource.PlayOneShot(sonidoRespawn);
        }

        // 4. Volvemos a encender la física
        if (rb != null)
        {
            rb.isKinematic = false; 
        }

        // 5. Reiniciamos los contadores
        estaEnElSuelo = false;
        tiempoEnElSuelo = 0f;
    }
}
