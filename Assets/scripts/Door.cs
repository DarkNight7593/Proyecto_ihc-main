using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float rotationSpeed = 90f;
    public Vector3 openRotation = new Vector3(0, 90, 0);
    private bool isOpening = false;
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // 1. Detección para GAFAS (Mapeo universal de Unity)
        // El botón A suele mapearse como "btn 0" en el control derecho
        bool oculusButton = Input.GetKeyDown(KeyCode.JoystickButton0) || 
                            OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);

        // 2. Detección para SIMULADOR y TECLADO
        bool keyboardButton = Input.GetKeyDown(KeyCode.Space);

        if (oculusButton || keyboardButton)
        {
            isOpening = !isOpening;
        }

        // Aplicar rotación
        Quaternion target = isOpening 
            ? initialRotation * Quaternion.Euler(openRotation) 
            : initialRotation;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, 
            target, 
            rotationSpeed * Time.deltaTime
        );
    }
}