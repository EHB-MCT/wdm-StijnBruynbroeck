using UnityEngine;
using System.Collections;

public class VisualEffects : MonoBehaviour
{
    public static VisualEffects Instance { get; private set; }

    [Header("Effects")]
    public GameObject resourcePopupPrefab;
    public GameObject buildEffectPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject visualEffects = new GameObject("VisualEffects");
            visualEffects.AddComponent<VisualEffects>();
        }
    }

    public void ShowTribeEncounterEffect(int gridX, int gridY, string tribeType)
    {
        HexGrid grid = FindObjectOfType<HexGrid>();
        if (grid == null) return;

        GameObject tile = grid.GetTile(gridX, gridY);
        if (tile == null) return;

        Color effectColor = GetTribeColor(tribeType);
        StartCoroutine(FlashTile(tile, effectColor));
    }

    private Color GetTribeColor(string tribeType)
    {
        switch (tribeType)
        {
            case "Peaceful Traders":
                return Color.green;
            case "Warrior Clan":
                return Color.red;
            case "Mystic Shamans":
                return Color.magenta;
            default:
                return Color.yellow;
        }
    }

    private System.Collections.IEnumerator FlashTile(GameObject tile, Color flashColor)
    {
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        if (renderer == null) yield break;

        Color originalColor = renderer.color;
        
        // Flash 3 times
        for (int i = 0; i < 3; i++)
        {
            renderer.color = flashColor;
            yield return new WaitForSeconds(0.2f);
            renderer.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void ShowBuildEffect(int gridX, int gridY)
    {
        HexGrid grid = FindObjectOfType<HexGrid>();
        if (grid == null) return;

        GameObject tile = grid.GetTile(gridX, gridY);
        if (tile == null) return;

        StartCoroutine(BuildPulse(tile));
    }

    private System.Collections.IEnumerator BuildPulse(GameObject tile)
    {
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        if (renderer == null) yield break;

        Color originalColor = renderer.color;
        Vector3 originalScale = tile.transform.localScale;
        
        // Pulse effect
        for (float t = 0; t < 1f; t += Time.deltaTime * 2f)
        {
            float pulse = Mathf.Sin(t * Mathf.PI * 4f) * 0.2f + 1f;
            tile.transform.localScale = originalScale * pulse;
            renderer.color = Color.Lerp(originalColor, Color.yellow, t * 0.3f);
            yield return null;
        }
        
        tile.transform.localScale = originalScale;
        renderer.color = originalColor;
    }

    public void ShowResourcePopup(Vector3 position, string resource, int amount, Color color)
    {
        if (resourcePopupPrefab == null)
        {
            CreateDefaultResourcePopup(position, resource, amount, color);
            return;
        }

        GameObject popup = Instantiate(resourcePopupPrefab, position + Vector3.up * 2f, Quaternion.identity);
        TextMesh textMesh = popup.GetComponentInChildren<TextMesh>();
        
        if (textMesh != null)
        {
            textMesh.text = $"{(amount > 0 ? "+" : "")}{amount} {resource}";
            textMesh.color = color;
        }

        StartCoroutine(AnimatePopup(popup));
    }

    void CreateDefaultResourcePopup(Vector3 position, string resource, int amount, Color color)
    {
        GameObject popup = new GameObject("ResourcePopup");
        popup.transform.position = position + Vector3.up * 2f;

        TextMesh textMesh = popup.AddComponent<TextMesh>();
        textMesh.text = $"{(amount > 0 ? "+" : "")}{amount} {resource}";
        textMesh.color = color;
        textMesh.fontSize = 20;
        textMesh.anchor = TextAnchor.MiddleCenter;

        StartCoroutine(AnimatePopup(popup));
    }

    IEnumerator AnimatePopup(GameObject popup)
    {
        Vector3 startPos = popup.transform.position;
        Vector3 endPos = startPos + Vector3.up * 3f;
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            popup.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            
            TextMesh textMesh = popup.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                Color color = textMesh.color;
                color.a = 1f - (elapsed / duration);
                textMesh.color = color;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(popup);
    }

    public void HighlightTile(GameObject tile, Color color, float duration = 1f)
    {
        if (tile == null) return;

        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            StartCoroutine(AnimateTileHighlight(renderer, color, duration));
        }
    }

    IEnumerator AnimateTileHighlight(Renderer renderer, Color highlightColor, float duration)
    {
        Color originalColor = renderer.material.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.PingPong(elapsed * 2f, 1f);
            renderer.material.color = Color.Lerp(originalColor, highlightColor, t * 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        renderer.material.color = originalColor;
    }
}