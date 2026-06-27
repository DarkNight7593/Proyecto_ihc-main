using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;

namespace Multijugador
{
    // 🔥 NOMBRES CAMBIADOS PARA EVITAR CONFLICTOS CON EL SINGLE PLAYER
    public enum ModoJuegoMulti { Normal, Desafio }
    public enum FaseDesafioMulti { Inactivo, CuentaRegresiva, Jugando, Descanso, Terminado }

    public class ControladorJuego_Multi : NetworkBehaviour 
    {
        public static ControladorJuego_Multi Instancia;

        [Header("⚙️ CONFIGURACIÓN DEL HOST (Global)")]
        public NetworkVariable<ModoJuegoMulti> modoActual = new NetworkVariable<ModoJuegoMulti>(ModoJuegoMulti.Normal);
        public NetworkVariable<FaseDesafioMulti> faseActual = new NetworkVariable<FaseDesafioMulti>(FaseDesafioMulti.Inactivo);
        
        public NetworkVariable<bool> arosDinamicos = new NetworkVariable<bool>(false);
        public NetworkVariable<float> gravedadGlobal = new NetworkVariable<float>(-9.81f);

        [Header("📺 PANTALLA GIGANTE (UI)")]
        public TextMeshProUGUI textoTiempoYAnuncios; 
        public TextMeshProUGUI textoMarcadorP1; 
        public TextMeshProUGUI textoMarcadorP2; 
        public TextMeshProUGUI textoMarcadorP3; 

        private NetworkVariable<int> puntosP1 = new NetworkVariable<int>(0);
        private NetworkVariable<int> puntosP2 = new NetworkVariable<int>(0);
        private NetworkVariable<int> puntosP3 = new NetworkVariable<int>(0);
        
        [Header("Ajustes de Rondas (Desafío)")]
        public int totalRondas = 3;
        public float tiempoPorRonda = 60f;
        public float tiempoDescanso = 10f;
        
        private float tiempoRestante;
        private Coroutine rutinaDesafioActual;

        [Header("🎵 Sistema de Audio")]
        public AudioSource fuenteMusica;
        public AudioSource fuenteSFX;
        public AudioSource fuenteVoz; 
        public AudioClip musicaNormal;
        public AudioClip musicaDesafio;
        [Range(0f, 1f)] public float volumenMaster = 0.3f; 
        public AudioClip audioConteoCompleto;
        public AudioClip audioInicioRonda;
        public AudioClip audioDescanso;
        public AudioClip audioFinJuego;
        private Coroutine rutinaTransicionMusica;

        void Awake()
        {
            if (Instancia == null) Instancia = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            if (fuenteMusica != null)
            {
                fuenteMusica.volume = volumenMaster;
                fuenteMusica.loop = true; 
            }
        }

        public override void OnNetworkSpawn()
        {
            puntosP1.OnValueChanged += (ant, nvo) => ActualizarMarcadoresUI();
            puntosP2.OnValueChanged += (ant, nvo) => ActualizarMarcadoresUI();
            puntosP3.OnValueChanged += (ant, nvo) => ActualizarMarcadoresUI();
            
            modoActual.OnValueChanged += (ant, nvo) => ManejarCambioModo(nvo);
            gravedadGlobal.OnValueChanged += (ant, nvo) => Physics.gravity = new Vector3(0, nvo, 0);

            ActualizarMarcadoresUI();
            ManejarCambioModo(modoActual.Value);
            Physics.gravity = new Vector3(0, gravedadGlobal.Value, 0);
        }
        
        public void ToggleArosDinamicos(bool activado) { if (IsServer) arosDinamicos.Value = activado; }
        public void CambiarGravedadLunar() { if (IsServer) gravedadGlobal.Value = -4.0f; }
        public void CambiarGravedadNormal() { if (IsServer) gravedadGlobal.Value = -9.81f; }

        public void IniciarPartidaDesafio()
        {
            if (!IsServer) return;
            DetenerDesafioSiExiste();
            modoActual.Value = ModoJuegoMulti.Desafio;
            ReiniciarPuntos();
            rutinaDesafioActual = StartCoroutine(SecuenciaModoDesafioServidor());
        }

        public void IniciarModoLibre()
        {
            if (!IsServer) return;
            DetenerDesafioSiExiste();
            modoActual.Value = ModoJuegoMulti.Normal;
            faseActual.Value = FaseDesafioMulti.Inactivo;
            ReiniciarPuntos();
            ActualizarPantallaCentralClientRpc("MODO LIBRE");
        }

        private void ReiniciarPuntos()
        {
            puntosP1.Value = 0;
            puntosP2.Value = 0;
            puntosP3.Value = 0;
        }

        public void RegistrarCanasta(int idJugador)
        {
            if (!IsServer) return; 
            int suma = (modoActual.Value == ModoJuegoMulti.Normal) ? 3 : 1;
            if (modoActual.Value == ModoJuegoMulti.Normal || (modoActual.Value == ModoJuegoMulti.Desafio && faseActual.Value == FaseDesafioMulti.Jugando))
            {
                if (idJugador == 1) puntosP1.Value += suma;
                else if (idJugador == 2) puntosP2.Value += suma;
                else if (idJugador == 3) puntosP3.Value += suma;
            }
        }

        private void ManejarCambioModo(ModoJuegoMulti nuevoModo)
        {
            if (nuevoModo == ModoJuegoMulti.Normal) CambiarMusicaConTransicion(musicaNormal);
            else CambiarMusicaConTransicion(musicaDesafio);
        }

        private void DetenerDesafioSiExiste()
        {
            if (rutinaDesafioActual != null) StopCoroutine(rutinaDesafioActual);
        }

        private IEnumerator SecuenciaModoDesafioServidor()
        {
            for (int rondaActual = 1; rondaActual <= totalRondas; rondaActual++)
            {
                faseActual.Value = FaseDesafioMulti.CuentaRegresiva;
                
                DispararAudioClientRpc(1); 
                ActualizarPantallaCentralClientRpc($"<color=yellow><size=80%><b>RONDA {rondaActual}</b></size></color>");
                yield return new WaitForSeconds(3.3f);
                
                DispararAudioClientRpc(2); 
                
                for (int i = 3; i > 0; i--) 
                {
                    ActualizarPantallaCentralClientRpc($"<color=#FF4500><size=220%><b>{i}</b></size></color>");
                    yield return new WaitForSeconds(1f);
                }
                
                ActualizarPantallaCentralClientRpc("<color=green><size=90%><b>¡A ENCESTAR!</b></size></color>");
                yield return new WaitForSeconds(1f);
                
                faseActual.Value = FaseDesafioMulti.Jugando;
                tiempoRestante = tiempoPorRonda;
                
                while (tiempoRestante > 0)
                {
                    tiempoRestante -= Time.deltaTime;
                    ActualizarRelojUIClientRpc(tiempoRestante);
                    yield return null; 
                }

                if (rondaActual < totalRondas)
                {
                    faseActual.Value = FaseDesafioMulti.Descanso;
                    DispararAudioClientRpc(3); 

                    float tiempoDescansoRestante = tiempoDescanso;
                    while (tiempoDescansoRestante > 0)
                    {
                        int seg = Mathf.CeilToInt(tiempoDescansoRestante);
                        ActualizarPantallaCentralClientRpc($"<color=#00FFFF><size=80%><b>¡DESCANSO!</b></size>\n<size=140%><b>{seg}</b></size></color>");
                        tiempoDescansoRestante -= Time.deltaTime;
                        yield return null;
                    }
                }
            }

            faseActual.Value = FaseDesafioMulti.Terminado;
            DispararAudioClientRpc(4); 
            
            string podioFinal = $"<color=red><b>¡JUEGO TERMINADO!</b></color>\n" +
                                $"<size=60%>MIRA TUS PUNTOS EN LOS MARCADORES</size>";
            ActualizarPantallaCentralClientRpc(podioFinal);
        }

        [ClientRpc] 
        private void ActualizarPantallaCentralClientRpc(string texto) 
        { 
            if(textoTiempoYAnuncios != null) textoTiempoYAnuncios.text = texto; 
        }

        [ClientRpc]
        private void ActualizarRelojUIClientRpc(float tiempo)
        {
            int min = Mathf.FloorToInt(tiempo / 60);
            int seg = Mathf.FloorToInt(tiempo % 60);
            if(textoTiempoYAnuncios != null) textoTiempoYAnuncios.text = string.Format("TIEMPO\n<size=150%>{0:00}:{1:00}</size>", min, seg);
        }

        [ClientRpc]
        private void DispararAudioClientRpc(int idAudio)
        {
            if (idAudio == 1 && fuenteSFX && audioInicioRonda) fuenteSFX.PlayOneShot(audioInicioRonda, 0.5f);
            if (idAudio == 2 && fuenteVoz && audioConteoCompleto) fuenteVoz.PlayOneShot(audioConteoCompleto);
            if (idAudio == 3 && fuenteSFX && audioDescanso) fuenteSFX.PlayOneShot(audioDescanso);
            if (idAudio == 4 && fuenteSFX && audioFinJuego) fuenteSFX.PlayOneShot(audioFinJuego);
        }

        private void ActualizarMarcadoresUI()
        {
            if (textoMarcadorP1 != null) textoMarcadorP1.text = puntosP1.Value.ToString("00");
            if (textoMarcadorP2 != null) textoMarcadorP2.text = puntosP2.Value.ToString("00");
            if (textoMarcadorP3 != null) textoMarcadorP3.text = puntosP3.Value.ToString("00");
        }

        private void CambiarMusicaConTransicion(AudioClip nuevaPista)
        {
            if (fuenteMusica == null || nuevaPista == null) return;
            if (fuenteMusica.clip == nuevaPista && fuenteMusica.isPlaying) return;

            if (rutinaTransicionMusica != null) StopCoroutine(rutinaTransicionMusica);
            rutinaTransicionMusica = StartCoroutine(RutinaFadeMusica(nuevaPista));
        }

        private IEnumerator RutinaFadeMusica(AudioClip nuevaPista)
        {
            float volInicial = fuenteMusica.volume;
            float t = 0;
            while (t < 0.8f) { t += Time.deltaTime; fuenteMusica.volume = Mathf.Lerp(volInicial, 0f, t / 0.8f); yield return null; }
            fuenteMusica.Stop();
            fuenteMusica.clip = nuevaPista;
            fuenteMusica.Play();
            t = 0;
            while (t < 0.8f) { t += Time.deltaTime; fuenteMusica.volume = Mathf.Lerp(0f, volumenMaster, t / 0.8f); yield return null; }
        }
    }
}