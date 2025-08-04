using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages automated testing, validation, and quality assurance for the virtual chemistry lab.
/// This component handles all testing-related operations and validation procedures.
/// </summary>
public class TestingManager : MonoBehaviour
{
    [Header("Testing Management")]
    [SerializeField] private bool enableTestingManagement = true;
    [SerializeField] private bool enableAutomatedTesting = true;
    [SerializeField] private bool enableUnitTesting = true;
    [SerializeField] private bool enableIntegrationTesting = true;
    [SerializeField] private bool enablePerformanceTesting = true;
    
    [Header("Test Configuration")]
    [SerializeField] private TestSuite[] availableTestSuites;
    [SerializeField] private string testResultSound = "test_complete";
    [SerializeField] private string testFailSound = "test_fail";
    [SerializeField] private string testPassSound = "test_pass";
    
    [Header("Test State")]
    [SerializeField] private Dictionary<string, TestInstance> activeTests = new Dictionary<string, TestInstance>();
    [SerializeField] private List<TestResult> testResults = new List<TestResult>();
    [SerializeField] private bool isTestingInProgress = false;
    
    [Header("Testing Settings")]
    [SerializeField] private bool enableContinuousTesting = false;
    [SerializeField] private bool enableTestLogging = true;
    [SerializeField] private bool enableTestReporting = true;
    [SerializeField] private float testTimeout = 30f;
    [SerializeField] private int maxConcurrentTests = 5;
    [SerializeField] private bool enableTestRetry = true;
    [SerializeField] private int maxRetryAttempts = 3;
    
    [Header("Validation Settings")]
    [SerializeField] private bool enableDataValidation = true;
    [SerializeField] private bool enableComponentValidation = true;
    [SerializeField] private bool enablePerformanceValidation = true;
    [SerializeField] private bool enableSafetyValidation = true;
    [SerializeField] private float validationThreshold = 0.95f;
    [SerializeField] private float performanceThreshold = 60f; // FPS
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logTestEvents = false;
    
    private static TestingManager instance;
    public static TestingManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TestingManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("TestingManager");
                    instance = go.AddComponent<TestingManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnTestStarted;
    public event Action<string> OnTestCompleted;
    public event Action<string> OnTestFailed;
    public event Action<TestResult> OnTestResult;
    public event Action<string> OnValidationPassed;
    public event Action<string> OnValidationFailed;
    public event Action<string> OnTestError;
    
    // Private variables
    private Dictionary<string, TestSuite> testSuiteDatabase = new Dictionary<string, TestSuite>();
    private bool isInitialized = false;
    
    [System.Serializable]
    public class TestSuite
    {
        public string id;
        public string name;
        public string description;
        public TestType type;
        public string[] testCases;
        public string[] dependencies;
        public float estimatedDuration;
        public bool isRequired;
        public TestPriority priority;
        public string[] targetComponents;
    }
    
    [System.Serializable]
    public class TestInstance
    {
        public string id;
        public string testSuiteId;
        public string name;
        public bool isActive;
        public bool isInProgress;
        public bool isCompleted;
        public float startTime;
        public float completionTime;
        public TestStatus status;
        public List<TestCaseResult> testCaseResults;
        public int passedTests;
        public int failedTests;
        public int totalTests;
    }
    
    [System.Serializable]
    public class TestResult
    {
        public string id;
        public string testSuiteId;
        public bool isSuccessful;
        public float executionTime;
        public int passedTests;
        public int failedTests;
        public int totalTests;
        public float successRate;
        public string errorMessage;
        public List<TestCaseResult> testCaseResults;
    }
    
    [System.Serializable]
    public class TestCaseResult
    {
        public string testCaseId;
        public string testCaseName;
        public bool isPassed;
        public float executionTime;
        public string errorMessage;
        public object expectedResult;
        public object actualResult;
        public TestCaseStatus status;
    }
    
    [System.Serializable]
    public enum TestType
    {
        Unit,
        Integration,
        Performance,
        Safety,
        Regression,
        Smoke,
        Acceptance,
        Stress
    }
    
    [System.Serializable]
    public enum TestStatus
    {
        Setup,
        Running,
        Completed,
        Failed,
        Timeout,
        Cancelled
    }
    
    [System.Serializable]
    public enum TestPriority
    {
        Critical,
        High,
        Normal,
        Low,
        Optional
    }
    
    [System.Serializable]
    public enum TestCaseStatus
    {
        NotStarted,
        Running,
        Passed,
        Failed,
        Skipped,
        Timeout
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTestingManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadTestSuiteDatabase();
    }
    
    private void Update()
    {
        UpdateTestProgress();
    }
    
    /// <summary>
    /// Initializes the testing manager.
    /// </summary>
    private void InitializeTestingManager()
    {
        activeTests.Clear();
        testResults.Clear();
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("TestingManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads test suite data from available test suites.
    /// </summary>
    private void LoadTestSuiteDatabase()
    {
        testSuiteDatabase.Clear();
        
        foreach (TestSuite testSuite in availableTestSuites)
        {
            if (testSuite != null && !string.IsNullOrEmpty(testSuite.id))
            {
                testSuiteDatabase[testSuite.id] = testSuite;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Loaded test suite: {testSuite.name} ({testSuite.id})");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {testSuiteDatabase.Count} test suites");
        }
    }
    
    /// <summary>
    /// Creates a new test instance.
    /// </summary>
    public TestInstance CreateTest(string testSuiteId)
    {
        if (!enableTestingManagement || !testSuiteDatabase.ContainsKey(testSuiteId))
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Test suite not found: {testSuiteId}");
            }
            return null;
        }
        
        TestSuite testSuite = testSuiteDatabase[testSuiteId];
        TestInstance instance = new TestInstance
        {
            id = GenerateTestId(),
            testSuiteId = testSuiteId,
            name = testSuite.name,
            isActive = true,
            isInProgress = false,
            isCompleted = false,
            startTime = 0f,
            completionTime = 0f,
            status = TestStatus.Setup,
            testCaseResults = new List<TestCaseResult>(),
            passedTests = 0,
            failedTests = 0,
            totalTests = testSuite.testCases.Length
        };
        
        activeTests[instance.id] = instance;
        
        OnTestStarted?.Invoke(instance.id);
        
        if (logTestEvents)
        {
            Debug.Log($"Created test: {testSuite.name} ({instance.id})");
        }
        
        return instance;
    }
    
    /// <summary>
    /// Starts a test.
    /// </summary>
    public bool StartTest(string instanceId)
    {
        if (!enableAutomatedTesting || !activeTests.ContainsKey(instanceId)) return false;
        
        TestInstance instance = activeTests[instanceId];
        
        if (instance.isInProgress || instance.isCompleted)
        {
            OnTestError?.Invoke($"Test {instance.name} is already in progress or completed");
            return false;
        }
        
        instance.isInProgress = true;
        instance.startTime = Time.time;
        instance.status = TestStatus.Running;
        
        isTestingInProgress = true;
        
        if (logTestEvents)
        {
            Debug.Log($"Started test: {instance.name} ({instance.id})");
        }
        
        return true;
    }
    
    /// <summary>
    /// Completes a test.
    /// </summary>
    public void CompleteTest(string instanceId)
    {
        if (!activeTests.ContainsKey(instanceId)) return;
        
        TestInstance instance = activeTests[instanceId];
        
        instance.isInProgress = false;
        instance.isCompleted = true;
        instance.completionTime = Time.time;
        instance.status = TestStatus.Completed;
        
        TestResult result = CalculateTestResult(instance);
        testResults.Add(result);
        
        isTestingInProgress = false;
        
        OnTestCompleted?.Invoke(instance.id);
        OnTestResult?.Invoke(result);
        
        if (logTestEvents)
        {
            Debug.Log($"Completed test: {instance.name} ({instance.id}) - Success: {result.isSuccessful}");
        }
    }
    
    /// <summary>
    /// Calculates test result.
    /// </summary>
    private TestResult CalculateTestResult(TestInstance instance)
    {
        TestResult result = new TestResult
        {
            id = GenerateResultId(),
            testSuiteId = instance.testSuiteId,
            isSuccessful = instance.failedTests == 0,
            executionTime = instance.completionTime - instance.startTime,
            passedTests = instance.passedTests,
            failedTests = instance.failedTests,
            totalTests = instance.totalTests,
            successRate = instance.totalTests > 0 ? (float)instance.passedTests / instance.totalTests : 0f,
            testCaseResults = new List<TestCaseResult>(instance.testCaseResults)
        };
        
        if (!result.isSuccessful)
        {
            result.errorMessage = $"{instance.failedTests} test cases failed";
        }
        
        return result;
    }
    
    /// <summary>
    /// Updates test progress.
    /// </summary>
    private void UpdateTestProgress()
    {
        if (!enableTestingManagement) return;
        
        foreach (var kvp in activeTests)
        {
            TestInstance instance = kvp.Value;
            
            if (instance.isInProgress && !instance.isCompleted)
            {
                if (Time.time - instance.startTime > testTimeout)
                {
                    instance.status = TestStatus.Timeout;
                    instance.isInProgress = false;
                    instance.isCompleted = true;
                    instance.completionTime = Time.time;
                    
                    OnTestFailed?.Invoke(instance.id);
                    
                    if (logTestEvents)
                    {
                        Debug.LogWarning($"Test {instance.name} timed out");
                    }
                }
            }
        }
        
        isTestingInProgress = false;
        foreach (var kvp in activeTests)
        {
            if (kvp.Value.isInProgress)
            {
                isTestingInProgress = true;
                break;
            }
        }
    }
    
    /// <summary>
    /// Runs validation tests.
    /// </summary>
    public bool RunValidation(string validationType)
    {
        switch (validationType)
        {
            case "Data":
                return enableDataValidation && ValidateData();
            case "Component":
                return enableComponentValidation && ValidateComponents();
            case "Performance":
                return enablePerformanceValidation && ValidatePerformance();
            case "Safety":
                return enableSafetyValidation && ValidateSafety();
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Validates data integrity.
    /// </summary>
    private bool ValidateData()
    {
        bool isValid = DataManager.Instance != null && SettingsManager.Instance != null;
        
        if (isValid)
        {
            OnValidationPassed?.Invoke("Data validation passed");
        }
        else
        {
            OnValidationFailed?.Invoke("Data validation failed");
        }
        
        return isValid;
    }
    
    /// <summary>
    /// Validates component integrity.
    /// </summary>
    private bool ValidateComponents()
    {
        bool isValid = GameManager.Instance != null && 
                      ExperimentStateManager.Instance != null && 
                      DataManager.Instance != null;
        
        if (isValid)
        {
            OnValidationPassed?.Invoke("Component validation passed");
        }
        else
        {
            OnValidationFailed?.Invoke("Component validation failed");
        }
        
        return isValid;
    }
    
    /// <summary>
    /// Validates performance.
    /// </summary>
    private bool ValidatePerformance()
    {
        float currentFPS = 1f / Time.deltaTime;
        bool isValid = currentFPS >= performanceThreshold;
        
        if (isValid)
        {
            OnValidationPassed?.Invoke("Performance validation passed");
        }
        else
        {
            OnValidationFailed?.Invoke($"Performance validation failed - FPS: {currentFPS:F1}");
        }
        
        return isValid;
    }
    
    /// <summary>
    /// Validates safety systems.
    /// </summary>
    private bool ValidateSafety()
    {
        bool isValid = SafetyManager.Instance != null && ChemicalManager.Instance != null;
        
        if (isValid)
        {
            OnValidationPassed?.Invoke("Safety validation passed");
        }
        else
        {
            OnValidationFailed?.Invoke("Safety validation failed");
        }
        
        return isValid;
    }
    
    /// <summary>
    /// Generates a unique test instance ID.
    /// </summary>
    private string GenerateTestId()
    {
        return $"test_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    /// <summary>
    /// Generates a unique result ID.
    /// </summary>
    private string GenerateResultId()
    {
        return $"res_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    // Public getters and setters
    public bool IsTestingInProgress() => isTestingInProgress;
    public int GetActiveTestCount() => activeTests.Count;
    public int GetTestResultCount() => testResults.Count;
    
    /// <summary>
    /// Gets a test instance by ID.
    /// </summary>
    public TestInstance GetTest(string instanceId)
    {
        return activeTests.ContainsKey(instanceId) ? activeTests[instanceId] : null;
    }
    
    /// <summary>
    /// Gets test suite data by ID.
    /// </summary>
    public TestSuite GetTestSuite(string testSuiteId)
    {
        return testSuiteDatabase.ContainsKey(testSuiteId) ? testSuiteDatabase[testSuiteId] : null;
    }
    
    /// <summary>
    /// Gets all available test suite IDs.
    /// </summary>
    public List<string> GetAvailableTestSuiteIds()
    {
        return new List<string>(testSuiteDatabase.Keys);
    }
    
    /// <summary>
    /// Gets test results.
    /// </summary>
    public List<TestResult> GetTestResults()
    {
        return new List<TestResult>(testResults);
    }
    
    /// <summary>
    /// Gets successful test results.
    /// </summary>
    public List<TestResult> GetSuccessfulResults()
    {
        return testResults.FindAll(r => r.isSuccessful);
    }
    
    /// <summary>
    /// Sets the test timeout.
    /// </summary>
    public void SetTestTimeout(float timeout)
    {
        testTimeout = Mathf.Clamp(timeout, 1f, 300f);
    }
    
    /// <summary>
    /// Sets the performance threshold.
    /// </summary>
    public void SetPerformanceThreshold(float threshold)
    {
        performanceThreshold = Mathf.Clamp(threshold, 10f, 120f);
    }
    
    /// <summary>
    /// Enables or disables automated testing.
    /// </summary>
    public void SetAutomatedTestingEnabled(bool enabled)
    {
        enableAutomatedTesting = enabled;
    }
    
    /// <summary>
    /// Generates a test report.
    /// </summary>
    public string GenerateTestReport()
    {
        if (!enableTestReporting) return "";
        
        string report = "=== Test Report ===\n";
        report += $"Total Tests: {testResults.Count}\n";
        report += $"Successful: {GetSuccessfulResults().Count}\n";
        report += $"Failed: {testResults.Count - GetSuccessfulResults().Count}\n";
        report += $"Success Rate: {(testResults.Count > 0 ? (float)GetSuccessfulResults().Count / testResults.Count * 100f : 0f):F1}%\n";
        report += "==================\n";
        
        return report;
    }
    
    /// <summary>
    /// Logs the current testing manager status.
    /// </summary>
    public void LogTestingStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Testing Manager Status ===");
        Debug.Log($"Active Tests: {activeTests.Count}");
        Debug.Log($"Test Results: {testResults.Count}");
        Debug.Log($"Is Testing In Progress: {isTestingInProgress}");
        Debug.Log($"Test Suite Database Size: {testSuiteDatabase.Count}");
        Debug.Log($"Automated Testing: {(enableAutomatedTesting ? "Enabled" : "Disabled")}");
        Debug.Log($"Unit Testing: {(enableUnitTesting ? "Enabled" : "Disabled")}");
        Debug.Log($"Integration Testing: {(enableIntegrationTesting ? "Enabled" : "Disabled")}");
        Debug.Log($"Performance Testing: {(enablePerformanceTesting ? "Enabled" : "Disabled")}");
        Debug.Log("=============================");
    }
} 