using System.Collections;
using UnityEngine;
using TMPro;

public enum ModoJuego { Normal, Desafio }
public enum FaseDesafio { Inactivo, CuentaRegresiva, Jugando, Descanso, Terminado }

public class ControladorJuego : MonoBehaviour
{
    public static ControladorJuego Instancia;

    [Header("Estado del Juego")]
    public ModoJuego modoActual = ModoJuego.Normal;
    public FaseDesafio faseActual = FaseDesafio.Inactivo;

    [Header("UI y Marcador")]
    public TextMeshProUGUI textoMarcador;
    public TextMeshProUGUI textoTiempo; 
    
    [Header("UI de Anuncios Gigantes")]
    public TextMeshProUGUI textoAnuncios; 

    private int puntosTotales = 0;
    
    [Header("Ajustes de Rondas (Desafío)")]
    public int totalRondas = 3;
    public float tiempoPorRonda = 60f;
    public float tiempoDescanso = 10f;
    
    private float tiempoRestante;
    private Coroutine rutinaDesafioActual;

    [Header("🎵 Sistema de Audio (Asignar Canales)")]
    public AudioSource fuenteMusica;
    public AudioSource fuenteSFX;
    public AudioSource fuenteVoz; // 🔥 NUEVO: Ahora expuesto en el Inspector para que lo asignes

    [Header("🎵 Música")]
    public AudioClip musicaNormal;
    public AudioClip musicaDesafio;
    public float duracionTransicion = 0.8f;
    [Range(0f, 1f)] public float volumenMaster = 0.3f; 

    [Header("🔊 Efectos de Voz y Sonido (SFX)")]
    [Tooltip("Tu archivo largo '3 2 1 Go green screen'")]
    public AudioClip audioConteoCompleto;
    
    [Tooltip("Sonido/Aplausos que sonarán de fondo ANTES de que salga la ronda y durante el conteo")]
    public AudioClip audioInicioRonda;
    
    [Range(0f, 1f)] public float volumenAplausos = 0.5f; 
    
    [Tooltip("Sonido para el inicio del tiempo de descanso")]
    public AudioClip audioDescanso;
    [Tooltip("Sonido de victoria/fin de juego completo")]
    public AudioClip audioFinJuego;

    private Coroutine rutinaTransicionMusica;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 🔥 LÓGICA ACTUALIZADA: Reconoce los 3 AudioSource si no los has asignado a mano
        AudioSource[] fuentes = GetComponents<AudioSource>();
        if (fuentes.Length >= 3)
        {
            if (fuenteMusica == null) fuenteMusica = fuentes[0];
            if (fuenteSFX == null) fuenteSFX = fuentes[1];
            if (fuenteVoz == null) fuenteVoz = fuentes[2];
        }
        else if (fuentes.Length == 2)
        {
            if (fuenteMusica == null) fuenteMusica = fuentes[0];
            if (fuenteSFX == null) fuenteSFX = fuentes[1];
        }

        if (fuenteMusica != null)
        {
            fuenteMusica.volume = volumenMaster;
            fuenteMusica.loop = true; 
        }

        ActivarModoNormal();
    }

    public void RegistrarCanasta()
    {
        if (modoActual == ModoJuego.Normal) puntosTotales += 3;
        else if (modoActual == ModoJuego.Desafio && faseActual == FaseDesafio.Jugando) puntosTotales += 1;
        ActualizarMarcadorUI();
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

    public void ActivarModoNormal()
    {
        DetenerDesafioSiExiste();
        modoActual = ModoJuego.Normal;
        faseActual = FaseDesafio.Inactivo;
        puntosTotales = 0;
        
        if (textoTiempo != null) textoTiempo.text = "TIEMPO: --:--";
        if (textoAnuncios != null) textoAnuncios.text = "";
        
        ActualizarMarcadorUI();
        CambiarMusicaConTransicion(musicaNormal);
    }

    public void ActivarModoDesafio()
    {
        DetenerDesafioSiExiste();
        modoActual = ModoJuego.Desafio;
        puntosTotales = 0;
        ActualizarMarcadorUI();

        CambiarMusicaConTransicion(musicaDesafio);
        rutinaDesafioActual = StartCoroutine(SecuenciaModoDesafio());
    }

    private void DetenerDesafioSiExiste()
    {
        if (rutinaDesafioActual != null) StopCoroutine(rutinaDesafioActual);
        if (textoAnuncios != null) textoAnuncios.text = "";
    }

    // --- SECUENCIA ARCADÉ OPTIMIZADA ---
    private IEnumerator SecuenciaModoDesafio()
    {
        for (int rondaActual = 1; rondaActual <= totalRondas; rondaActual++)
        {
            faseActual = FaseDesafio.CuentaRegresiva;
            textoTiempo.text = "RONDA " + rondaActual;
            
            // 💥 1. DISPARAMOS LOS APLAUSOS (Al 50% de volumen en el canal SFX)
            if (fuenteSFX != null && audioInicioRonda != null)
            {
                fuenteSFX.PlayOneShot(audioInicioRonda, volumenAplausos);
            }

            yield return new WaitForSeconds(0.5f);

            // 💥 2. APARECE EL TEXTO DE LA RONDA
            textoAnuncios.text = $"<color=yellow><size=80%><b>RONDA {rondaActual}</b></size></color>";

            yield return new WaitForSeconds(2.8f);
            
            // 💥 3. ENTRA LA VOZ DEL "3, 2, 1" (En su propio canal independiente)
            if (fuenteVoz != null && audioConteoCompleto != null)
            {
                fuenteVoz.PlayOneShot(audioConteoCompleto);
            }
            
            // 💥 4. CUENTA REGRESIVA VISUAL
            for (int i = 3; i > 0; i--) 
            {
                textoAnuncios.text = $"<color=#FF4500><size=220%><b>{i}</b></size></color>";
                yield return new WaitForSeconds(1f);
            }
            
            // 💥 5. ¡A ENCESTAR!
            textoAnuncios.text = "<color=green><size=90%><b>¡A ENCESTAR!</b></size></color>";
            
            yield return new WaitForSeconds(1f);
            textoAnuncios.text = ""; 
            
            // --- FASE DE JUEGO ACTIVO ---
            faseActual = FaseDesafio.Jugando;
            tiempoRestante = tiempoPorRonda;
            
            while (tiempoRestante > 0)
            {
                tiempoRestante -= Time.deltaTime;
                ActualizarRelojUI();
                yield return null; 
            }

            // 💥 6. DESCANSO ENTRE RONDAS
            if (rondaActual < totalRondas)
            {
                faseActual = FaseDesafio.Descanso;
                textoTiempo.text = "DESCANSO";
                
                if (fuenteSFX != null && audioDescanso != null)
                {
                    fuenteSFX.PlayOneShot(audioDescanso);
                }

                float tiempoDescansoRestante = tiempoDescanso;
                while (tiempoDescansoRestante > 0)
                {
                    int seg = Mathf.CeilToInt(tiempoDescansoRestante);
                    textoAnuncios.text = $"<color=#00FFFF><size=80%><b>¡DESCANSO!</b></size>\n<size=140%><b>{seg}</b></size></color>";
                    
                    tiempoDescansoRestante -= Time.deltaTime;
                    yield return null;
                }
            }
        }

        // 💥 7. PANTALLA DE FIN DE JUEGO
        faseActual = FaseDesafio.Terminado;
        textoTiempo.text = "FIN";
        
        if (fuenteSFX != null && audioFinJuego != null)
        {
            fuenteSFX.PlayOneShot(audioFinJuego);
        }
        
        textoAnuncios.text = $"<color=red><size=80%><b>¡JUEGO TERMINADO!</b></size></color>\n" +
                             $"<color=white><size=50%>Puntaje Final:</size></color>\n" +
                             $"<color=#FFD700><size=140%><b>{puntosTotales} PTS</b></size></color>";
        
        yield return new WaitForSeconds(6f);
        if (faseActual == FaseDesafio.Terminado) textoAnuncios.text = ""; 
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

        fuenteMusica.Stop();
        fuenteMusica.clip = nuevaPista;
        fuenteMusica.Play();

        float tiempoTranscurrido = 0;
        while (tiempoTranscurrido < duracionTransicion)
        {
            tiempoTranscurrido += Time.deltaTime;
            fuenteMusica.volume = Mathf.Lerp(0f, volumenMaster, tiempoTranscurrido / duracionTransicion);
            yield return null;
        }

        fuenteMusica.volume = volumenMaster;
        rutinaTransicionMusica = null; 
    }

    void ActualizarMarcadorUI() => textoMarcador.text = "SCORE: " + puntosTotales.ToString("00");

    void ActualizarRelojUI()
    {
        int min = Mathf.FloorToInt(tiempoRestante / 60);
        int seg = Mathf.FloorToInt(tiempoRestante % 60);
        textoTiempo.text = string.Format("TIEMPO: {0:00}:{1:00}", min, seg);
    }
}