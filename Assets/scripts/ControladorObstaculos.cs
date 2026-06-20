using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorObstaculos : MonoBehaviour
{
    [Header("Conexión con el Aro")]
    [Tooltip("Arrastra aquí el objeto que tiene el script 'MovimientoAro'")]
    public MovimientoAro scriptMovimientoAro;

    [Header("Listas de Obstáculos")]
    [Tooltip("Pon aquí los obstáculos que están CERCA del aro (Se usan siempre)")]
    public List<GameObject> obstaculosCercaAro;
    
    [Tooltip("Pon aquí los obstáculos ALEJADOS (Solo se usan si el aro dinámico está activado)")]
    public List<GameObject> obstaculosExtra;

    [Header("Ajustes de Aparición")]
    public float tiempoAparicionMin = 4f; 
    public float tiempoAparicionMax = 5f; 
    public float tiempoActivoMin = 2f;    
    public float tiempoActivoMax = 3.5f;

    void Start()
    {
        ApagarTodos();
        StartCoroutine(RutinaAparicionObstaculos());
    }

    private void ApagarTodos()
    {
        foreach (GameObject obs in obstaculosCercaAro) if (obs != null) obs.SetActive(false);
        foreach (GameObject obs in obstaculosExtra) if (obs != null) obs.SetActive(false);
    }

    IEnumerator RutinaAparicionObstaculos()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(tiempoAparicionMin, tiempoAparicionMax));

            if (ControladorJuego.Instancia != null && 
                ControladorJuego.Instancia.modoActual == ModoJuego.Desafio && 
                ControladorJuego.Instancia.faseActual == FaseDesafio.Jugando)
            {
                int cantidadAparecer = Random.Range(1, 3); 
                
                // Pedimos los obstáculos dependiendo de si el aro se mueve o no
                List<GameObject> elegidos = ObtenerObstaculosAleatorios(cantidadAparecer);

                foreach (GameObject obs in elegidos)
                {
                    if (obs != null) obs.SetActive(true);
                }

                yield return new WaitForSeconds(Random.Range(tiempoActivoMin, tiempoActivoMax));

                foreach (GameObject obs in elegidos)
                {
                    // 🔥 CAMBIO AQUÍ: Verificamos si el obstáculo NO ha sido golpeado.
                    // Si ya fue golpeado, NO lo apagamos y dejamos que termine su audio/animación.
                    if (obs != null && obs.activeInHierarchy)
                    {
                        DeteccionObstaculo deteccion = obs.GetComponent<DeteccionObstaculo>();
                        
                        if (deteccion == null || !deteccion.yaGolpeado)
                        {
                            obs.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    private List<GameObject> ObtenerObstaculosAleatorios(int cantidad)
    {
        // 1. Siempre metemos los que están cerca
        List<GameObject> disponibles = new List<GameObject>(obstaculosCercaAro);
        
        // 2. MAGIA AQUÍ: Le preguntamos DIRECTAMENTE al script del aro si está activado
        if (scriptMovimientoAro != null && scriptMovimientoAro.movimientoActivado)
        {
            disponibles.AddRange(obstaculosExtra);
        }

        // Limpiamos los que por casualidad ya estén encendidos para no repetir
        disponibles.RemoveAll(obs => obs == null || obs.activeSelf);

        List<GameObject> seleccionados = new List<GameObject>();

        for (int i = 0; i < cantidad; i++)
        {
            if (disponibles.Count == 0) break;

            int indiceRandom = Random.Range(0, disponibles.Count);
            seleccionados.Add(disponibles[indiceRandom]);
            disponibles.RemoveAt(indiceRandom); 
        }

        return seleccionados;
    }
}