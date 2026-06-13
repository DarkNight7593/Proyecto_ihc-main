using UnityEngine;

public class MovimientoAroNormal : MonoBehaviour
{
    public bool movimientoActivado = false;
    
    [Header("Ajustes de Movimiento")]
    public float velocidad = 1.0f;
    public float distancia = 1.5f;

    private Vector3 posicionInicial;
    
    // Nuestro propio cronómetro que nosotros controlamos
    private float tiempoInterno = 0f; 

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        if (movimientoActivado)
        {
            // Sumamos el tiempo solo si el movimiento está activado
            tiempoInterno += Time.deltaTime;

            // Usamos nuestro 'tiempoInterno' en lugar del 'Time.time' global
            float movimientoX = Mathf.Sin(tiempoInterno * velocidad) * distancia;
            transform.position = new Vector3(posicionInicial.x + movimientoX, posicionInicial.y, posicionInicial.z);
        }
    }

    public void AlternarMovimiento(bool activado)
    {
        movimientoActivado = activado;
        
        // Si apagamos el movimiento...
        if (!activado)
        {
            transform.position = posicionInicial; // Regresamos al centro
            tiempoInterno = 0f; // ¡Y reiniciamos el reloj para que empiece limpio la próxima vez!
        }
    }
}