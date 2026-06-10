using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// AQUÍ ESTÁ EL CAMBIO: Agregamos las interfaces separadas por comas
public class EfectoBoton3D : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    public float zPresionado = 0f; // La posición Z a la que irá al presionarse
    private float zOriginal;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        zOriginal = rectTransform.localPosition.z; // Guardará tu -10 automáticamente
    }

    // Se ejecuta al hacer clic
    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 pos = rectTransform.localPosition;
        pos.z = zPresionado;
        rectTransform.localPosition = pos;
    }

    // Se ejecuta al soltar el clic
    public void OnPointerUp(PointerEventData eventData)
    {
        Vector3 pos = rectTransform.localPosition;
        pos.z = zOriginal;
        rectTransform.localPosition = pos;
    }
}