using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode; // 🔥 NUEVO

public class RetornoBalon_Multi : NetworkBehaviour // 🔥 NUEVO
{
    [Header("Configuración de Retorno")]
    public Transform puntoDeRetorno; 

    [Header("Tiempos y Límites (Nueva Lógica)")]
    public float tiempoEnSueloMax = 1f; 
    public float tiempoEnAroMax = 3f; 
    public int maxBotesPermitidos = 3;

    [Header("Superficies Válidas (Tags)")]
    public List<string> tagsSuelo = new List<string> { "rebote_suelo", "rebote_madera" };
    public List<string> tagsAro = new List<string> { "rebote_aro" };

    [Header("Configuración de Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoRespawn;
    [Range(0f, 2f)] public float volumenRespawn = 1.5f;

    [HideInInspector] public float tiempoUltimoRetorno = 0f;

    private float cronometroSuelo = 0f;
    private float cronometroAro = 0f;
    private float tiempoDesdeUltimoBote = 0f; 
    
    private int contadorBotes = 0;
    private int contactosSuelo = 0; 
    private int contactosAro = 0; 
    
    private Rigidbody rb;
    private enum TipoSuperficie { Ninguna, Suelo, Aro }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!IsOwner) return; // 🔥 MAGIA DE RED

        if (rb != null && rb.isKinematic)
        {
            ResetearContadores();
            return;
        }

        tiempoDesdeUltimoBote += Time.deltaTime;

        if (contactosSuelo > 0)
        {
            cronometroSuelo += Time.deltaTime;
            if (cronometroSuelo >= tiempoEnSueloMax)
            {
                TeletransportarBalon();
                return;
            }
        }
        else cronometroSuelo = 0f;

        if (contactosAro > 0)
        {
            cronometroAro += Time.deltaTime;
            if (cronometroAro >= tiempoEnAroMax)
            {
                TeletransportarBalon();
                return;
            }
        }
        else cronometroAro = 0f; 
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return; // 🔥 MAGIA DE RED

        TipoSuperficie sup = EvaluarSuperficie(collision.gameObject);

        if (sup != TipoSuperficie.Ninguna)
        {
            if (tiempoDesdeUltimoBote > 0.15f)
            {
                tiempoDesdeUltimoBote = 0f;

                if (sup == TipoSuperficie.Suelo)
                {
                    contadorBotes++;
                    if (contadorBotes >= maxBotesPermitidos)
                    {
                        TeletransportarBalon();
                        return; 
                    }
                }
            }

            if (sup == TipoSuperficie.Suelo) contactosSuelo++;
            if (sup == TipoSuperficie.Aro) contactosAro++;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (!IsOwner) return; // 🔥 MAGIA DE RED
        TipoSuperficie sup = EvaluarSuperficie(collision.gameObject);
        if (sup == TipoSuperficie.Suelo && contactosSuelo == 0) contactosSuelo = 1;
        if (sup == TipoSuperficie.Aro && contactosAro == 0) contactosAro = 1;
    }

    void OnCollisionExit(Collision collision)
    {
        if (!IsOwner) return; // 🔥 MAGIA DE RED
        TipoSuperficie sup = EvaluarSuperficie(collision.gameObject);
        if (sup == TipoSuperficie.Suelo)
        {
            contactosSuelo--;
            if (contactosSuelo < 0) contactosSuelo = 0;
        }
        if (sup == TipoSuperficie.Aro)
        {
            contactosAro--;
            if (contactosAro < 0) contactosAro = 0;
        }
    }

    private TipoSuperficie EvaluarSuperficie(GameObject objeto)
    {
        if (tagsSuelo.Contains(objeto.tag)) return TipoSuperficie.Suelo;
        if (tagsAro.Contains(objeto.tag)) return TipoSuperficie.Aro;
        
        if (objeto.transform.parent != null)
        {
            GameObject padre = objeto.transform.parent.gameObject;
            if (tagsSuelo.Contains(padre.tag)) return TipoSuperficie.Suelo;
            if (tagsAro.Contains(padre.tag)) return TipoSuperficie.Aro;
        }

        return TipoSuperficie.Ninguna;
    }

    private void TeletransportarBalon()
    {
        tiempoUltimoRetorno = Time.time; 

        if (rb != null)
        {
            rb.isKinematic = true; 
            rb.velocity = Vector3.zero;         
            rb.angularVelocity = Vector3.zero;  
        }

        transform.position = puntoDeRetorno.position;
        transform.rotation = puntoDeRetorno.rotation;

        if (audioSource != null)
        {
            audioSource.Stop(); 
            if (sonidoRespawn != null)
            {
                audioSource.volume = volumenRespawn;
                audioSource.PlayOneShot(sonidoRespawn);
            }
        }

        if (rb != null) rb.isKinematic = false; 
        ResetearContadores();
    }

    private void ResetearContadores()
    {
        contactosSuelo = 0;
        contactosAro = 0;
        cronometroSuelo = 0f;
        cronometroAro = 0f;
        contadorBotes = 0;
        tiempoDesdeUltimoBote = 0f;
    }
}