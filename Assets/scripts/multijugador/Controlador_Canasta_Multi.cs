using System.Collections;
using UnityEngine;
using Unity.Netcode; 

namespace Multijugador
{
    public class Controlador_Canasta_Multi : NetworkBehaviour 
    {
        [Header("Identificación de la Cancha")]
        [Tooltip("¿De quién es este aro? 1 = Host, 2 = Jugador 2, 3 = Jugador 3")]
        public int idJugador = 1;

        [Header("Efecto de Brillo (Glow)")]
        public Light luzDestello;
        public AudioSource sonidoMalla;
        public float duracionBrillo = 0.4f;

        [Header("Textos Flotantes")]
        public GameObject prefabTextoPuntos;

        [Header("Ajustes para Aro Dinámico")]
        public float tiempoBloqueoCanasta = 1.8f; 

        private bool canastaMarcada = false;

        void Start()
        {
            if (luzDestello != null) luzDestello.enabled = false;
        }

        void OnEnable()
        {
            canastaMarcada = false;
            if (luzDestello != null) luzDestello.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return; 

            if ((other.CompareTag("ball") || other.name.Contains("Balon")) && !canastaMarcada)
            {
                StartCoroutine(ProcesarCanastaServidor());
            }
        }

        IEnumerator ProcesarCanastaServidor()
        {
            canastaMarcada = true;

            if (ControladorJuego_Multi.Instancia != null)
            {
                ControladorJuego_Multi.Instancia.RegistrarCanasta(idJugador);
            }

            ReproducirEfectosClientRpc();

            yield return new WaitForSeconds(tiempoBloqueoCanasta);
            canastaMarcada = false; 
        }

        [ClientRpc]
        private void ReproducirEfectosClientRpc()
        {
            StartCoroutine(RutinaEfectosVisuales());
        }

        IEnumerator RutinaEfectosVisuales()
        {
            if (luzDestello != null) luzDestello.enabled = true;
            if (sonidoMalla != null) sonidoMalla.Play();

            if (prefabTextoPuntos != null)
            {
                GameObject txt = Instantiate(prefabTextoPuntos, transform.position + (Vector3.up * 0.3f), Quaternion.identity);
                TextoFlotante scriptTxt = txt.GetComponent<TextoFlotante>();

                if (scriptTxt != null && ControladorJuego_Multi.Instancia != null)
                {
                    // 🔥 CORREGIDO AQUÍ: Ahora lee "ModoJuegoMulti.Normal"
                    if (ControladorJuego_Multi.Instancia.modoActual.Value == ModoJuegoMulti.Normal)
                    {
                        scriptTxt.Configurar("+3", new Color(1f, 0.7f, 0f)); 
                    }
                    else
                    {
                        scriptTxt.Configurar("+1", Color.green); 
                    }
                }
            }

            yield return new WaitForSeconds(duracionBrillo);
            if (luzDestello != null) luzDestello.enabled = false;
        }
    }
}