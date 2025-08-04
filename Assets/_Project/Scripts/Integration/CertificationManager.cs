using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages certification system for lab safety certificates and competency tracking.
/// Handles certification issuance, validation, and progress tracking.
/// </summary>
public class CertificationManager : MonoBehaviour
{
    [Header("Certification Settings")]
    [SerializeField] private bool enableCertification = true;
    [SerializeField] private bool enableSafetyCertification = true;
    [SerializeField] private bool enableCompetencyTracking = true;
    [SerializeField] private bool enablePortfolioSystem = true;
    [SerializeField] private bool enableBadgeSystem = true;
    
    [Header("Safety Certification")]
    [SerializeField] private bool enableLabSafetyCert = true;
    [SerializeField] private bool enableChemicalSafetyCert = true;
    [SerializeField] private bool enableEquipmentSafetyCert = true;
    [SerializeField] private bool enableEmergencyProceduresCert = true;
    [SerializeField] private float safetyCertExpiryDays = 365f;
    
    [Header("Competency Tracking")]
    [SerializeField] private bool enableSkillAssessment = true;
    [SerializeField] private bool enableProgressTracking = true;
    [SerializeField] private bool enableCompetencyLevels = true;
    [SerializeField] private int maxCompetencyLevel = 5;
    [SerializeField] private bool enableSkillValidation = true;
    
    [Header("Portfolio System")]
    [SerializeField] private bool enableWorkPortfolio = true;
    [SerializeField] private bool enableAchievementTracking = true;
    [SerializeField] private bool enableTranscriptGeneration = true;
    [SerializeField] private bool enableCredentialVerification = true;
    [SerializeField] private int maxPortfolioItems = 100;
    
    [Header("Badge System")]
    [SerializeField] private bool enableAchievementBadges = true;
    [SerializeField] private bool enableSkillBadges = true;
    [SerializeField] private bool enableSafetyBadges = true;
    [SerializeField] private bool enableProgressionBadges = true;
    [SerializeField] private int maxBadgesPerUser = 50;
    
    [Header("Validation")]
    [SerializeField] private bool enableCertValidation = true;
    [SerializeField] private bool enableExpiryTracking = true;
    [SerializeField] private bool enableRenewalReminders = true;
    [SerializeField] private bool enableRevocationSystem = true;
    [SerializeField] private float reminderDaysBeforeExpiry = 30f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showCertificationData = false;
    [SerializeField] private bool logCertificationEvents = false;
    
    private static CertificationManager instance;
    public static CertificationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CertificationManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("CertificationManager");
                    instance = go.AddComponent<CertificationManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnCertificationIssued;
    public event Action<string> OnCertificationExpired;
    public event Action<string> OnCompetencyLevelUp;
    public event Action<string> OnBadgeEarned;
    public event Action<string> OnPortfolioUpdated;
    public event Action<string> OnSafetyViolation;
    public event Action<string> OnCertificationRevoked;
    
    [System.Serializable]
    public class Certification
    {
        public string certificationId;
        public string userId;
        public string certificationType;
        public string title;
        public string description;
        public DateTime issueDate;
        public DateTime expiryDate;
        public bool isActive;
        public float score;
        public string issuer;
        public string certificateNumber;
        public Dictionary<string, object> metadata;
        
        public Certification(string id, string user, string type)
        {
            certificationId = id;
            userId = user;
            certificationType = type;
            title = "";
            description = "";
            issueDate = DateTime.Now;
            expiryDate = DateTime.Now.AddDays(365);
            isActive = true;
            score = 0f;
            issuer = "Virtual Chemistry Lab";
            certificateNumber = "";
            metadata = new Dictionary<string, object>();
        }
    }
    
    [System.Serializable]
    public class CompetencyProfile
    {
        public string userId;
        public int overallLevel;
        public Dictionary<string, int> skillLevels;
        public Dictionary<string, float> skillScores;
        public List<string> completedExperiments;
        public List<string> masteredTechniques;
        public DateTime lastAssessment;
        public Dictionary<string, object> achievements;
        
        public CompetencyProfile(string user)
        {
            userId = user;
            overallLevel = 1;
            skillLevels = new Dictionary<string, int>();
            skillScores = new Dictionary<string, float>();
            completedExperiments = new List<string>();
            masteredTechniques = new List<string>();
            lastAssessment = DateTime.Now;
            achievements = new Dictionary<string, object>();
        }
    }
    
    [System.Serializable]
    public class PortfolioItem
    {
        public string itemId;
        public string userId;
        public string itemType;
        public string title;
        public string description;
        public DateTime creationDate;
        public string experimentId;
        public float score;
        public string evidence;
        public Dictionary<string, object> metadata;
        
        public PortfolioItem(string id, string user, string type)
        {
            itemId = id;
            userId = user;
            itemType = type;
            title = "";
            description = "";
            creationDate = DateTime.Now;
            experimentId = "";
            score = 0f;
            evidence = "";
            metadata = new Dictionary<string, object>();
        }
    }
    
    [System.Serializable]
    public class Badge
    {
        public string badgeId;
        public string userId;
        public string badgeType;
        public string title;
        public string description;
        public string iconPath;
        public DateTime earnedDate;
        public bool isActive;
        public int rarity;
        public Dictionary<string, object> criteria;
        
        public Badge(string id, string user, string type)
        {
            badgeId = id;
            userId = user;
            badgeType = type;
            title = "";
            description = "";
            iconPath = "";
            earnedDate = DateTime.Now;
            isActive = true;
            rarity = 1;
            criteria = new Dictionary<string, object>();
        }
    }
    
    [System.Serializable]
    public class SafetyRecord
    {
        public string userId;
        public List<string> safetyCertifications;
        public List<string> safetyViolations;
        public DateTime lastSafetyTraining;
        public bool isSafetyCompliant;
        public Dictionary<string, DateTime> certExpiryDates;
        public List<string> emergencyProcedures;
        
        public SafetyRecord(string user)
        {
            userId = user;
            safetyCertifications = new List<string>();
            safetyViolations = new List<string>();
            lastSafetyTraining = DateTime.Now;
            isSafetyCompliant = true;
            certExpiryDates = new Dictionary<string, DateTime>();
            emergencyProcedures = new List<string>();
        }
    }
    
    private Dictionary<string, Certification> certifications;
    private Dictionary<string, CompetencyProfile> competencyProfiles;
    private Dictionary<string, List<PortfolioItem>> portfolios;
    private Dictionary<string, List<Badge>> badges;
    private Dictionary<string, SafetyRecord> safetyRecords;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCertificationManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (enableCertification)
        {
            LoadCertificationData();
            CheckExpiringCertifications();
        }
    }
    
    private void Update()
    {
        if (enableCertification)
        {
            HandleCertificationInput();
        }
    }
    
    /// <summary>
    /// Initializes the certification manager with basic settings.
    /// </summary>
    private void InitializeCertificationManager()
    {
        if (enableDebugLogging)
        {
            Debug.Log("CertificationManager initialized successfully");
        }
        
        // Initialize collections
        certifications = new Dictionary<string, Certification>();
        competencyProfiles = new Dictionary<string, CompetencyProfile>();
        portfolios = new Dictionary<string, List<PortfolioItem>>();
        badges = new Dictionary<string, List<Badge>>();
        safetyRecords = new Dictionary<string, SafetyRecord>();
    }
    
    /// <summary>
    /// Issues a certification to a user.
    /// </summary>
    public void IssueCertification(string userId, string certificationType, string title, string description, float score = 100f)
    {
        if (!enableCertification)
            return;
        
        string certificationId = System.Guid.NewGuid().ToString();
        Certification certification = new Certification(certificationId, userId, certificationType);
        certification.title = title;
        certification.description = description;
        certification.score = score;
        certification.certificateNumber = GenerateCertificateNumber(certificationType);
        
        // Set expiry date based on certification type
        switch (certificationType.ToLower())
        {
            case "lab_safety":
                certification.expiryDate = DateTime.Now.AddDays(safetyCertExpiryDays);
                break;
            case "chemical_safety":
                certification.expiryDate = DateTime.Now.AddDays(safetyCertExpiryDays);
                break;
            case "equipment_safety":
                certification.expiryDate = DateTime.Now.AddDays(safetyCertExpiryDays);
                break;
            default:
                certification.expiryDate = DateTime.Now.AddDays(365);
                break;
        }
        
        certifications[certificationId] = certification;
        
        // Update safety record if applicable
        if (certificationType.ToLower().Contains("safety"))
        {
            UpdateSafetyRecord(userId, certificationType, certification.expiryDate);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Certification issued: {title} to {userId}");
        }
        
        OnCertificationIssued?.Invoke(certificationId);
    }
    
    /// <summary>
    /// Generates a certificate number.
    /// </summary>
    private string GenerateCertificateNumber(string certificationType)
    {
        string prefix = certificationType.ToUpper().Substring(0, Mathf.Min(3, certificationType.Length));
        string timestamp = DateTime.Now.ToString("yyyyMMdd");
        string random = UnityEngine.Random.Range(1000, 9999).ToString();
        
        return $"{prefix}-{timestamp}-{random}";
    }
    
    /// <summary>
    /// Updates safety record for a user.
    /// </summary>
    private void UpdateSafetyRecord(string userId, string certificationType, DateTime expiryDate)
    {
        if (!safetyRecords.ContainsKey(userId))
        {
            safetyRecords[userId] = new SafetyRecord(userId);
        }
        
        SafetyRecord record = safetyRecords[userId];
        
        if (!record.safetyCertifications.Contains(certificationType))
        {
            record.safetyCertifications.Add(certificationType);
        }
        
        record.certExpiryDates[certificationType] = expiryDate;
        record.lastSafetyTraining = DateTime.Now;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Safety record updated for {userId}: {certificationType}");
        }
    }
    
    /// <summary>
    /// Records a safety violation.
    /// </summary>
    public void RecordSafetyViolation(string userId, string violationType, string description)
    {
        if (!enableCertification)
            return;
        
        if (!safetyRecords.ContainsKey(userId))
        {
            safetyRecords[userId] = new SafetyRecord(userId);
        }
        
        SafetyRecord record = safetyRecords[userId];
        record.safetyViolations.Add($"{violationType}: {description}");
        
        // Check if violation affects safety compliance
        if (record.safetyViolations.Count > 3)
        {
            record.isSafetyCompliant = false;
            
            // Revoke safety certifications
            RevokeSafetyCertifications(userId);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Safety violation recorded for {userId}: {violationType}");
        }
        
        OnSafetyViolation?.Invoke(violationType);
    }
    
    /// <summary>
    /// Revokes safety certifications for a user.
    /// </summary>
    private void RevokeSafetyCertifications(string userId)
    {
        List<string> certsToRevoke = new List<string>();
        
        foreach (var kvp in certifications)
        {
            Certification cert = kvp.Value;
            if (cert.userId == userId && cert.certificationType.ToLower().Contains("safety"))
            {
                certsToRevoke.Add(cert.certificationId);
            }
        }
        
        foreach (string certId in certsToRevoke)
        {
            certifications[certId].isActive = false;
            OnCertificationRevoked?.Invoke(certId);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Safety certifications revoked for {userId}");
        }
    }
    
    /// <summary>
    /// Updates competency level for a user.
    /// </summary>
    public void UpdateCompetencyLevel(string userId, string skill, float score)
    {
        if (!enableCompetencyTracking)
            return;
        
        if (!competencyProfiles.ContainsKey(userId))
        {
            competencyProfiles[userId] = new CompetencyProfile(userId);
        }
        
        CompetencyProfile profile = competencyProfiles[userId];
        profile.skillScores[skill] = score;
        
        // Calculate skill level based on score
        int newLevel = CalculateSkillLevel(score);
        int oldLevel = profile.skillLevels.ContainsKey(skill) ? profile.skillLevels[skill] : 0;
        
        profile.skillLevels[skill] = newLevel;
        
        if (newLevel > oldLevel)
        {
            OnCompetencyLevelUp?.Invoke(skill);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Competency level up for {userId}: {skill} Level {newLevel}");
            }
        }
        
        // Update overall level
        UpdateOverallCompetencyLevel(profile);
    }
    
    /// <summary>
    /// Calculates skill level based on score.
    /// </summary>
    private int CalculateSkillLevel(float score)
    {
        if (score >= 90f) return 5;
        else if (score >= 80f) return 4;
        else if (score >= 70f) return 3;
        else if (score >= 60f) return 2;
        else return 1;
    }
    
    /// <summary>
    /// Updates overall competency level.
    /// </summary>
    private void UpdateOverallCompetencyLevel(CompetencyProfile profile)
    {
        if (profile.skillLevels.Count == 0)
            return;
        
        float totalLevel = 0f;
        foreach (int level in profile.skillLevels.Values)
        {
            totalLevel += level;
        }
        
        int newOverallLevel = Mathf.RoundToInt(totalLevel / profile.skillLevels.Count);
        newOverallLevel = Mathf.Clamp(newOverallLevel, 1, maxCompetencyLevel);
        
        if (newOverallLevel > profile.overallLevel)
        {
            profile.overallLevel = newOverallLevel;
            
            if (enableDebugLogging)
            {
                Debug.Log($"Overall competency level increased to {newOverallLevel}");
            }
        }
    }
    
    /// <summary>
    /// Adds an item to user's portfolio.
    /// </summary>
    public void AddPortfolioItem(string userId, string itemType, string title, string description, string experimentId = "", float score = 0f)
    {
        if (!enablePortfolioSystem)
            return;
        
        if (!portfolios.ContainsKey(userId))
        {
            portfolios[userId] = new List<PortfolioItem>();
        }
        
        List<PortfolioItem> userPortfolio = portfolios[userId];
        
        if (userPortfolio.Count >= maxPortfolioItems)
        {
            // Remove oldest item
            userPortfolio.RemoveAt(0);
        }
        
        string itemId = System.Guid.NewGuid().ToString();
        PortfolioItem item = new PortfolioItem(itemId, userId, itemType);
        item.title = title;
        item.description = description;
        item.experimentId = experimentId;
        item.score = score;
        
        userPortfolio.Add(item);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Portfolio item added for {userId}: {title}");
        }
        
        OnPortfolioUpdated?.Invoke(itemId);
    }
    
    /// <summary>
    /// Awards a badge to a user.
    /// </summary>
    public void AwardBadge(string userId, string badgeType, string title, string description, int rarity = 1)
    {
        if (!enableBadgeSystem)
            return;
        
        if (!badges.ContainsKey(userId))
        {
            badges[userId] = new List<Badge>();
        }
        
        List<Badge> userBadges = badges[userId];
        
        if (userBadges.Count >= maxBadgesPerUser)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning($"User {userId} has reached maximum badge limit");
            }
            return;
        }
        
        // Check if user already has this badge
        foreach (Badge badge in userBadges)
        {
            if (badge.badgeType == badgeType)
            {
                if (enableDebugLogging)
                {
                    Debug.Log($"User {userId} already has badge: {badgeType}");
                }
                return;
            }
        }
        
        string badgeId = System.Guid.NewGuid().ToString();
        Badge badge = new Badge(badgeId, userId, badgeType);
        badge.title = title;
        badge.description = description;
        badge.rarity = rarity;
        
        userBadges.Add(badge);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Badge awarded to {userId}: {title}");
        }
        
        OnBadgeEarned?.Invoke(badgeId);
    }
    
    /// <summary>
    /// Checks for expiring certifications.
    /// </summary>
    private void CheckExpiringCertifications()
    {
        if (!enableExpiryTracking)
            return;
        
        DateTime reminderDate = DateTime.Now.AddDays(reminderDaysBeforeExpiry);
        
        foreach (var kvp in certifications)
        {
            Certification cert = kvp.Value;
            
            if (cert.isActive && cert.expiryDate <= reminderDate && cert.expiryDate > DateTime.Now)
            {
                // Send renewal reminder
                if (enableDebugLogging)
                {
                    Debug.Log($"Certification expiring soon: {cert.title} for {cert.userId}");
                }
            }
            else if (cert.isActive && cert.expiryDate <= DateTime.Now)
            {
                // Expire certification
                cert.isActive = false;
                OnCertificationExpired?.Invoke(cert.certificationId);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Certification expired: {cert.title} for {cert.userId}");
                }
            }
        }
    }
    
    /// <summary>
    /// Loads certification data from storage.
    /// </summary>
    private void LoadCertificationData()
    {
        // This would load data from persistent storage
        // For now, we'll create sample data
        
        if (enableDebugLogging)
        {
            Debug.Log("Certification data loaded");
        }
    }
    
    /// <summary>
    /// Handles certification-related input.
    /// </summary>
    private void HandleCertificationInput()
    {
        // Handle certification-related input
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Check certifications
            CheckExpiringCertifications();
        }
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            // Award sample badge
            AwardBadge("sample_user", "first_experiment", "First Experiment", "Completed your first experiment", 1);
        }
    }
    
    /// <summary>
    /// Gets certifications for a user.
    /// </summary>
    public List<Certification> GetUserCertifications(string userId)
    {
        List<Certification> userCerts = new List<Certification>();
        
        foreach (Certification cert in certifications.Values)
        {
            if (cert.userId == userId)
            {
                userCerts.Add(cert);
            }
        }
        
        return userCerts;
    }
    
    /// <summary>
    /// Gets competency profile for a user.
    /// </summary>
    public CompetencyProfile GetCompetencyProfile(string userId)
    {
        if (competencyProfiles.ContainsKey(userId))
        {
            return competencyProfiles[userId];
        }
        return null;
    }
    
    /// <summary>
    /// Gets portfolio for a user.
    /// </summary>
    public List<PortfolioItem> GetUserPortfolio(string userId)
    {
        if (portfolios.ContainsKey(userId))
        {
            return new List<PortfolioItem>(portfolios[userId]);
        }
        return new List<PortfolioItem>();
    }
    
    /// <summary>
    /// Gets badges for a user.
    /// </summary>
    public List<Badge> GetUserBadges(string userId)
    {
        if (badges.ContainsKey(userId))
        {
            return new List<Badge>(badges[userId]);
        }
        return new List<Badge>();
    }
    
    /// <summary>
    /// Gets safety record for a user.
    /// </summary>
    public SafetyRecord GetSafetyRecord(string userId)
    {
        if (safetyRecords.ContainsKey(userId))
        {
            return safetyRecords[userId];
        }
        return null;
    }
    
    /// <summary>
    /// Generates a transcript for a user.
    /// </summary>
    public string GenerateTranscript(string userId)
    {
        if (!enableTranscriptGeneration)
            return "Transcript generation not enabled.";
        
        string transcript = $"TRANSCRIPT FOR USER: {userId}\n";
        transcript += $"Generated: {DateTime.Now}\n\n";
        
        // Add certifications
        List<Certification> userCerts = GetUserCertifications(userId);
        transcript += "CERTIFICATIONS:\n";
        foreach (Certification cert in userCerts)
        {
            transcript += $"- {cert.title} ({cert.certificationType})\n";
            transcript += $"  Issued: {cert.issueDate:yyyy-MM-dd}\n";
            transcript += $"  Expires: {cert.expiryDate:yyyy-MM-dd}\n";
            transcript += $"  Score: {cert.score:F1}%\n\n";
        }
        
        // Add competency profile
        CompetencyProfile profile = GetCompetencyProfile(userId);
        if (profile != null)
        {
            transcript += "COMPETENCY PROFILE:\n";
            transcript += $"Overall Level: {profile.overallLevel}\n";
            transcript += "Skill Levels:\n";
            foreach (var kvp in profile.skillLevels)
            {
                transcript += $"- {kvp.Key}: Level {kvp.Value}\n";
            }
            transcript += "\n";
        }
        
        // Add completed experiments
        if (profile != null && profile.completedExperiments.Count > 0)
        {
            transcript += "COMPLETED EXPERIMENTS:\n";
            foreach (string experiment in profile.completedExperiments)
            {
                transcript += $"- {experiment}\n";
            }
            transcript += "\n";
        }
        
        return transcript;
    }
    
    /// <summary>
    /// Validates a certification.
    /// </summary>
    public bool ValidateCertification(string certificationId)
    {
        if (!enableCertValidation)
            return false;
        
        if (certifications.ContainsKey(certificationId))
        {
            Certification cert = certifications[certificationId];
            return cert.isActive && cert.expiryDate > DateTime.Now;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if user has required certification.
    /// </summary>
    public bool HasCertification(string userId, string certificationType)
    {
        foreach (Certification cert in certifications.Values)
        {
            if (cert.userId == userId && cert.certificationType == certificationType && cert.isActive)
            {
                return cert.expiryDate > DateTime.Now;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Logs certification status for debugging.
    /// </summary>
    public void LogCertificationStatus()
    {
        if (!enableDebugLogging)
            return;
        
        Debug.Log("=== Certification Status ===");
        Debug.Log($"Certifications: {certifications.Count}");
        Debug.Log($"Competency Profiles: {competencyProfiles.Count}");
        Debug.Log($"Portfolios: {portfolios.Count}");
        Debug.Log($"Badges: {badges.Count}");
        Debug.Log($"Safety Records: {safetyRecords.Count}");
        Debug.Log("===========================");
    }
} 