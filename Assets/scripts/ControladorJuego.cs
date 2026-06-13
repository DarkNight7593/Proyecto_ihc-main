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

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    void Start()
    {
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
            // En desafío, SOLO suma puntos si estamos en la fase "Jugando" (no en descanso ni cuenta regresiva)
            puntosTotales += 1;
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
        if (textoAnuncios != null) textoAnuncios.text = ""; // Limpiamos la pantalla
        
        ActualizarMarcadorUI();
    }

    // --- BOTÓN MODO DESAFÍO ---
    public void ActivarModoDesafio()
    {
        DetenerDesafioSiExiste();
        modoActual = ModoJuego.Desafio;
        puntosTotales = 0;
        ActualizarMarcadorUI();

        // Iniciamos la gran secuencia del desafío
        rutinaDesafioActual = StartCoroutine(SecuenciaModoDesafio());
    }

    private void DetenerDesafioSiExiste()
    {
        if (rutinaDesafioActual != null)
        {
            StopCoroutine(rutinaDesafioActual);
        }
    }

    // --- LA LÍNEA DE TIEMPO DEL MODO DESAFÍO ---
    private IEnumerator SecuenciaModoDesafio()
    {
        for (int rondaActual = 1; rondaActual <= totalRondas; rondaActual++)
        {
            // 1. FASE DE CUENTA REGRESIVA
            faseActual = FaseDesafio.CuentaRegresiva;
            textoTiempo.text = "RONDA " + rondaActual;
            
            for (int i = 3; i > 0; i--) // Cuenta de 3, 2, 1...
            {
                textoAnuncios.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }
            textoAnuncios.text = "¡COMIENZA!";
            yield return new WaitForSeconds(1f);
            textoAnuncios.text = ""; // Borramos el texto
            
            // 2. FASE DE JUEGO (Cronómetro)
            faseActual = FaseDesafio.Jugando;
            tiempoRestante = tiempoPorRonda;
            
            while (tiempoRestante > 0)
            {
                tiempoRestante -= Time.deltaTime;
                ActualizarRelojUI();
                yield return null; // Esperamos al siguiente frame
            }

            // 3. FASE DE DESCANSO (Solo si no es la última ronda)
            if (rondaActual < totalRondas)
            {
                faseActual = FaseDesafio.Descanso;
                textoAnuncios.text = "¡TIEMPO!\nToma aire por " + tiempoDescanso + "s";
                textoTiempo.text = "DESCANSO";
                yield return new WaitForSeconds(tiempoDescanso);
            }
        }

        // 4. FIN DEL JUEGO
        faseActual = FaseDesafio.Terminado;
        textoAnuncios.text = "¡FIN DEL JUEGO!\nPuntaje: " + puntosTotales;
        textoTiempo.text = "FIN";
        
        // Dejamos el mensaje final unos segundos y luego lo limpiamos
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