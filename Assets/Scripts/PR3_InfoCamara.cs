using UnityEngine;

// Práctica 3 · Ejercicio 10
// Va en la Main Camera (ortográfica). Informa en la consola de la altura y la anchura visibles
// cada vez que cambian el Size o la relación de aspecto de la vista Game.
// Opcional: coloca cuatro marcadores en los bordes para comprobar los cálculos.
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class PR3_InfoCamara : MonoBehaviour
{
    [Header("Marcadores (opcional)")]
    [Tooltip("Déjalo desactivado para colocar tú los objetos a mano y comprobar el cálculo. Actívalo para verificarlo.")]
    [SerializeField] private bool colocarMarcadores = false;
    [SerializeField] private Transform marcadorArriba;
    [SerializeField] private Transform marcadorAbajo;
    [SerializeField] private Transform marcadorIzquierda;
    [SerializeField] private Transform marcadorDerecha;

    private Camera cam;
    private float ultimoSize = -1f;
    private float ultimoAspect = -1f;

    private void OnEnable()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (cam == null) cam = GetComponent<Camera>();

        if (!cam.orthographic)
        {
            if (ultimoSize != -2f)
            {
                Debug.LogWarning("La cámara no es ortográfica: cambia Projection a Orthographic.", this);
                ultimoSize = -2f;
            }
            return;
        }

        float size = cam.orthographicSize;
        float aspect = cam.aspect;

        if (!Mathf.Approximately(size, ultimoSize) || !Mathf.Approximately(aspect, ultimoAspect))
        {
            float alto = 2f * size;
            float ancho = alto * aspect;
            Debug.Log(
                $"Cámara: Size = {size} | Aspecto = {aspect:F3} ({cam.pixelWidth}×{cam.pixelHeight})\n" +
                $"Visible: {ancho:F2} u de ancho × {alto:F2} u de alto (bordes Y = ±{size}, X = ±{ancho / 2f:F2})",
                this);

            ultimoSize = size;
            ultimoAspect = aspect;
        }

        if (colocarMarcadores)
            ColocarMarcadores(size, aspect);
    }

    private void ColocarMarcadores(float size, float aspect)
    {
        Vector3 c = transform.position;
        float mitadAncho = size * aspect;

        if (marcadorArriba)    marcadorArriba.position    = new Vector3(c.x, c.y + size, 0f);
        if (marcadorAbajo)     marcadorAbajo.position     = new Vector3(c.x, c.y - size, 0f);
        if (marcadorIzquierda) marcadorIzquierda.position = new Vector3(c.x - mitadAncho, c.y, 0f);
        if (marcadorDerecha)   marcadorDerecha.position   = new Vector3(c.x + mitadAncho, c.y, 0f);
    }
}
