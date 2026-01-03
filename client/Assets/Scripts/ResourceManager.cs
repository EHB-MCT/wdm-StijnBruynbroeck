using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages all game resources including gold, wood, food, stone, and population.
/// Implements singleton pattern for global access and SOLID principles.
/// Sources: Microsoft C# coding conventions, SOLID design patterns.
/// References: 
/// - https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
/// - https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/
/// </summary>
public class ResourceManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static ResourceManager Instance { get; private set; }

    #region Resource Configuration

    [Header("Resource Starting Values")]
    [Tooltip("Starting amount of gold")]
    public int gold = 10;

    [Tooltip("Starting amount of wood")]
    public int wood = 10;

    [Tooltip("Starting amount of food")]
    public int food = 15;

    [Tooltip("Starting amount of stone")]
    public int stone = 5;

    [Tooltip("Starting population count")]
    public int population = 1;

    [Header("Movement Settings")]
    [Tooltip("Resource cost for each movement action")]
    public int movementCost = 1;

    #endregion

    #region Private Fields

    private Dictionary<string, int> resourceData;
    private Dictionary<string, int> maxResources;

    #endregion

    #region Lifecycle

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

        InitializeResourceData();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject resourceManager = new GameObject("ResourceManager");
            resourceManager.AddComponent<ResourceManager>();
        }
    }

    private void Start()
    {
        Debug.Log($"ResourceManager initialized - Gold: {gold}, Wood: {wood}");
        ValidateResourceConfiguration();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Adds specified amount to a resource type.
    /// </summary>
    /// <param name="resourceType">Type of resource to add</param>
    /// <param name="amount">Amount to add</param>
    public void AddResource(string resourceType, int amount)
    {
        if (string.IsNullOrEmpty(resourceType))
        {
            Debug.LogError("Resource type cannot be null or empty");
            return;
        }

        var key = resourceType.ToLower();
        if (!resourceData.ContainsKey(key))
        {
            Debug.LogWarning($"Unknown resource type: {resourceType}");
            return;
        }

        resourceData[key] += amount;
        var newValue = resourceData[key];
        
        Debug.Log($"Added {amount} {resourceType}! Total: {newValue}");
        
        try
        {
            GameLogger.Instance?.RecordResourceManagement("Gained", resourceType.ToLower(), amount, newValue);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to log resource management: {ex.Message}");
        }

        try
        {
            if (VisualEffects.Instance != null && PlayerController.Instance != null)
            {
                var color = GetResourceColor(resourceType);
                VisualEffects.Instance.ShowResourcePopup(
                    PlayerController.Instance.transform.position, 
                    resourceType, 
                    amount, 
                    color
                );
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to show resource popup: {ex.Message}");
        }

        NotifyResourceChanged(resourceType, newValue);
    }

    /// <summary>
    /// Attempts to spend specified amount of a resource type.
    /// </summary>
    /// <param name="resourceType">Type of resource to spend</param>
    /// <param name="amount">Amount to spend</param>
    /// <returns>True if spending was successful, false otherwise</returns>
    public bool SpendResource(string resourceType, int amount)
    {
        if (string.IsNullOrEmpty(resourceType))
        {
            Debug.LogError("Resource type cannot be null or empty");
            return false;
        }

        var key = resourceType.ToLower();
        if (!resourceData.ContainsKey(key))
        {
            Debug.LogWarning($"Unknown resource type: {resourceType}");
            return false;
        }

        if (resourceData[key] < amount)
        {
            Debug.LogWarning($"Insufficient {resourceType}: have {resourceData[key]}, need {amount}");
            try
            {
                GameLogger.Instance?.RecordResourceManagement("FailedSpend", resourceType, amount, resourceData[key]);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to log failed spend: {ex.Message}");
            }
            return false;
        }

        resourceData[key] -= amount;
        var newValue = resourceData[key];
        
        Debug.Log($"Spent {amount} {resourceType}! Remaining: {newValue}");
        
        try
        {
            GameLogger.Instance?.RecordResourceManagement("Spent", resourceType, amount, newValue);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to log resource spend: {ex.Message}");
        }

        NotifyResourceChanged(resourceType, newValue);
        return true;
    }

    /// <summary>
    /// Gets current amount of specified resource type.
    /// </summary>
    /// <param name="resourceType">Type of resource to get</param>
    /// <returns>Current amount of resource, or 0 if unknown type</returns>
    public int GetResource(string resourceType)
    {
        if (string.IsNullOrEmpty(resourceType))
        {
            Debug.LogError("Resource type cannot be null or empty");
            return 0;
        }

        var key = resourceType.ToLower();
        return resourceData.ContainsKey(key) ? resourceData[key] : 0;
    }

    /// <summary>
    /// Sets resource amount to specified value.
    /// </summary>
    /// <param name="resourceType">Type of resource to set</param>
    /// <param name="amount">New amount value</param>
    public void SetResource(string resourceType, int amount)
    {
        if (string.IsNullOrEmpty(resourceType))
        {
            Debug.LogError("Resource type cannot be null or empty");
            return;
        }

        var key = resourceType.ToLower();
        var oldValue = resourceData.ContainsKey(key) ? resourceData[key] : 0;
        resourceData[key] = amount;
        
        Debug.Log($"{resourceType} set to {amount} (was {oldValue})");
        NotifyResourceChanged(resourceType, amount);
    }

    /// <summary>
    /// Checks if player has enough resources for movement.
    /// </summary>
    /// <returns>True if movement is affordable</returns>
    public bool CanAffordMove()
    {
        return resourceData.Values.Any(amount => amount >= movementCost);
    }

    /// <summary>
    /// Spends appropriate movement cost based on available resources.
    /// </summary>
    public void SpendMovementCost()
    {
        var movementCosts = new Dictionary<string, int>
        {
            { "gold", movementCost },
            { "wood", movementCost },
            { "food", movementCost }
        };

        var availableCosts = movementCosts
            .Where(kvp => resourceData.ContainsKey(kvp.Key) && resourceData[kvp.Key] >= kvp.Value)
            .ToList();

        if (!availableCosts.Any())
        {
            Debug.LogWarning("Insufficient resources for movement!");
            return;
        }

        var chosenResource = availableCosts.First();
        SpendResource(chosenResource.Key, chosenResource.Value);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initializes resource data dictionary and validates configuration.
    /// </summary>
    private void InitializeResourceData()
    {
        resourceData = new Dictionary<string, int>
        {
            { "gold", gold },
            { "wood", wood },
            { "food", food },
            { "stone", stone },
            { "population", population }
        };

        maxResources = new Dictionary<string, int>
        {
            { "gold", int.MaxValue },
            { "wood", int.MaxValue },
            { "food", int.MaxValue },
            { "stone", int.MaxValue },
            { "population", int.MaxValue }
        };
    }

    /// <summary>
    /// Validates resource configuration for consistency and logical constraints.
    /// </summary>
    private void ValidateResourceConfiguration()
    {
        if (movementCost <= 0)
        {
            Debug.LogError("Movement cost must be positive!");
        }

        var negativeResources = resourceData.Where(kvp => kvp.Value < 0).ToList();
        if (negativeResources.Any())
        {
            var negativeList = string.Join(", ", negativeResources.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            Debug.LogWarning($"Negative resource values detected: {negativeList}");
        }
    }

    /// <summary>
    /// Gets color associated with resource type for UI display.
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <returns>Unity Color for resource type</returns>
    private Color GetResourceColor(string resourceType)
    {
        return resourceType.ToLower() switch
        {
            "gold" => new Color(1f, 0.84f, 0f),      // Gold color
            "wood" => new Color(0.55f, 0.27f, 0.07f), // Brown color  
            "food" => Color.green,
            "stone" => Color.gray,
            _ => Color.white
        };
    }

    /// <summary>
    /// Notifies UI systems of resource changes.
    /// </summary>
    /// <param name="resourceType">Type that changed</param>
    /// <param name="newAmount">New amount value</param>
    private void NotifyResourceChanged(string resourceType, int newAmount)
    {
        try
        {
            if (GameUI.Instance != null)
            {
                GameUI.Instance.UpdateResourceDisplay();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to update UI: {ex.Message}");
        }

        try
        {
            var progressionSystem = FindObjectOfType<ProgressionSystem>();
            if (progressionSystem != null)
            {
                progressionSystem.OnResourceChanged(resourceType, newAmount);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to notify progression system: {ex.Message}");
        }
    }

    #endregion
}