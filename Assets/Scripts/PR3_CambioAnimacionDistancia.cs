using UnityEngine;

// Práctica 3 · Ejercicio 7
// Acumula la distancia recorrida frame a frame y, cuando supera la distancia de activación,
// le indica al Animator que pase a otra animación (parámetro Bool).
public class PR3_CambioAnimacionDistancia : MonoBehaviour
{
    [Header("Activación")]
    [Tooltip("Distancia (en unidades) que hay que recorrer para cambiar de animación.")]
    [SerializeField] private float distanciaActivacion = 10f;
    [SerializeField] private string parametroAnimator = "cansado";

    [Header("Estado (solo lectura, se ve en el Inspector durante el Play)")]
    [SerializeField] private float distanciaRecorrida;     // suma de lo que se ha movido en cada frame
    [SerializeField] private float distanciaDesdeInicio;   // desplazamiento neto respecto a la posición inicial
    [SerializeField] private bool activado;

    private Animator animator;
    private bool animatorTieneParametro;
    private Vector3 firstPosition;      // posición inicial
    private Vector3 posicionAnterior;   // posición del frame anterior

    private void Awake()
    {
        // Referencia al Animator una sola vez.
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        animatorTieneParametro = PR3_MovimientoHorizontal.TieneParametroBool(animator, parametroAnimator);
        firstPosition = transform.position;
        posicionAnterior = firstPosition;
    }

    private void Update()
    {
        // Cuánto se ha movido en este frame (posición actual frente a la anterior).
        float distanceThisFrame = Vector3.Distance(posicionAnterior, transform.position);
        distanciaRecorrida += distanceThisFrame;
        posicionAnterior = transform.position;

        // Cuánto se ha alejado de la posición inicial (no crece si va y vuelve).
        distanciaDesdeInicio = Vector3.Distance(firstPosition, transform.position);

        if (!activado && distanciaRecorrida >= distanciaActivacion)
        {
            activado = true;
            Debug.Log($"{name} ha recorrido {distanciaRecorrida:F2} u (≥ {distanciaActivacion}). Cambio de animación.", this);

            if (animatorTieneParametro)
                animator.SetBool(parametroAnimator, true);
        }
    }

    // Clic derecho en el componente → "Reiniciar distancia" (útil para repetir la prueba sin parar el Play).
    [ContextMenu("Reiniciar distancia")]
    private void Reiniciar()
    {
        distanciaRecorrida = 0f;
        activado = false;
        firstPosition = transform.position;
        posicionAnterior = firstPosition;

        if (animatorTieneParametro)
            animator.SetBool(parametroAnimator, false);
    }
}
