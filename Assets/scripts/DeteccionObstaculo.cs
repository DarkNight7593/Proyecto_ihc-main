using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DeteccionObstaculo : MonoBehaviour
{
    [Header("Efectos de Choque")]
    public AudioClip sonidoChoque;
    
    [Header("Textos Flotantes")]
    public GameObject prefabTextoPuntos;

    [Header("Configuración de Vibración VR (Castigo)")]
    [Range(0f, 1f)] public float frecuenciaVibracion = 1f;
    [Range(0f, 1f)] public float amplitudVibracion = 1f; 
    public float duracionVibracion = 0.3f; 
    
    private AudioSource audioSource;
    private MeshRenderer meshRenderer;
    private Collider miCollider;
    
    private Color colorOriginal;
    private Vector3 posicionOriginal;
    private bool yaGolpeado = false;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        miCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        
        if (meshRenderer != null)
        {
            colorOriginal = meshRenderer.material.color;
        }
    }

    void OnEnable()
    {
        yaGolpeado = false;
        posicionOriginal = transform.localPosition;
        
        if (miCollider != null) miCollider.enabled = true;
        
        if (meshRenderer != null) 
        {
            meshRenderer.enabled = true; 
            meshRenderer.material.color = colorOriginal;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (yaGolpeado) return; 

        if (collision.gameObject.CompareTag("ball") || collision.gameObject.name.Contains("Balon"))
        {
            yaGolpeado = true;

            if (ControladorJuego.Instancia != null)
            {
                ControladorJuego.Instancia.RestarPunto();
            }

            OVRInput.Controller mandoActivo = OVRInput.Controller.Active;
            if (mandoActivo != OVRInput.Controller.None)
            {
                OVRInput.SetControllerVibration(frecuenciaVibracion, amplitudVibracion, mandoActivo);
                Invoke("ApagarVibracionMando", duracionVibracion); 
            }

            StartCoroutine(EfectoImpacto());
        }
    }

    private void ApagarVibracionMando()
    {
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.Active);
    }

    IEnumerator EfectoImpacto()
    {
        if (miCollider != null) miCollider.enabled = false;
        if (meshRenderer != null) meshRenderer.material.color = Color.red;
        if (sonidoChoque != null) audioSource.PlayOneShot(sonidoChoque);

        // --- LANZAMOS EL TEXTO FLOTANTE ROJO ---
        if (prefabTextoPuntos != null)
        {
            // Aparece un poco por encima del obstáculo
            GameObject txt = Instantiate(prefabTextoPuntos, transform.position + (Vector3.up * 0.3f), Quaternion.identity);
            TextoFlotante scriptTxt = txt.GetComponent<TextoFlotante>();
            if (scriptTxt != null)
            {
                scriptTxt.Configurar("-1", Color.red); 
            }
        }

        float tiempoVibracionCubo = 0.3f;
        float tiempoActual = 0f;

        while (tiempoActual < tiempoVibracionCubo)
        {
            tiempoActual += Time.deltaTime;
            transform.localPosition = posicionOriginal + (Random.insideUnitSphere * 0.1f);
            yield return null;
        }

        transform.localPosition = posicionOriginal;
        
        if (meshRenderer != null) meshRenderer.enabled = false;

        if (sonidoChoque != null)
        {
            float tiempoRestanteAudio = sonidoChoque.length - tiempoVibracionCubo;
            if (tiempoRestanteAudio > 0)
            {
                yield return new WaitForSeconds(tiempoRestanteAudio);
            }
        }

        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        ApagarVibracionMando();
        CancelInvoke("ApagarVibracionMando");
    }
}