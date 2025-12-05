using UnityEngine;

public class VisualEffects : MonoBehaviour
{
    public static VisualEffects Instance { get; private set; }

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
}