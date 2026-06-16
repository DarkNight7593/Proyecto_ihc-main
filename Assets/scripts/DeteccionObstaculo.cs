using System.Collections;
using UnityEngine;

// Esto obliga a Unity a añadir un AudioSource automáticamente a este objeto si no lo tiene
[RequireComponent(typeof(AudioSource))]
public class DeteccionObstaculo : MonoBehaviour
{
    [Header("Efectos de Choque")]
    [Tooltip("Arrastra aquí el sonido que hará al chocar")]
    public AudioClip sonidoChoque;

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
        
        // Agarramos el AudioSource
        audioSource = GetComponent<AudioSource>();
        
        // Guardamos el color original
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
        
        // CRUCIAL: Volvemos a hacer visible el cubo y le regresamos su color
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

            Debug.Log("💥 ¡Choque! Obstáculo rojo y Vibración VR Activada.");

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
        // 1. Apagamos física para que no rebote más
        if (miCollider != null) miCollider.enabled = false;

        // 2. Lo ponemos rojo
        if (meshRenderer != null) meshRenderer.material.color = Color.red;

        // 3. Empezamos el sonido
        if (sonidoChoque != null) audioSource.PlayOneShot(sonidoChoque);

        // 4. Vibramos el cubo visualmente
        float tiempoVibracionCubo = 0.3f;
        float tiempoActual = 0f;

        while (tiempoActual < tiempoVibracionCubo)
        {
            tiempoActual += Time.deltaTime;
            transform.localPosition = posicionOriginal + (Random.insideUnitSphere * 0.1f);
            yield return null;
        }

        // Dejamos el cubo en su lugar original tras temblar
        transform.localPosition = posicionOriginal;

        // 5. EN LUGAR DE APAGARLO, LO HACEMOS INVISIBLE
        if (meshRenderer != null) meshRenderer.enabled = false;

        // 6. Esperamos a que termine el audio antes de destruir/apagar el objeto
        if (sonidoChoque != null)
        {
            // Calculamos cuánto tiempo le falta al audio tras la vibración
            float tiempoRestanteAudio = sonidoChoque.length - tiempoVibracionCubo;
            if (tiempoRestanteAudio > 0)
            {
                yield return new WaitForSeconds(tiempoRestanteAudio);
            }
        }

        // 7. Ahora sí, apagamos el objeto por completo
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        ApagarVibracionMando();
        CancelInvoke("ApagarVibracionMando");
    }
}