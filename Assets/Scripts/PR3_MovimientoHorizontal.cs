using UnityEngine;
using UnityEngine.InputSystem;

// Práctica 3 · Ejercicios 3 a 6 (y parámetro isWalking del ejercicio 7)
// Mueve el personaje en horizontal con las flechas (o A/D) usando transform.Translate
// y orienta el sprite con flipX según la dirección del movimiento.
// Usa el Input System nuevo, pero el eje se comporta como Input.GetAxis("Horizontal").
public class PR3_MovimientoHorizontal : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 4f;

    [Header("Eje horizontal (imita Input.GetAxis)")]
    [Tooltip("Activo: el eje sube y baja gradualmente como Input.GetAxis. Inactivo: salta de 0 a 1 como GetAxisRaw.")]
    [SerializeField] private bool suavizarEje = true;
    [Tooltip("Unidades de eje por segundo (el Input Manager clásico usa 3).")]
    [SerializeField] private float sensibilidad = 3f;

    [Header("Orientación")]
    [Tooltip("Márcalo si el sprite original mira hacia la derecha.")]
    [SerializeField] private bool spriteMiraDerecha = true;

    [Header("Animator (opcional)")]
    [SerializeField] private string parametroCaminando = "isWalking";

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool animatorTieneParametro;
    private float horizontalInput;

    private void Awake()
    {
        // Las referencias se obtienen una sola vez, no en cada frame.
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        animatorTieneParametro = TieneParametroBool(animator, parametroCaminando);
    }

    private void Update()
    {
        // Ej. 4: el avance solo se produce si se pulsan las flechas izquierda/derecha.
        float objetivo = LeerEjeHorizontal();

        if (suavizarEje)
        {
            // "Snap": al cambiar de sentido el eje vuelve a 0 de golpe, igual que el Input Manager.
            if (objetivo != 0f && horizontalInput != 0f && Mathf.Sign(objetivo) != Mathf.Sign(horizontalInput))
                horizontalInput = 0f;

            horizontalInput = Mathf.MoveTowards(horizontalInput, objetivo, sensibilidad * Time.deltaTime);
        }
        else
        {
            horizontalInput = objetivo;
        }

        // Ej. 3 y 5: movimiento solo en X con Translate.
        Vector2 movement = new Vector2(horizontalInput, 0f);
        transform.Translate(movement * speed * Time.deltaTime);

        // Ej. 6: orientar el sprite hacia donde camina.
        if (spriteRenderer != null)
        {
            if (horizontalInput > 0f)
                spriteRenderer.flipX = !spriteMiraDerecha;   // hacia la derecha
            else if (horizontalInput < 0f)
                spriteRenderer.flipX = spriteMiraDerecha;    // hacia la izquierda
            // Si es 0, se mantiene la última orientación.
        }

        // Ej. 7: informar al Animator de si está caminando.
        if (animatorTieneParametro)
            animator.SetBool(parametroCaminando, Mathf.Abs(horizontalInput) > 0.01f);
    }

    private float LeerEjeHorizontal()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado == null) return 0f;

        float valor = 0f;
        if (teclado.leftArrowKey.isPressed || teclado.aKey.isPressed) valor -= 1f;
        if (teclado.rightArrowKey.isPressed || teclado.dKey.isPressed) valor += 1f;
        return valor;
    }

    // Comprueba que el Animator tiene el parámetro Bool para no llenar la consola de avisos.
    public static bool TieneParametroBool(Animator anim, string nombre)
    {
        if (anim == null || anim.runtimeAnimatorController == null || string.IsNullOrEmpty(nombre))
            return false;

        foreach (AnimatorControllerParameter p in anim.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Bool && p.name == nombre)
                return true;
        }

        Debug.LogWarning($"[{anim.name}] El Animator no tiene un parámetro Bool llamado \"{nombre}\".", anim);
        return false;
    }
}
