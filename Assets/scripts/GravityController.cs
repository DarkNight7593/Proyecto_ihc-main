using UnityEngine;

public class GravityController : MonoBehaviour
{
    // Gravedad normal de la Tierra (aproximadamente -9.81 m/s^2 en el eje Y)
    private readonly Vector3 normalGravity = new Vector3(0, -9.81f, 0);

    // Gravedad Lunar (aproximadamente 1/6 de la Tierra, -1.62 m/s^2 en el eje Y)
    private readonly Vector3 lunarGravity = new Vector3(0, -1.62f, 0);

    void Start()
    {
        // Buena práctica: Asegurar que el juego inicie siempre con gravedad normal
        SetNormalGravity();
    }

    // Método para el botón de Pesa (Normal)
    public void SetNormalGravity()
    {
        Physics.gravity = normalGravity;
        Debug.Log("Gravedad del sistema: NORMAL");
    }

    // Método para el botón de Pluma (Lunar)
    public void SetLunarGravity()
    {
        Physics.gravity = lunarGravity;
        Debug.Log("Gravedad del sistema: LUNAR");
    }
}