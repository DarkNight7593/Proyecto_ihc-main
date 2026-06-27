using UnityEngine;
using Unity.Netcode; 

namespace Multijugador
{
    public class MovimientoAro_Multi : NetworkBehaviour 
    {
        [Header("Referencias de las 3 Canastas")]
        public GameObject[] canastasConPalo = new GameObject[3];
        public GameObject[] canastasSinPalo = new GameObject[3];

        [Header("Ajustes de Movimiento")]
        public float velocidad = 1.5f;
        public float distanciaX = 2.0f; 
        public float distanciaY = 0.6f;
        public float velocidadSuavizado = 5.0f; 

        private Vector3[] posicionesIniciales = new Vector3[3];
        private float[] tiemposInternos = new float[3];
        private float[] desfasesAleatorios = new float[3];
        
        private bool movimientoActivoLocal = false;

        void Start()
        {
            for (int i = 0; i < canastasSinPalo.Length; i++)
            {
                if (canastasSinPalo[i] != null) 
                {
                    posicionesIniciales[i] = canastasSinPalo[i].transform.position;
                    desfasesAleatorios[i] = Random.Range(0f, 10f);
                    tiemposInternos[i] = desfasesAleatorios[i];
                }
            }
        }

        void Update()
        {
            if (ControladorJuego_Multi.Instancia == null) return;

            bool arosActivadosGlobal = ControladorJuego_Multi.Instancia.arosDinamicos.Value;
            
            if (arosActivadosGlobal != movimientoActivoLocal)
            {
                movimientoActivoLocal = arosActivadosGlobal;
                for (int i = 0; i < 3; i++)
                {
                    if (canastasConPalo[i] != null) canastasConPalo[i].SetActive(!movimientoActivoLocal);
                    if (canastasSinPalo[i] != null) canastasSinPalo[i].SetActive(movimientoActivoLocal);
                }
            }

            if (!IsServer || !movimientoActivoLocal) return;

            // 🔥 ACTUALIZADO AQUÍ PARA LEER "ModoJuegoMulti"
            bool enJuego = ControladorJuego_Multi.Instancia.modoActual.Value == ModoJuegoMulti.Normal || 
                          (ControladorJuego_Multi.Instancia.modoActual.Value == ModoJuegoMulti.Desafio && 
                           ControladorJuego_Multi.Instancia.faseActual.Value == FaseDesafioMulti.Jugando);

            for (int i = 0; i < 3; i++)
            {
                if (canastasSinPalo[i] == null) continue;

                Vector3 posicionDestino = posicionesIniciales[i];

                if (enJuego)
                {
                    tiemposInternos[i] += Time.deltaTime;
                    posicionDestino.x += Mathf.Sin(tiemposInternos[i] * velocidad) * distanciaX;
                    posicionDestino.y += Mathf.Sin(tiemposInternos[i] * velocidad * 2f) * distanciaY;
                }
                else
                {
                    tiemposInternos[i] = desfasesAleatorios[i];
                }

                canastasSinPalo[i].transform.position = Vector3.Lerp(canastasSinPalo[i].transform.position, posicionDestino, Time.deltaTime * velocidadSuavizado);
            }
        }
    }
}