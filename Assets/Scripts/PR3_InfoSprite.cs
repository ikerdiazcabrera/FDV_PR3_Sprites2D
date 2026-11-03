using UnityEngine;

// Práctica 3 · Ejercicios 8 y 9
// Muestra en la consola el tamaño del sprite en píxeles, su PPU y cuánto mide en unidades.
// Funciona también fuera del Play: vuelve a informar cuando cambian el PPU o la escala.
// Clic derecho en el componente → "Mostrar medidas" para forzarlo.
[ExecuteAlways]
public class PR3_InfoSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float ultimoPPU = -1f;
    private Vector3 ultimaEscala;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        MostrarMedidas();
    }

    private void Update()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        bool cambioPPU = !Mathf.Approximately(spriteRenderer.sprite.pixelsPerUnit, ultimoPPU);
        bool cambioEscala = transform.lossyScale != ultimaEscala;

        if (cambioPPU || cambioEscala)
            MostrarMedidas();
    }

    [ContextMenu("Mostrar medidas")]
    private void MostrarMedidas()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        Sprite s = spriteRenderer.sprite;
        float ppu = s.pixelsPerUnit;
        Vector2 px = s.rect.size;
        Vector2 unidadesSinEscala = px / ppu;
        Vector3 escala = transform.lossyScale;
        Vector2 unidadesConEscala = new Vector2(unidadesSinEscala.x * Mathf.Abs(escala.x),
                                                unidadesSinEscala.y * Mathf.Abs(escala.y));
        // Tamaño de un píxel del sprite en unidades de mundo (para comparar sprites en el ejercicio 9).
        float tamPixel = Mathf.Abs(escala.y) / ppu;

        Debug.Log(
            $"[{name}] Sprite '{s.name}': {px.x} × {px.y} px | PPU = {ppu}\n" +
            $"Tamaño (escala 1): {unidadesSinEscala.x:F3} × {unidadesSinEscala.y:F3} u | " +
            $"Escala {escala.x:F2}: {unidadesConEscala.x:F3} × {unidadesConEscala.y:F3} u | " +
            $"1 px del sprite = {tamPixel:F4} u",
            this);

        ultimoPPU = ppu;
        ultimaEscala = escala;
    }
}
