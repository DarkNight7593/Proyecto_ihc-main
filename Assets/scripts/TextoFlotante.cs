using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class TextoFlotante : MonoBehaviour
{
    public float velocidadSubida = 1.5f;
    public float tiempoVida = 1.5f;
    
    [Header("Intensidad del Brillo")]
    [Tooltip("Multiplicador para que el color se vuelva HDR y brille")]
    public float intensidadBrillo = 3f; 
    
    private TextMeshPro texto;
    private Color colorInicial;
    private float tiempoActual = 0f;
    private Material materialInstanciado;

    public void Configurar(string mensaje, Color color)
    {
        texto = GetComponent<TextMeshPro>();
        texto.text = mensaje;
        texto.color = color;
        colorInicial = color;
        
        // Clonamos el material para que este brillo no afecte a otros textos
        materialInstanciado = texto.fontMaterial;
        materialInstanciado.EnableKeyword("GLOW_ON");
        
        // Multiplicamos el color para que "queme" y genere el brillo neón
        Color colorBrillante = color * intensidadBrillo;
        materialInstanciado.SetColor("_GlowColor", colorBrillante);

        // Se destruye solo tras un tiempo
        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        transform.position += Vector3.up * velocidadSubida * Time.deltaTime;

        if (texto != null)
        {
            tiempoActual += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tiempoActual / tiempoVida);
            
            // Desvanecemos el color del texto
            texto.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, alpha);
            
            // Desvanecemos también el brillo
            if (materialInstanciado != null)
            {
                Color colorBrillante = colorInicial * intensidadBrillo;
                colorBrillante.a = alpha; 
                materialInstanciado.SetColor("_GlowColor", colorBrillante);
            }
        }
    }
}