using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorObstaculos : MonoBehaviour
{
[Header("Lista de Obstáculos")]
    [Tooltip("Arrastra aquí todos los cubos semitransparentes de tu escena")]
    public GameObject[] listaObstaculos;

    [Header("Ajustes de Aparición")]
    public float tiempoAparicionMin = 4f; // Mínimo de segundos para aparecer
    public float tiempoAparicionMax = 5f; // Máximo de segundos
    public float tiempoActivoMin = 2f;    // Cuánto tiempo se quedan bloqueando
    public float tiempoActivoMax = 3.5f;

    void Start()
    {
        // Al empezar el juego, nos aseguramos de que todos estén ocultos
        ApagarTodos();
        
        // Iniciamos el ciclo principal
        StartCoroutine(RutinaAparicionObstaculos());
    }

    private void ApagarTodos()
    {
        foreach (GameObject obs in listaObstaculos)
        {
            if (obs != null) obs.SetActive(false);
        }
    }

    IEnumerator RutinaAparicionObstaculos()
    {
        while (true)
        {
            // 1. Esperamos 4 a 5 segundos
            yield return new WaitForSeconds(Random.Range(tiempoAparicionMin, tiempoAparicionMax));

            // 2. Comprobamos si estamos jugando en modo Desafío
            if (ControladorJuego.Instancia != null && 
                ControladorJuego.Instancia.modoActual == ModoJuego.Desafio && 
                ControladorJuego.Instancia.faseActual == FaseDesafio.Jugando)
            {
                // 3. Elegimos cuántos van a aparecer (1 o 2)
                int cantidadAparecer = Random.Range(1, 3); 
                
                // 4. Seleccionamos los obstáculos al azar sin repetir
                List<GameObject> elegidos = ObtenerObstaculosAleatorios(cantidadAparecer);

                // 5. Los encendemos
                foreach (GameObject obs in elegidos)
                {
                    if (obs != null) obs.SetActive(true);
                }

                // 6. Esperamos el tiempo que van a estar activos
                yield return new WaitForSeconds(Random.Range(tiempoActivoMin, tiempoActivoMax));

                // 7. Los volvemos a apagar
                foreach (GameObject obs in elegidos)
                {
                    if (obs != null) obs.SetActive(false);
                }
            }
        }
    }

    // Función auxiliar para elegir cubos al azar sin que se repita el mismo dos veces
    private List<GameObject> ObtenerObstaculosAleatorios(int cantidad)
    {
        List<GameObject> disponibles = new List<GameObject>(listaObstaculos);
        List<GameObject> seleccionados = new List<GameObject>();

        for (int i = 0; i < cantidad; i++)
        {
            if (disponibles.Count == 0) break;

            int indiceRandom = Random.Range(0, disponibles.Count);
            seleccionados.Add(disponibles[indiceRandom]);
            disponibles.RemoveAt(indiceRandom); // Lo quitamos para no volver a elegirlo
        }

        return seleccionados;
    }
}
