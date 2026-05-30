using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject objetoAPreparar; 
    public Transform puntoDeAparicion; 

    public GameObject _objetoInstanciado;

    // Este método se activa cuando la mano virtual entra en el área del botón
    private void OnTriggerEnter(Collider other)
    {
        // 1. Veremos en consola el nombre exacto de lo que tocó el botón
        Debug.Log("El objeto que tocó el botón se llama: " + other.gameObject.name);

        // 2. Por ahora, quitemos el filtro para confirmar que funciona
        // Una vez que confirmes, podemos volver a filtrar por nombre o Tag
        GestionarObjeto();
    }
    public void GestionarObjeto()
    {
        if (objetoAPreparar == null || puntoDeAparicion == null) return;

        if (_objetoInstanciado == null)
        {
            _objetoInstanciado = Instantiate(objetoAPreparar, puntoDeAparicion.position, puntoDeAparicion.rotation);
        }
        else 
        {
            _objetoInstanciado.transform.position = puntoDeAparicion.position;
            _objetoInstanciado.transform.rotation = puntoDeAparicion.rotation;

            Rigidbody rb = _objetoInstanciado.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}