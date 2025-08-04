using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// Manages internationalization and localization for multi-language support and cultural adaptation.
/// Handles language switching, cultural content adaptation, and regional curriculum alignment.
/// </summary>
public class InternationalizationManager : MonoBehaviour
{
    [Header("Internationalization Settings")]
    [SerializeField] private bool enableI18n = true;
    [SerializeField] private string defaultLanguage = "en";
    [SerializeField] private string currentLanguage = "en";
    [SerializeField] private bool enableAutoLanguageDetection = true;
    [SerializeField] private bool enableCulturalAdaptation = true;
    [SerializeField] private bool enableRegionalCurriculum = true;
    
    [Header("Language Support")]
    [SerializeField] private bool enableEnglish = true;
    [SerializeField] private bool enableSpanish = true;
    [SerializeField] private bool enableFrench = true;
    [SerializeField] private bool enableGerman = true;
    [SerializeField] private bool enableChinese = true;
    [SerializeField] private bool enableJapanese = true;
    [SerializeField] private bool enableArabic = true;
    [SerializeField] private bool enableHindi = true;
    
    [Header("Cultural Adaptation")]
    [SerializeField] private bool enableCulturalContent = true;
    [SerializeField] private bool enableRegionalExamples = true;
    [SerializeField] private bool enableLocalUnits = true;
    [SerializeField] private bool enableCulturalSensitivity = true;
    [SerializeField] private bool enableRegionalSafety = true;
    
    [Header("Accessibility")]
    [SerializeField] private bool enableScreenReader = true;
    [SerializeField] private bool enableHighContrast = true;
    [SerializeField] private bool enableLargeText = true;
    [SerializeField] private bool enableColorBlindSupport = true;
    [SerializeField] private bool enableKeyboardNavigation = true;
    
    [Header("Regional Settings")]
    [SerializeField] private bool enableRegionalFormats = true;
    [SerializeField] private bool enableLocalCurrency = false;
    [SerializeField] private bool enableRegionalTime = true;
    [SerializeField] private bool enableLocalHolidays = true;
    [SerializeField] private bool enableRegionalStandards = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showLanguageInfo = false;
    [SerializeField] private bool logTranslationEvents = false;
    
    private static InternationalizationManager instance;
    public static InternationalizationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InternationalizationManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("InternationalizationManager");
                    instance = go.AddComponent<InternationalizationManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnLanguageChanged;
    public event Action<string> OnCultureChanged;
    public event Action<string> OnAccessibilityChanged;
    public event Action<string> OnRegionalSettingsChanged;
    public event Action<string> OnTranslationRequested;
    public event Action<string> OnContentAdapted;
    
    [System.Serializable]
    public class LanguageData
    {
        public string languageCode;
        public string languageName;
        public string nativeName;
        public bool isRTL;
        public string fontFamily;
        public Dictionary<string, string> translations;
        public Dictionary<string, object> culturalSettings;
        
        public LanguageData(string code, string name)
        {
            languageCode = code;
            languageName = name;
            nativeName = name;
            isRTL = false;
            fontFamily = "Arial";
            translations = new Dictionary<string, string>();
            culturalSettings = new Dictionary<string, object>();
        }
    }
    
    [System.Serializable]
    public class CulturalData
    {
        public string cultureCode;
        public string regionName;
        public string dateFormat;
        public string timeFormat;
        public string numberFormat;
        public string currency;
        public List<string> holidays;
        public Dictionary<string, object> customs;
        
        public CulturalData(string code, string region)
        {
            cultureCode = code;
            regionName = region;
            dateFormat = "MM/dd/yyyy";
            timeFormat = "HH:mm";
            numberFormat = "0.00";
            currency = "USD";
            holidays = new List<string>();
            customs = new Dictionary<string, object>();
        }
    }
    
    [System.Serializable]
    public class AccessibilitySettings
    {
        public bool screenReaderEnabled;
        public bool highContrastEnabled;
        public bool largeTextEnabled;
        public bool colorBlindSupportEnabled;
        public bool keyboardNavigationEnabled;
        public float textScale;
        public string colorScheme;
        public Dictionary<string, object> preferences;
        
        public AccessibilitySettings()
        {
            screenReaderEnabled = false;
            highContrastEnabled = false;
            largeTextEnabled = false;
            colorBlindSupportEnabled = false;
            keyboardNavigationEnabled = false;
            textScale = 1.0f;
            colorScheme = "default";
            preferences = new Dictionary<string, object>();
        }
    }
    
    [System.Serializable]
    public class RegionalCurriculum
    {
        public string regionCode;
        public string curriculumName;
        public List<string> requiredTopics;
        public List<string> optionalTopics;
        public Dictionary<string, object> standards;
        public List<string> assessmentMethods;
        
        public RegionalCurriculum(string code, string name)
        {
            regionCode = code;
            curriculumName = name;
            requiredTopics = new List<string>();
            optionalTopics = new List<string>();
            standards = new Dictionary<string, object>();
            assessmentMethods = new List<string>();
        }
    }
    
    private Dictionary<string, LanguageData> supportedLanguages;
    private Dictionary<string, CulturalData> culturalData;
    private Dictionary<string, RegionalCurriculum> regionalCurricula;
    private AccessibilitySettings accessibilitySettings;
    private CulturalData currentCulture;
    private RegionalCurriculum currentCurriculum;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInternationalizationManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (enableI18n)
        {
            LoadLanguageData();
            DetectSystemLanguage();
            SetupAccessibility();
        }
    }
    
    private void Update()
    {
        if (enableI18n)
        {
            HandleI18nInput();
        }
    }
    
    /// <summary>
    /// Initializes the internationalization manager with basic settings.
    /// </summary>
    private void InitializeInternationalizationManager()
    {
        if (enableDebugLogging)
        {
            Debug.Log("InternationalizationManager initialized successfully");
        }
        
        // Initialize collections
        supportedLanguages = new Dictionary<string, LanguageData>();
        culturalData = new Dictionary<string, CulturalData>();
        regionalCurricula = new Dictionary<string, RegionalCurriculum>();
        accessibilitySettings = new AccessibilitySettings();
        
        // Setup default language
        SetupDefaultLanguage();
    }
    
    /// <summary>
    /// Sets up the default language and cultural data.
    /// </summary>
    private void SetupDefaultLanguage()
    {
        // English (US)
        LanguageData english = new LanguageData("en", "English");
        english.nativeName = "English";
        english.isRTL = false;
        english.fontFamily = "Arial";
        supportedLanguages["en"] = english;
        
        // Spanish
        LanguageData spanish = new LanguageData("es", "Spanish");
        spanish.nativeName = "Español";
        spanish.isRTL = false;
        spanish.fontFamily = "Arial";
        supportedLanguages["es"] = spanish;
        
        // French
        LanguageData french = new LanguageData("fr", "French");
        french.nativeName = "Français";
        french.isRTL = false;
        french.fontFamily = "Arial";
        supportedLanguages["fr"] = french;
        
        // German
        LanguageData german = new LanguageData("de", "German");
        german.nativeName = "Deutsch";
        german.isRTL = false;
        german.fontFamily = "Arial";
        supportedLanguages["de"] = german;
        
        // Chinese (Simplified)
        LanguageData chinese = new LanguageData("zh", "Chinese");
        chinese.nativeName = "中文";
        chinese.isRTL = false;
        chinese.fontFamily = "SimHei";
        supportedLanguages["zh"] = chinese;
        
        // Japanese
        LanguageData japanese = new LanguageData("ja", "Japanese");
        japanese.nativeName = "日本語";
        japanese.isRTL = false;
        japanese.fontFamily = "MS Gothic";
        supportedLanguages["ja"] = japanese;
        
        // Arabic
        LanguageData arabic = new LanguageData("ar", "Arabic");
        arabic.nativeName = "العربية";
        arabic.isRTL = true;
        arabic.fontFamily = "Arial";
        supportedLanguages["ar"] = arabic;
        
        // Hindi
        LanguageData hindi = new LanguageData("hi", "Hindi");
        hindi.nativeName = "हिन्दी";
        hindi.isRTL = false;
        hindi.fontFamily = "Arial";
        supportedLanguages["hi"] = hindi;
        
        // Setup cultural data
        SetupCulturalData();
        
        // Setup regional curricula
        SetupRegionalCurricula();
    }
    
    /// <summary>
    /// Sets up cultural data for different regions.
    /// </summary>
    private void SetupCulturalData()
    {
        // US Culture
        CulturalData usCulture = new CulturalData("en-US", "United States");
        usCulture.dateFormat = "MM/dd/yyyy";
        usCulture.timeFormat = "h:mm tt";
        usCulture.numberFormat = "0.00";
        usCulture.currency = "USD";
        usCulture.holidays.AddRange(new string[] { "Independence Day", "Thanksgiving", "Christmas" });
        culturalData["en-US"] = usCulture;
        
        // UK Culture
        CulturalData ukCulture = new CulturalData("en-GB", "United Kingdom");
        ukCulture.dateFormat = "dd/MM/yyyy";
        ukCulture.timeFormat = "HH:mm";
        ukCulture.numberFormat = "0.00";
        ukCulture.currency = "GBP";
        ukCulture.holidays.AddRange(new string[] { "Christmas", "Easter", "Bank Holidays" });
        culturalData["en-GB"] = ukCulture;
        
        // Spanish Culture
        CulturalData esCulture = new CulturalData("es-ES", "Spain");
        esCulture.dateFormat = "dd/MM/yyyy";
        esCulture.timeFormat = "HH:mm";
        esCulture.numberFormat = "0,00";
        esCulture.currency = "EUR";
        esCulture.holidays.AddRange(new string[] { "Navidad", "Semana Santa", "Día de la Hispanidad" });
        culturalData["es-ES"] = esCulture;
        
        // French Culture
        CulturalData frCulture = new CulturalData("fr-FR", "France");
        frCulture.dateFormat = "dd/MM/yyyy";
        frCulture.timeFormat = "HH:mm";
        frCulture.numberFormat = "0,00";
        frCulture.currency = "EUR";
        frCulture.holidays.AddRange(new string[] { "Noël", "Pâques", "Bastille Day" });
        culturalData["fr-FR"] = frCulture;
        
        // German Culture
        CulturalData deCulture = new CulturalData("de-DE", "Germany");
        deCulture.dateFormat = "dd.MM.yyyy";
        deCulture.timeFormat = "HH:mm";
        deCulture.numberFormat = "0,00";
        deCulture.currency = "EUR";
        deCulture.holidays.AddRange(new string[] { "Weihnachten", "Ostern", "Tag der Deutschen Einheit" });
        culturalData["de-DE"] = deCulture;
        
        // Chinese Culture
        CulturalData zhCulture = new CulturalData("zh-CN", "China");
        zhCulture.dateFormat = "yyyy-MM-dd";
        zhCulture.timeFormat = "HH:mm";
        zhCulture.numberFormat = "0.00";
        zhCulture.currency = "CNY";
        zhCulture.holidays.AddRange(new string[] { "春节", "中秋节", "国庆节" });
        culturalData["zh-CN"] = zhCulture;
        
        // Japanese Culture
        CulturalData jaCulture = new CulturalData("ja-JP", "Japan");
        jaCulture.dateFormat = "yyyy/MM/dd";
        jaCulture.timeFormat = "HH:mm";
        jaCulture.numberFormat = "0.00";
        jaCulture.currency = "JPY";
        jaCulture.holidays.AddRange(new string[] { "お正月", "こどもの日", "文化の日" });
        culturalData["ja-JP"] = jaCulture;
    }
    
    /// <summary>
    /// Sets up regional curricula for different education systems.
    /// </summary>
    private void SetupRegionalCurricula()
    {
        // US Curriculum
        RegionalCurriculum usCurriculum = new RegionalCurriculum("en-US", "US Chemistry Standards");
        usCurriculum.requiredTopics.AddRange(new string[] {
            "Atomic Structure",
            "Chemical Bonding",
            "Stoichiometry",
            "Acid-Base Chemistry",
            "Thermochemistry"
        });
        usCurriculum.assessmentMethods.AddRange(new string[] {
            "Laboratory Reports",
            "Written Examinations",
            "Practical Assessments"
        });
        regionalCurricula["en-US"] = usCurriculum;
        
        // UK Curriculum
        RegionalCurriculum ukCurriculum = new RegionalCurriculum("en-GB", "UK Chemistry Curriculum");
        ukCurriculum.requiredTopics.AddRange(new string[] {
            "Atomic Structure and Bonding",
            "Energetics",
            "Equilibria",
            "Redox Reactions",
            "Organic Chemistry"
        });
        ukCurriculum.assessmentMethods.AddRange(new string[] {
            "Practical Skills Assessment",
            "Written Examinations",
            "Coursework"
        });
        regionalCurricula["en-GB"] = ukCurriculum;
        
        // International Baccalaureate
        RegionalCurriculum ibCurriculum = new RegionalCurriculum("ib", "IB Chemistry");
        ibCurriculum.requiredTopics.AddRange(new string[] {
            "Stoichiometric Relationships",
            "Atomic Structure",
            "Periodicity",
            "Chemical Bonding",
            "Energetics/Thermochemistry"
        });
        ibCurriculum.assessmentMethods.AddRange(new string[] {
            "Internal Assessment",
            "External Assessment",
            "Practical Work"
        });
        regionalCurricula["ib"] = ibCurriculum;
    }
    
    /// <summary>
    /// Loads language data and translations.
    /// </summary>
    private void LoadLanguageData()
    {
        // Load translations for each language
        LoadTranslations("en");
        LoadTranslations("es");
        LoadTranslations("fr");
        LoadTranslations("de");
        LoadTranslations("zh");
        LoadTranslations("ja");
        LoadTranslations("ar");
        LoadTranslations("hi");
        
        if (enableDebugLogging)
        {
            Debug.Log("Language data loaded successfully");
        }
    }
    
    /// <summary>
    /// Loads translations for a specific language.
    /// </summary>
    private void LoadTranslations(string languageCode)
    {
        if (!supportedLanguages.ContainsKey(languageCode))
            return;
        
        LanguageData language = supportedLanguages[languageCode];
        
        // Load basic translations
        switch (languageCode)
        {
            case "en":
                language.translations["welcome"] = "Welcome to Virtual Chemistry Laboratory";
                language.translations["start_experiment"] = "Start Experiment";
                language.translations["safety_first"] = "Safety First";
                language.translations["instructions"] = "Instructions";
                language.translations["results"] = "Results";
                language.translations["settings"] = "Settings";
                break;
                
            case "es":
                language.translations["welcome"] = "Bienvenido al Laboratorio Virtual de Química";
                language.translations["start_experiment"] = "Comenzar Experimento";
                language.translations["safety_first"] = "Seguridad Primero";
                language.translations["instructions"] = "Instrucciones";
                language.translations["results"] = "Resultados";
                language.translations["settings"] = "Configuración";
                break;
                
            case "fr":
                language.translations["welcome"] = "Bienvenue au Laboratoire de Chimie Virtuel";
                language.translations["start_experiment"] = "Commencer l'Expérience";
                language.translations["safety_first"] = "Sécurité d'Abord";
                language.translations["instructions"] = "Instructions";
                language.translations["results"] = "Résultats";
                language.translations["settings"] = "Paramètres";
                break;
                
            case "de":
                language.translations["welcome"] = "Willkommen im Virtuellen Chemielabor";
                language.translations["start_experiment"] = "Experiment Starten";
                language.translations["safety_first"] = "Sicherheit Zuerst";
                language.translations["instructions"] = "Anweisungen";
                language.translations["results"] = "Ergebnisse";
                language.translations["settings"] = "Einstellungen";
                break;
                
            case "zh":
                language.translations["welcome"] = "欢迎来到虚拟化学实验室";
                language.translations["start_experiment"] = "开始实验";
                language.translations["safety_first"] = "安全第一";
                language.translations["instructions"] = "说明";
                language.translations["results"] = "结果";
                language.translations["settings"] = "设置";
                break;
                
            case "ja":
                language.translations["welcome"] = "仮想化学実験室へようこそ";
                language.translations["start_experiment"] = "実験開始";
                language.translations["safety_first"] = "安全第一";
                language.translations["instructions"] = "説明";
                language.translations["results"] = "結果";
                language.translations["settings"] = "設定";
                break;
                
            case "ar":
                language.translations["welcome"] = "مرحباً بك في مختبر الكيمياء الافتراضي";
                language.translations["start_experiment"] = "ابدأ التجربة";
                language.translations["safety_first"] = "السلامة أولاً";
                language.translations["instructions"] = "التعليمات";
                language.translations["results"] = "النتائج";
                language.translations["settings"] = "الإعدادات";
                break;
                
            case "hi":
                language.translations["welcome"] = "आभासी रसायन प्रयोगशाला में आपका स्वागत है";
                language.translations["start_experiment"] = "प्रयोग शुरू करें";
                language.translations["safety_first"] = "सुरक्षा पहले";
                language.translations["instructions"] = "निर्देश";
                language.translations["results"] = "परिणाम";
                language.translations["settings"] = "सेटिंग्स";
                break;
        }
    }
    
    /// <summary>
    /// Detects the system language and sets it as current.
    /// </summary>
    private void DetectSystemLanguage()
    {
        if (!enableAutoLanguageDetection)
            return;
        
        string systemLanguage = Application.systemLanguage.ToString().ToLower();
        
        // Map system language to supported language codes
        string detectedLanguage = "en"; // Default
        
        if (systemLanguage.Contains("spanish")) detectedLanguage = "es";
        else if (systemLanguage.Contains("french")) detectedLanguage = "fr";
        else if (systemLanguage.Contains("german")) detectedLanguage = "de";
        else if (systemLanguage.Contains("chinese")) detectedLanguage = "zh";
        else if (systemLanguage.Contains("japanese")) detectedLanguage = "ja";
        else if (systemLanguage.Contains("arabic")) detectedLanguage = "ar";
        else if (systemLanguage.Contains("hindi")) detectedLanguage = "hi";
        
        SetLanguage(detectedLanguage);
        
        if (enableDebugLogging)
        {
            Debug.Log($"System language detected: {systemLanguage} -> {detectedLanguage}");
        }
    }
    
    /// <summary>
    /// Sets up accessibility features.
    /// </summary>
    private void SetupAccessibility()
    {
        // Load accessibility settings from player prefs
        accessibilitySettings.screenReaderEnabled = PlayerPrefs.GetInt("Accessibility_ScreenReader", 0) == 1;
        accessibilitySettings.highContrastEnabled = PlayerPrefs.GetInt("Accessibility_HighContrast", 0) == 1;
        accessibilitySettings.largeTextEnabled = PlayerPrefs.GetInt("Accessibility_LargeText", 0) == 1;
        accessibilitySettings.colorBlindSupportEnabled = PlayerPrefs.GetInt("Accessibility_ColorBlind", 0) == 1;
        accessibilitySettings.keyboardNavigationEnabled = PlayerPrefs.GetInt("Accessibility_Keyboard", 0) == 1;
        accessibilitySettings.textScale = PlayerPrefs.GetFloat("Accessibility_TextScale", 1.0f);
        accessibilitySettings.colorScheme = PlayerPrefs.GetString("Accessibility_ColorScheme", "default");
        
        ApplyAccessibilitySettings();
        
        if (enableDebugLogging)
        {
            Debug.Log("Accessibility settings loaded and applied");
        }
    }
    
    /// <summary>
    /// Applies accessibility settings to the UI.
    /// </summary>
    private void ApplyAccessibilitySettings()
    {
        // Apply text scaling
        if (accessibilitySettings.largeTextEnabled)
        {
            // Scale UI text elements
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.scaleFactor = accessibilitySettings.textScale;
                }
            }
        }
        
        // Apply high contrast
        if (accessibilitySettings.highContrastEnabled)
        {
            // Apply high contrast color scheme
            ApplyHighContrastColors();
        }
        
        // Apply color blind support
        if (accessibilitySettings.colorBlindSupportEnabled)
        {
            // Apply color blind friendly colors
            ApplyColorBlindColors();
        }
        
        OnAccessibilityChanged?.Invoke("Settings applied");
    }
    
    /// <summary>
    /// Applies high contrast colors.
    /// </summary>
    private void ApplyHighContrastColors()
    {
        // This would apply high contrast color scheme to UI elements
        if (enableDebugLogging)
        {
            Debug.Log("High contrast colors applied");
        }
    }
    
    /// <summary>
    /// Applies color blind friendly colors.
    /// </summary>
    private void ApplyColorBlindColors()
    {
        // This would apply color blind friendly color scheme
        if (enableDebugLogging)
        {
            Debug.Log("Color blind friendly colors applied");
        }
    }
    
    /// <summary>
    /// Sets the current language.
    /// </summary>
    public void SetLanguage(string languageCode)
    {
        if (!supportedLanguages.ContainsKey(languageCode))
        {
            Debug.LogWarning($"Language {languageCode} not supported. Using default.");
            languageCode = defaultLanguage;
        }
        
        if (currentLanguage != languageCode)
        {
            currentLanguage = languageCode;
            
            // Apply language-specific settings
            ApplyLanguageSettings(languageCode);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Language changed to: {languageCode}");
            }
            
            OnLanguageChanged?.Invoke(languageCode);
        }
    }
    
    /// <summary>
    /// Applies language-specific settings.
    /// </summary>
    private void ApplyLanguageSettings(string languageCode)
    {
        LanguageData language = supportedLanguages[languageCode];
        
        // Apply font family
        ApplyFontFamily(language.fontFamily);
        
        // Apply RTL if needed
        if (language.isRTL)
        {
            ApplyRTLSupport();
        }
        
        // Update UI text elements
        UpdateUIText();
    }
    
    /// <summary>
    /// Applies font family to UI elements.
    /// </summary>
    private void ApplyFontFamily(string fontFamily)
    {
        // This would apply the font family to all text elements
        if (enableDebugLogging)
        {
            Debug.Log($"Font family applied: {fontFamily}");
        }
    }
    
    /// <summary>
    /// Applies RTL (Right-to-Left) support.
    /// </summary>
    private void ApplyRTLSupport()
    {
        // This would apply RTL layout to UI elements
        if (enableDebugLogging)
        {
            Debug.Log("RTL support applied");
        }
    }
    
    /// <summary>
    /// Updates UI text elements with current language.
    /// </summary>
    private void UpdateUIText()
    {
        // This would update all UI text elements with translations
        if (enableDebugLogging)
        {
            Debug.Log("UI text updated with current language");
        }
    }
    
    /// <summary>
    /// Sets the current culture.
    /// </summary>
    public void SetCulture(string cultureCode)
    {
        if (!culturalData.ContainsKey(cultureCode))
        {
            Debug.LogWarning($"Culture {cultureCode} not supported. Using default.");
            cultureCode = "en-US";
        }
        
        currentCulture = culturalData[cultureCode];
        
        // Apply cultural settings
        ApplyCulturalSettings(cultureCode);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Culture changed to: {cultureCode}");
        }
        
        OnCultureChanged?.Invoke(cultureCode);
    }
    
    /// <summary>
    /// Applies cultural settings.
    /// </summary>
    private void ApplyCulturalSettings(string cultureCode)
    {
        CulturalData culture = culturalData[cultureCode];
        
        // Apply date format
        // Apply time format
        // Apply number format
        // Apply currency format
        
        if (enableDebugLogging)
        {
            Debug.Log($"Cultural settings applied for {cultureCode}");
        }
    }
    
    /// <summary>
    /// Sets the regional curriculum.
    /// </summary>
    public void SetRegionalCurriculum(string regionCode)
    {
        if (!regionalCurricula.ContainsKey(regionCode))
        {
            Debug.LogWarning($"Regional curriculum {regionCode} not supported. Using default.");
            regionCode = "en-US";
        }
        
        currentCurriculum = regionalCurricula[regionCode];
        
        if (enableDebugLogging)
        {
            Debug.Log($"Regional curriculum set to: {regionCode}");
        }
    }
    
    /// <summary>
    /// Translates text to current language.
    /// </summary>
    public string Translate(string key)
    {
        if (!supportedLanguages.ContainsKey(currentLanguage))
            return key;
        
        LanguageData language = supportedLanguages[currentLanguage];
        
        if (language.translations.ContainsKey(key))
        {
            return language.translations[key];
        }
        
        // Fallback to default language
        if (supportedLanguages.ContainsKey(defaultLanguage))
        {
            LanguageData defaultLang = supportedLanguages[defaultLanguage];
            if (defaultLang.translations.ContainsKey(key))
            {
                return defaultLang.translations[key];
            }
        }
        
        return key;
    }
    
    /// <summary>
    /// Formats date according to current culture.
    /// </summary>
    public string FormatDate(DateTime date)
    {
        if (currentCulture == null)
            return date.ToString("MM/dd/yyyy");
        
        return date.ToString(currentCulture.dateFormat);
    }
    
    /// <summary>
    /// Formats time according to current culture.
    /// </summary>
    public string FormatTime(DateTime time)
    {
        if (currentCulture == null)
            return time.ToString("HH:mm");
        
        return time.ToString(currentCulture.timeFormat);
    }
    
    /// <summary>
    /// Formats number according to current culture.
    /// </summary>
    public string FormatNumber(float number)
    {
        if (currentCulture == null)
            return number.ToString("0.00");
        
        return number.ToString(currentCulture.numberFormat);
    }
    
    /// <summary>
    /// Handles internationalization input.
    /// </summary>
    private void HandleI18nInput()
    {
        // Handle language switching
        if (Input.GetKeyDown(KeyCode.L))
        {
            // Cycle through languages
            CycleLanguage();
        }
        
        // Handle accessibility toggles
        if (Input.GetKeyDown(KeyCode.A))
        {
            ToggleAccessibility();
        }
    }
    
    /// <summary>
    /// Cycles through available languages.
    /// </summary>
    private void CycleLanguage()
    {
        List<string> availableLanguages = new List<string>(supportedLanguages.Keys);
        int currentIndex = availableLanguages.IndexOf(currentLanguage);
        int nextIndex = (currentIndex + 1) % availableLanguages.Count;
        
        SetLanguage(availableLanguages[nextIndex]);
    }
    
    /// <summary>
    /// Toggles accessibility features.
    /// </summary>
    private void ToggleAccessibility()
    {
        accessibilitySettings.largeTextEnabled = !accessibilitySettings.largeTextEnabled;
        accessibilitySettings.highContrastEnabled = !accessibilitySettings.highContrastEnabled;
        
        ApplyAccessibilitySettings();
        
        // Save settings
        PlayerPrefs.SetInt("Accessibility_LargeText", accessibilitySettings.largeTextEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Accessibility_HighContrast", accessibilitySettings.highContrastEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Gets the current language.
    /// </summary>
    public string GetCurrentLanguage()
    {
        return currentLanguage;
    }
    
    /// <summary>
    /// Gets the current culture.
    /// </summary>
    public CulturalData GetCurrentCulture()
    {
        return currentCulture;
    }
    
    /// <summary>
    /// Gets the current curriculum.
    /// </summary>
    public RegionalCurriculum GetCurrentCurriculum()
    {
        return currentCurriculum;
    }
    
    /// <summary>
    /// Gets accessibility settings.
    /// </summary>
    public AccessibilitySettings GetAccessibilitySettings()
    {
        return accessibilitySettings;
    }
    
    /// <summary>
    /// Gets supported languages.
    /// </summary>
    public List<string> GetSupportedLanguages()
    {
        return new List<string>(supportedLanguages.Keys);
    }
    
    /// <summary>
    /// Gets supported cultures.
    /// </summary>
    public List<string> GetSupportedCultures()
    {
        return new List<string>(culturalData.Keys);
    }
    
    /// <summary>
    /// Checks if language is supported.
    /// </summary>
    public bool IsLanguageSupported(string languageCode)
    {
        return supportedLanguages.ContainsKey(languageCode);
    }
    
    /// <summary>
    /// Checks if culture is supported.
    /// </summary>
    public bool IsCultureSupported(string cultureCode)
    {
        return culturalData.ContainsKey(cultureCode);
    }
    
    /// <summary>
    /// Logs internationalization status for debugging.
    /// </summary>
    public void LogInternationalizationStatus()
    {
        if (!enableDebugLogging)
            return;
        
        Debug.Log("=== Internationalization Status ===");
        Debug.Log($"Current Language: {currentLanguage}");
        Debug.Log($"Supported Languages: {supportedLanguages.Count}");
        Debug.Log($"Supported Cultures: {culturalData.Count}");
        Debug.Log($"Regional Curricula: {regionalCurricula.Count}");
        Debug.Log($"Accessibility Enabled: {accessibilitySettings.largeTextEnabled}");
        Debug.Log("===================================");
    }
} 