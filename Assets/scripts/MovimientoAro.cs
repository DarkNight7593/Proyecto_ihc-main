using UnityEngine;

public class MovimientoAro : MonoBehaviour
{
    public bool movimientoActivado = false;

    [Header("Referencias de Canastas")]
    [Tooltip("Arrastra aquí el objeto 'canasta' (la fija con palo)")]
    public GameObject canastaConPalo;
    
    [Tooltip("Arrastra aquí el objeto 'canasta (1)' (la móvil sin palo)")]
    public GameObject canastaSinPalo;

    [Header("Ajustes Modo Normal")]
    public float velocidadNormal = 1.0f;
    public float distanciaNormalX = 1.5f;

    [Header("Ajustes Modo Desafío (3D)")]
    public float velocidadDesafio = 1.5f;
    public float distanciaDesafioX = 2.0f; 
    public float distanciaDesafioY = 0.6f;
    public float distanciaDesafioZ = 1.0f; 

    [Header("Ajustes de Erraticidad (Ruido)")]
    public float magnitudRuido = 0.5f; 
    public float velocidadRuido = 2.0f; 

    [Header("Suavizado de Transición")]
    public float velocidadSuavizado = 5.0f; 

    private Vector3 posicionInicialSinPalo;
    private float tiempoInterno = 0f; 

    void Start()
    {
        // Guardamos la posición original donde arranca la canasta flotante
        if (canastaSinPalo != null)
        {
            posicionInicialSinPalo = canastaSinPalo.transform.position;
        }

        // Forzamos que al iniciar la partida todo esté en su sitio correcto
        ActualizarVisibilidadCanastas();
    }

    void Update()
    {
        // 1. REVISAR SI EL JUEGO PERMITE EL MOVIMIENTO EN ESTE FRAME
        bool debeMoverseAhora = false;

        if (movimientoActivado && ControladorJuego.Instancia != null)
            {
            if (ControladorJuego.Instancia.modoActual == ModoJuego.Normal)
            {
                debeMoverseAhora = true;
            }
            else if (ControladorJuego.Instancia.modoActual == ModoJuego.Desafio && 
                     ControladorJuego.Instancia.faseActual == FaseDesafio.Jugando)
            {
                debeMoverseAhora = true;
            }
        }

        if (canastaSinPalo == null) return;

        // 2. CALCULAR DESTINO (Solo si la canasta sin palo está activa)
        Vector3 posicionDestino = posicionInicialSinPalo;

        if (debeMoverseAhora)
        {
            tiempoInterno += Time.deltaTime;

            if (ControladorJuego.Instancia.modoActual == ModoJuego.Normal)
            {
                posicionDestino.x += Mathf.Sin(tiempoInterno * velocidadNormal) * distanciaNormalX;
            }
            else if (ControladorJuego.Instancia.modoActual == ModoJuego.Desafio)
            {
                float baseX = Mathf.Sin(tiempoInterno * velocidadDesafio) * distanciaDesafioX;
                float baseY = Mathf.Sin(tiempoInterno * velocidadDesafio * 2f) * distanciaDesafioY;
                float baseZ = Mathf.Cos(tiempoInterno * velocidadDesafio * 0.7f) * distanciaDesafioZ;

                float ruidoX = (Mathf.PerlinNoise(tiempoInterno * velocidadRuido, 0f) - 0.5f) * magnitudRuido;
                float ruidoY = (Mathf.PerlinNoise(0f, tiempoInterno * velocidadRuido) - 0.5f) * magnitudRuido;
                float ruidoZ = (Mathf.PerlinNoise(tiempoInterno * velocidadRuido + 10f, 10f) - 0.5f) * magnitudRuido;

                posicionDestino.x += baseX + ruidoX;
                posicionDestino.y += baseY + ruidoY;
                posicionDestino.z += baseZ + ruidoZ;
            }
        }
        else
        {
            tiempoInterno = 0f;
        }

        // 3. APLICAR MOVIMIENTO SUAVE (Solo afecta a la canasta flotante si está encendida)
        if (canastaSinPalo.activeSelf)
        {
            canastaSinPalo.transform.position = Vector3.Lerp(canastaSinPalo.transform.position, posicionDestino, Time.deltaTime * velocidadSuavizado);
        }
    }

    public void AlternarMovimiento(bool activado)
    {
        movimientoActivado = activado;
        
        if (!activado)
        {
            tiempoInterno = 0f;
            // Al apagar, reseteamos la posición de la canasta móvil al centro al instante
            if (canastaSinPalo != null) canastaSinPalo.transform.position = posicionInicialSinPalo;
        }

        ActualizarVisibilidadCanastas();
    }

    private void ActualizarVisibilidadCanastas()
    {
        if (canastaConPalo != null && canastaSinPalo != null)
        {
            // Si el movimiento dinámico está activado:
            // Prendemos la flotante [canasta (1)] y apagamos la fija [canasta]
            canastaSinPalo.SetActive(movimientoActivado);
            canastaConPalo.SetActive(!movimientoActivado);
        }
    }
}