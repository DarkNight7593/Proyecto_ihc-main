using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiadorEscenas : MonoBehaviour
{
    // Esta función recibe el nombre de la escena a la que quieres ir
    public void CambiarEscena(string nombreDeLaEscena)
    {
        Debug.Log("Viajando a la escena: " + nombreDeLaEscena);
        SceneManager.LoadScene(nombreDeLaEscena);
    }
}