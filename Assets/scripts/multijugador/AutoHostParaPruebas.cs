using UnityEngine;
using Unity.Netcode;

namespace Multijugador
{
    public class AutoHostParaPruebas : MonoBehaviour
    {
        void Start()
        {
            // Buscamos el NetworkManager de la escena
            NetworkManager netManager = FindFirstObjectByType<NetworkManager>();

            if (netManager != null)
            {
                // 🔥 Esto le dice a Unity: "Apenas entres a la escena, arranca como HOST automáticamente"
                netManager.StartHost();
                Debug.Log("🚀 Netcode Iniciado automáticamente como HOST para pruebas.");
            }
            else
            {
                Debug.LogError("❌ No se encontró un NetworkManager en la escena. ¡Asegúrate de tener uno!");
            }
        }
    }
}