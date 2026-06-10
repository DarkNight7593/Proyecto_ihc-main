using UnityEngine;
using UnityEngine.UI;
public class ControladorMenu : MonoBehaviour
{
[Header("Componentes Outline de Gravedad")]
    public Outline outlineGravedadLunar;
    public Outline outlineGravedadNormal;

    [Header("Componentes Outline de Modo")]
    public Outline outlineModoDesafio;
    public Outline outlineModoNormal;

    void Start()
    {
        // Aseguramos el estado por defecto al iniciar
        SeleccionarGravedadNormal();
        SeleccionarModoNormal();
    }

    // --- MÉTODOS PARA GRAVEDAD ---
    public void SeleccionarGravedadLunar()
    {
        outlineGravedadLunar.enabled = true;
        outlineGravedadNormal.enabled = false;
    }

    public void SeleccionarGravedadNormal()
    {
        outlineGravedadLunar.enabled = false;
        outlineGravedadNormal.enabled = true;
    }

    // --- MÉTODOS PARA MODO ---
    public void SeleccionarModoDesafio()
    {
        outlineModoDesafio.enabled = true;
        outlineModoNormal.enabled = false;
    }

    public void SeleccionarModoNormal()
    {
        outlineModoDesafio.enabled = false;
        outlineModoNormal.enabled = true;
    }
}
