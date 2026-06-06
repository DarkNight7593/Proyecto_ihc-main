using UnityEngine;

public class ExitController : MonoBehaviour
{
    public void SalirDelJuego()
    {
        Debug.Log("Intentando salir del juego...");

        // Esto cierra la aplicación cuando ya está instalada en el visor (el APK)
        Application.Quit();

        // Esto hace que el modo "Play" de Unity se detenga para que sepas que funciona en tu PC
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}