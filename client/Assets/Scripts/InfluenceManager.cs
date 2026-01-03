using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

/// <summary>
/// Advanced behavioral influence system that applies psychological mechanisms to affect player decisions.
/// Implements A/B testing and adaptive influence based on player profiles.
/// Sources: Cognitive psychology research, behavioral economics principles.
/// Reference: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
/// </summary>
[DefaultExecutionOrder(-900)]
public class InfluenceManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static InfluenceManager Instance;
    
    [Header("Influence Configuration")]
    [SerializeField] private bool enableInfluenceSystem = true;
    [SerializeField] private float baseInfluenceStrength = 0.3f;
    [SerializeField] private bool enableABTesting = true;
    
    [Header("Dynamic Difficulty")]
    [SerializeField] private bool enableDynamicDifficulty = true;
    [SerializeField] private float difficultyAdjustmentRate = 0.1f;
    
    private UserProfile currentPlayerProfile;
    private Dictionary<string, float> influenceEffectiveness = new Dictionary<string, float>();
    private string currentTestGroup = "control";
    private float currentDifficultyMultiplier = 1.0f;
    private string playerUID = "";
    
    [System.Serializable]
    public class InfluenceData
    {
        public string type;
        public string mechanism;
        public Dictionary<string, object> parameters;
        public string context;
        public float strength;
        public bool playerResisted;
    }
    
    [System.Serializable]
    public class UserProfile
    {
        public string uid;
        public float riskTolerance;
        public float decisionSpeed;
        public float engagementLevel;
        public float strategicScore;
        public float influenceSusceptibility;
        public float skillLevel;
        public string playerType;
    }
    
    [System.Serializable]
    public class InfluenceResponse
    {
        public string originalChoice;
        public string influencedChoice;
        public float deliberationTime;
        public bool wasInfluenced;
        public string influenceType;
        public float influenceStrength;
    }
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        StartCoroutine(InitializeInfluenceSystem());
    }
    
    private IEnumerator InitializeInfluenceSystem()
    {
        if (!enableInfluenceSystem)
        {
            Debug.Log("Influence system disabled");
            yield break;
        }
        
        // Get player UID from PlayerPrefs (same as GameLogger)
        if (PlayerPrefs.HasKey("PlayerUID"))
        {
            playerUID = PlayerPrefs.GetString("PlayerUID");
        }
        else
        {
            playerUID = "Player_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            PlayerPrefs.SetString("PlayerUID", playerUID);
            PlayerPrefs.Save();
        }
        
        // Load user profile
        yield return StartCoroutine(LoadUserProfile());
        
        // Initialize A/B testing
        if (enableABTesting)
        {
            yield return StartCoroutine(InitializeABTesting());
        }
        
        Debug.Log($"Influence system initialized for {playerUID}");
    }
    
    private IEnumerator LoadUserProfile()
    {
        string apiUrl = "http://localhost:8080/api/analytics/" + playerUID;
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                currentPlayerProfile = JsonUtility.FromJson<UserProfile>(jsonResponse);
                Debug.Log("User profile loaded successfully");
            }
            else
            {
                Debug.LogWarning($"Failed to load user profile: {webRequest.error}");
                // Create default profile
                currentPlayerProfile = CreateDefaultProfile(playerUID);
            }
        }
    }
    
    private UserProfile CreateDefaultProfile(string uid)
    {
        return new UserProfile
        {
            uid = uid,
            riskTolerance = 0.5f,
            decisionSpeed = 0.5f,
            engagementLevel = 0.5f,
            strategicScore = 0.5f,
            influenceSusceptibility = 0.5f,
            skillLevel = 0.5f,
            playerType = "Balanced"
        };
    }
    
    private IEnumerator InitializeABTesting()
    {
        string apiUrl = "http://localhost:8080/api/abtest/framing_effects/assign/" + playerUID;
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                currentTestGroup = webRequest.downloadHandler.text.Trim('"');
                Debug.Log($"A/B test assignment: {currentTestGroup}");
            }
            else
            {
                Debug.LogWarning($"Failed to get A/B test assignment: {webRequest.error}");
            }
        }
    }
    
    #region Framing Effects
    
    public string ApplyFramingEffect(string decisionType, string originalText)
    {
        if (!enableInfluenceSystem || currentPlayerProfile == null)
            return originalText;
        
        string framedText = originalText;
        string influenceType = "framing";
        
        switch (decisionType.ToLower())
        {
            case "threat":
                framedText = ApplyThreatFraming(originalText);
                break;
            case "resource":
                framedText = ApplyResourceFraming(originalText);
                break;
            case "quest":
                framedText = ApplyQuestFraming(originalText);
                break;
            case "building":
                framedText = ApplyBuildingFraming(originalText);
                break;
        }
        
        if (framedText != originalText)
        {
            RecordInfluenceEvent(influenceType, decisionType, framedText, originalText);
        }
        
        return framedText;
    }
    
    private string ApplyThreatFraming(string originalText)
    {
        // Use gain framing for risk-takers, loss framing for risk-averse players
        if (currentPlayerProfile.riskTolerance > 0.6f)
        {
            return originalText.Replace("Pay off", "Secure your territory and gain bonus resources by paying off");
        }
        else
        {
            return originalText.Replace("Pay off", "Avoid losing 50% of your resources by paying off now");
        }
    }
    
    private string ApplyResourceFraming(string originalText)
    {
        if (currentPlayerProfile.engagementLevel > 0.7f)
        {
            return "⚠️ LIMITED OFFER: " + originalText + " (Available for next 60 seconds only!)";
        }
        
        return "🌟 POPULAR CHOICE: " + originalText + " (85% of players choose this option)";
    }
    
    private string ApplyQuestFraming(string originalText)
    {
        switch (currentPlayerProfile.playerType)
        {
            case "Aggressive":
                return originalText.Replace("quest", "CONQUEST opportunity");
            case "Cautious":
                return originalText.Replace("quest", "SAFE exploration quest");
            default:
                return originalText;
        }
    }
    
    private string ApplyBuildingFraming(string originalText)
    {
        if (currentPlayerProfile.decisionSpeed < 3f)
        {
            return "🏗️ RECOMMENDED: " + originalText + " (Perfect for your current strategy)";
        }
        
        return originalText;
    }
    
    #endregion
    
    #region Dynamic Difficulty
    
    public float GetAdjustedDifficulty()
    {
        if (!enableDynamicDifficulty || currentPlayerProfile == null)
            return 1.0f;
        
        return currentDifficultyMultiplier;
    }
    
    public void UpdateDynamicDifficulty(float playerPerformance)
    {
        if (!enableDynamicDifficulty || currentPlayerProfile == null)
            return;
        
        if (playerPerformance > 0.8f)
        {
            currentDifficultyMultiplier += difficultyAdjustmentRate;
        }
        else if (playerPerformance < 0.3f)
        {
            currentDifficultyMultiplier -= difficultyAdjustmentRate;
        }
        
        currentDifficultyMultiplier = Mathf.Clamp(currentDifficultyMultiplier, 0.5f, 2.0f);
        
        Debug.Log($"Difficulty adjusted to: {currentDifficultyMultiplier:F2}");
    }
    
    public int GetAdjustedThreatStrength()
    {
        float baseThreatStrength = 50;
        return Mathf.RoundToInt(baseThreatStrength * currentDifficultyMultiplier);
    }
    
    public int GetAdjustedResourceReward()
    {
        float baseReward = 25;
        return Mathf.RoundToInt(baseReward * (2.0f - currentDifficultyMultiplier));
    }
    
    #endregion
    
    #region Influence Tracking
    
    private void RecordInfluenceEvent(string influenceType, string context, object influencedValue, object originalValue)
    {
        var influenceData = new InfluenceData
        {
            type = influenceType,
            mechanism = GetMechanismFromType(influenceType),
            parameters = new Dictionary<string, object>
            {
                ["original"] = originalValue,
                ["influenced"] = influencedValue,
                ["test_group"] = currentTestGroup,
                ["timestamp"] = Time.time
            },
            context = context,
            strength = CalculateInfluenceStrength(influenceType),
            playerResisted = false
        };
        
        StartCoroutine(SendInfluenceData(influenceData));
    }
    
    private string GetMechanismFromType(string influenceType)
    {
        switch (influenceType)
        {
            case "framing": return "loss_aversion_gain_framing";
            case "anchoring": return "default_option_suggestion";
            case "scarcity": return "limited_quantity_indicator";
            case "urgency": return "time_pressure_effect";
            case "social_proof": return "peer_validation";
            default: return "unknown_mechanism";
        }
    }
    
    private float CalculateInfluenceStrength(string influenceType)
    {
        float baseStrength = baseInfluenceStrength;
        
        if (currentPlayerProfile?.influenceSusceptibility != null)
        {
            baseStrength *= currentPlayerProfile.influenceSusceptibility;
        }
        
        if (influenceEffectiveness.ContainsKey(influenceType))
        {
            baseStrength *= influenceEffectiveness[influenceType];
        }
        
        return Mathf.Clamp(baseStrength, 0f, 1f);
    }
    
    private IEnumerator SendInfluenceData(InfluenceData data)
    {
        string apiUrl = "http://localhost:8080/api/influence/" + playerUID;
        
        string jsonData = JsonUtility.ToJson(data);
        
        using (UnityWebRequest webRequest = UnityWebRequest.Post(apiUrl, jsonData, "application/json"))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Failed to send influence data: {webRequest.error}");
            }
        }
    }
    
    public void RecordPlayerResponse(string influenceType, string playerAction, float deliberationTime, bool wasInfluenced)
    {
        if (!influenceEffectiveness.ContainsKey(influenceType))
        {
            influenceEffectiveness[influenceType] = 0.5f;
        }
        
        float oldEffectiveness = influenceEffectiveness[influenceType];
        float learningRate = 0.1f;
        
        if (wasInfluenced)
        {
            influenceEffectiveness[influenceType] = oldEffectiveness + learningRate * (1.0f - oldEffectiveness);
        }
        else
        {
            influenceEffectiveness[influenceType] = oldEffectiveness + learningRate * (0.0f - oldEffectiveness);
        }
    }
    
    #endregion
    
    #region Public API
    
    public UserProfile GetCurrentProfile()
    {
        return currentPlayerProfile;
    }
    
    public string GetCurrentTestGroup()
    {
        return currentTestGroup;
    }
    
    public float GetInfluenceEffectiveness(string influenceType)
    {
        return influenceEffectiveness.ContainsKey(influenceType) ? influenceEffectiveness[influenceType] : 0.5f;
    }
    
    public void SetInfluenceEnabled(bool enabled)
    {
        enableInfluenceSystem = enabled;
        Debug.Log($"Influence system {(enabled ? "enabled" : "disabled")}");
    }
    
    public void RefreshProfile()
    {
        StartCoroutine(LoadUserProfile());
    }
    
    public string GetPlayerUID()
    {
        return playerUID;
    }
    
    // Missing influence methods needed by GameUI
    public string ApplyAnchoringEffect(string decisionType, string originalText, int suggestedAmount = -1)
    {
        if (!enableInfluenceSystem || currentPlayerProfile == null)
            return originalText;
        
        int anchorAmount = suggestedAmount > 0 ? suggestedAmount : 25;
        return originalText + $"\n💡 SUGGESTED: {anchorAmount} resources (Most players choose this amount)";
    }
    
    public string ApplyScarcityEffect(string decisionType, string originalText)
    {
        if (!enableInfluenceSystem || currentPlayerProfile == null)
            return originalText;
        
        return $"⏰ LIMITED TIME: {originalText} (Expires in 2 minutes!)";
    }
    
    public string ApplySocialProof(string decisionType, string originalText)
    {
        if (!enableInfluenceSystem || currentPlayerProfile == null)
            return originalText;
        
        return originalText + "\n👥 85% of successful players choose this option";
    }
    
    #endregion
}