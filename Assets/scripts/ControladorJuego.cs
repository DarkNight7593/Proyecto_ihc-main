using System.Collections;
using UnityEngine;
using TMPro;

public enum ModoJuego { Normal, Desafio }
// Añadimos estados internos para saber en qué fase del desafío estamos
public enum FaseDesafio { Inactivo, CuentaRegresiva, Jugando, Descanso, Terminado }

public class ControladorJuego : MonoBehaviour
{
    public static ControladorJuego Instancia;

    [Header("Estado del Juego")]
    public ModoJuego modoActual = ModoJuego.Normal;
    public FaseDesafio faseActual = FaseDesafio.Inactivo;

    [Header("UI del Marcador")]
    public TextMeshProUGUI textoMarcador;
    public TextMeshProUGUI textoTiempo; 
    
    [Header("UI de Anuncios Gigantes")]
    [Tooltip("Arrastra aquí el texto semitransparente del centro de la pantalla")]
    public TextMeshProUGUI textoAnuncios; 

    private int puntosTotales = 0;
    
    [Header("Ajustes de Rondas (Desafío)")]
    public int totalRondas = 3;
    public float tiempoPorRonda = 60f; // 60 segundos por ronda
    public float tiempoDescanso = 10f; // 10 segundos de respiro
    
    private float tiempoRestante;
    private Coroutine rutinaDesafioActual; // Para poder detenerla si cambiamos de modo

    [Header("🎵 Sistema de Música de Fondo")]
    [Tooltip("El componente AudioSource asignado al GameManager")]
    public AudioSource fuenteMusica;
    [Tooltip("Canción para el Modo Normal")]
    public AudioClip musicaNormal;
    [Tooltip("Canción intensa para el Modo Desafío")]
    public AudioClip musicaDesafio;
    [Tooltip("Tiempo en segundos que tardará en desvanecerse la música al cambiar")]
    public float duracionTransicion = 0.8f;

    [Range(0f, 1f)]
    [Tooltip("Volumen general de la música (0 = Silencio, 1 = Máximo)")]
    public float volumenMaster = 0.5f; // Empezamos a mitad de volumen por defecto

    private Coroutine rutinaTransicionMusica;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Si no se asignó en el inspector, intentamos obtener el AudioSource del mismo objeto
        if (fuenteMusica == null) fuenteMusica = GetComponent<AudioSource>();

        if (fuenteMusica != null)
        {
            fuenteMusica.volume = volumenMaster;
            fuenteMusica.loop = true; // Nos aseguramos de que repita la música
        }

        ActivarModoNormal(); // Empezamos en Normal por defecto
    }

    // --- LÓGICA DE PUNTUACIÓN ---
    public void RegistrarCanasta()
    {
        if (modoActual == ModoJuego.Normal)
        {
            puntosTotales += 3;
            ActualizarMarcadorUI();
        }
        else if (modoActual == ModoJuego.Desafio && faseActual == FaseDesafio.Jugando)
        {
            puntosTotales += 1;
            ActualizarMarcadorUI();
        }
    }

    public void RestarPunto()
    {
        if (modoActual == ModoJuego.Desafio && faseActual == FaseDesafio.Jugando)
        {
            puntosTotales -= 1;
            puntosTotales = Mathf.Max(0, puntosTotales); 
            ActualizarMarcadorUI();
        }
    }

    // --- BOTÓN MODO NORMAL ---
    public void ActivarModoNormal()
    {
        DetenerDesafioSiExiste();
        modoActual = ModoJuego.Normal;
        faseActual = FaseDesafio.Inactivo;
        puntosTotales = 0;
        
        if (textoTiempo != null) textoTiempo.text = "TIEMPO: --:--";
        if (textoAnuncios != null) textoAnuncios.text = ""; 
        
        ActualizarMarcadorUI();

        // 🎵 Transición a la música del Modo Normal
        CambiarMusicaConTransicion(musicaNormal);
    }

    // --- BOTÓN MODO DESAFÍO ---
    public void ActivarModoDesafio()
    {
        DetenerDesafioSiExiste();
        modoActual = ModoJuego.Desafio;
        puntosTotales = 0;
        ActualizarMarcadorUI();

        // 🎵 Transición a la música de desafío
        CambiarMusicaConTransicion(musicaDesafio);

        rutinaDesafioActual = StartCoroutine(SecuenciaModoDesafio());
    }

    private void DetenerDesafioSiExiste()
    {
        if (rutinaDesafioActual != null)
        {
            StopCoroutine(rutinaDesafioActual);
        }
    }

    // --- GESTIÓN DE AUDIO Y VOLUMEN ---
    
    /// <summary>
    /// Función pública para cambiar el volumen desde un Slider de la UI u otro script.
    /// Acepta valores entre 0.0 y 1.0.
    /// </summary>
    public void AjustarVolumen(float nuevoVolumen)
    {
        // Aseguramos que el valor esté entre 0 y 1
        volumenMaster = Mathf.Clamp01(nuevoVolumen);

        // Si no hay ninguna transición de Fade ejecutándose, aplicamos el volumen directamente
        if (rutinaTransicionMusica == null && fuenteMusica != null)
        {
            fuenteMusica.volume = volumenMaster;
        }
    }

    private void CambiarMusicaConTransicion(AudioClip nuevaPista)
    {
        if (fuenteMusica == null || nuevaPista == null) return;

        if (fuenteMusica.clip == nuevaPista && fuenteMusica.isPlaying) return;

        if (rutinaTransicionMusica != null) StopCoroutine(rutinaTransicionMusica);

        rutinaTransicionMusica = StartCoroutine(RutinaFadeMusica(nuevaPista));
    }

    private IEnumerator RutinaFadeMusica(AudioClip nuevaPista)
    {
        // 1. FADE OUT: Disminuimos el volumen gradualmente tomando como tope el volumenMaster actual
        if (fuenteMusica.isPlaying)
        {
            float volumenInicial = fuenteMusica.volume;
            float tiempoAsignado = 0;

            while (tiempoAsignado < duracionTransicion)
            {
                tiempoAsignado += Time.deltaTime;
                fuenteMusica.volume = Mathf.Lerp(volumenInicial, 0f, tiempoAsignado / duracionTransicion);
                yield return null;
            }
        }

        // 2. CAMBIO DE CLIP
        fuenteMusica.Stop();
        fuenteMusica.clip = nuevaPista;
        fuenteMusica.Play();

        // 3. FADE IN: Subimos el volumen respetando el límite que tenga asignado volumenMaster
        float tiempoTranscurrido = 0;
        while (tiempoTranscurrido < duracionTransicion)
        {
            tiempoTranscurrido += Time.deltaTime;
            fuenteMusica.volume = Mathf.Lerp(0f, volumenMaster, tiempoTranscurrido / duracionTransicion);
            yield return null;
        }

        // Aseguramos precisión al concluir el desvanecimiento
        fuenteMusica.volume = volumenMaster;
        rutinaTransicionMusica = null; // Liberamos la referencia
    }

    // --- LA LÍNEA DE TIEMPO DEL MODO DESAFÍO ---
    private IEnumerator SecuenciaModoDesafio()
    {
        for (int rondaActual = 1; rondaActual <= totalRondas; rondaActual++)
        {
            faseActual = FaseDesafio.CuentaRegresiva;
            textoTiempo.text = "RONDA " + rondaActual;
            
            for (int i = 3; i > 0; i--) 
            {
                textoAnuncios.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }
            textoAnuncios.text = "¡COMIENZA!";
            yield return new WaitForSeconds(1f);
            textoAnuncios.text = ""; 
            
            faseActual = FaseDesafio.Jugando;
            tiempoRestante = tiempoPorRonda;
            
            while (tiempoRestante > 0)
            {
                tiempoRestante -= Time.deltaTime;
                ActualizarRelojUI();
                yield return null; 
            }

            if (rondaActual < totalRondas)
            {
                faseActual = FaseDesafio.Descanso;
                textoAnuncios.text = "¡TIEMPO!\nToma aire por " + tiempoDescanso + "s";
                textoTiempo.text = "DESCANSO";
                yield return new WaitForSeconds(tiempoDescanso);
            }
        }

        faseActual = FaseDesafio.Terminado;
        textoAnuncios.text = "¡FIN DEL JUEGO!\nPuntaje: " + puntosTotales;
        textoTiempo.text = "FIN";
        
        yield return new WaitForSeconds(5f);
        if (faseActual == FaseDesafio.Terminado) textoAnuncios.text = ""; 
    }

    // --- ACTUALIZACIÓN VISUAL ---
    void ActualizarMarcadorUI()
    {
        if (textoMarcador != null) textoMarcador.text = "SCORE: " + puntosTotales.ToString("00");
    }

    void ActualizarRelojUI()
    {
        if (textoTiempo != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            textoTiempo.text = string.Format("TIEMPO: {0:00}:{1:00}", minutos, segundos);
        }
    }
}