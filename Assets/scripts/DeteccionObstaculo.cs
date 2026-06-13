using UnityEngine;

public class DeteccionObstaculo : MonoBehaviour
{
    // Cambiamos a OnCollisionEnter para colisiones físicas (rebotes)
    private void OnCollisionEnter(Collision collision)
    {
        // En OnCollisionEnter usamos collision.gameObject para revisar las etiquetas o nombres
        if (collision.gameObject.CompareTag("ball") || collision.gameObject.name.Contains("Balon"))
        {
            // Le avisamos al cerebro central que reste un punto
            if (ControladorJuego.Instancia != null)
            {
                ControladorJuego.Instancia.RestarPunto();
            }

            Debug.Log("💥 ¡Choque físico con obstáculo! -1 Punto y Rebote");

            // Nos apagamos instantáneamente para que el balón rebote una sola vez 
            // y no se quede pegado restando puntos infinitos en cada frame
            gameObject.SetActive(false);
        }
    }
}