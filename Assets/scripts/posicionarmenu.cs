using UnityEngine;

public class posicionarmenu : MonoBehaviour
{
    [Header("Configuración de la Cámara")]
    public Transform camaraVR; // Arrastra aquí el "CenterEyeAnchor" de tu OVRCameraRig
    
    [Header("Ajustes de Distancia")]
    public float distancia = 2.0f; // Qué tan lejos del jugador flotará (en metros)
    public float alturaOffset = -0.2f; // Ajuste vertical para que no tape los ojos directamente

    void Start()
    {
        PosicionarFrenteAlJugador();
    }

    [ContextMenu("Reposicionar Menú")] // Permite testearlo desde el editor haciendo clic derecho en el componente
    public void PosicionarFrenteAlJugador()
    {
        if (camaraVR == null)
        {
            // Intenta buscar el CenterEyeAnchor automáticamente si no se asignó
            camaraVR = Camera.main.transform; 
        }

        if (camaraVR != null)
        {
            // 1. Obtener la posición de la cámara y proyectar hacia adelante
            Vector3 posicionFrente = camaraVR.position + (camaraVR.forward * distancia);
            
            // 2. Aplicar el ajuste de altura
            posicionFrente.y += alturaOffset;
            
            transform.position = posicionFrente;

            // 3. Hacer que el menú mire al jugador (rotación)
            // Usamos una rotación hacia la cámara pero manteniendo el menú vertical
            Vector3 direccionHaciaCamara = camaraVR.position - transform.position;
            direccionHaciaCamara.y = 0; // Evita que el menú se incline raro hacia arriba/abajo
            
            transform.rotation = Quaternion.LookRotation(-direccionHaciaCamara);
        }
    }
}