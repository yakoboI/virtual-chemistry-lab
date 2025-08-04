using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Controls the behavior of a burette apparatus for volumetric analysis.
/// Handles filling, dispensing, and volume measurements.
/// </summary>
public class BuretteController : MonoBehaviour
{
    [Header("Burette Properties")]
    [SerializeField] private float capacity = 50f; // mL
    [SerializeField] private float precision = 0.05f; // mL
    [SerializeField] private float currentVolume = 0f;
    [SerializeField] private Chemical containedChemical;
    
    [Header("Visual Settings")]
    [SerializeField] private Material glassMaterial;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] private Transform liquidLevel;
    [SerializeField] private Transform meniscus;
    
    [Header("Audio")]
    [SerializeField] private AudioClip fillSound;
    [SerializeField] private AudioClip dispenseSound;
    [SerializeField] private AudioClip dropSound;
    
    [Header("Interaction")]
    [SerializeField] private bool isInteractable = true;
    [SerializeField] private float dispenseRate = 1f; // mL per second
    [SerializeField] private bool isDispensing = false;
    
    // Events
    public static event Action<BuretteController, Chemical> OnBuretteFilled;
    public static event Action<BuretteController, float> OnVolumeChanged;
    public static event Action<BuretteController, float> OnDispenseComplete;
    
    // Properties
    public float Capacity => capacity;
    public float Precision => precision;
    public float CurrentVolume => currentVolume;
    public Chemical ContainedChemical => containedChemical;
    public bool IsEmpty => currentVolume <= 0f;
    public bool IsFull => currentVolume >= capacity;
    public bool IsDispensing => isDispensing;
    
    private float dispenseStartTime;
    private float targetDispenseVolume;
    private Vector3 originalLiquidLevelPosition;
    
    private void Awake()
    {
        InitializeBurette();
    }
    
    private void Start()
    {
        UpdateVisualRepresentation();
    }
    
    private void Update()
    {
        HandleDispensing();
    }
    
    /// <summary>
    /// Initializes the burette with default settings.
    /// </summary>
    private void InitializeBurette()
    {
        // Store original liquid level position
        if (liquidLevel != null)
        {
            originalLiquidLevelPosition = liquidLevel.localPosition;
        }
        
        // Set up materials
        if (glassMaterial == null)
        {
            glassMaterial = new Material(Shader.Find("Standard"));
            glassMaterial.SetFloat("_Mode", 3); // Transparent mode
            glassMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            glassMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            glassMaterial.SetInt("_ZWrite", 0);
            glassMaterial.DisableKeyword("_ALPHATEST_ON");
            glassMaterial.EnableKeyword("_ALPHABLEND_ON");
            glassMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            glassMaterial.renderQueue = 3000;
        }
        
        if (liquidMaterial == null)
        {
            liquidMaterial = new Material(Shader.Find("Standard"));
            liquidMaterial.SetFloat("_Mode", 3); // Transparent mode
            liquidMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            liquidMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            liquidMaterial.SetInt("_ZWrite", 0);
            liquidMaterial.DisableKeyword("_ALPHATEST_ON");
            liquidMaterial.EnableKeyword("_ALPHABLEND_ON");
            liquidMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            liquidMaterial.renderQueue = 3000;
        }
    }
    
    /// <summary>
    /// Fills the burette with a specific chemical and volume.
    /// </summary>
    public bool FillBurette(Chemical chemical, float volume)
    {
        if (chemical == null || volume <= 0f)
        {
            Debug.LogWarning("Invalid chemical or volume for burette filling");
            return false;
        }
        
        if (volume > capacity)
        {
            Debug.LogWarning($"Volume {volume} mL exceeds burette capacity {capacity} mL");
            return false;
        }
        
        // Check if burette already contains a different chemical
        if (containedChemical != null && containedChemical != chemical)
        {
            Debug.LogWarning("Burette already contains a different chemical");
            return false;
        }
        
        containedChemical = chemical;
        currentVolume = volume;
        
        UpdateVisualRepresentation();
        
        // Play fill sound
        if (fillSound != null)
        {
            AudioSource.PlayClipAtPoint(fillSound, transform.position);
        }
        
        OnBuretteFilled?.Invoke(this, chemical);
        OnVolumeChanged?.Invoke(this, currentVolume);
        
        Debug.Log($"Burette filled with {chemical.ChemicalName}: {volume} mL");
        return true;
    }
    
    /// <summary>
    /// Empties the burette completely.
    /// </summary>
    public void EmptyBurette()
    {
        if (currentVolume <= 0f) return;
        
        float emptiedVolume = currentVolume;
        currentVolume = 0f;
        containedChemical = null;
        
        UpdateVisualRepresentation();
        
        OnVolumeChanged?.Invoke(this, currentVolume);
        
        Debug.Log($"Burette emptied: {emptiedVolume} mL");
    }
    
    /// <summary>
    /// Starts dispensing liquid from the burette.
    /// </summary>
    public void StartDispensing(float volume)
    {
        if (IsEmpty || volume <= 0f || volume > currentVolume)
        {
            Debug.LogWarning("Cannot dispense: invalid volume or empty burette");
            return;
        }
        
        isDispensing = true;
        dispenseStartTime = Time.time;
        targetDispenseVolume = volume;
        
        Debug.Log($"Started dispensing {volume} mL from burette");
    }
    
    /// <summary>
    /// Stops dispensing liquid from the burette.
    /// </summary>
    public void StopDispensing()
    {
        if (!isDispensing) return;
        
        isDispensing = false;
        float dispensedVolume = targetDispenseVolume;
        
        // Remove dispensed volume
        currentVolume -= dispensedVolume;
        currentVolume = Mathf.Max(0f, currentVolume);
        
        UpdateVisualRepresentation();
        
        OnVolumeChanged?.Invoke(this, currentVolume);
        OnDispenseComplete?.Invoke(this, dispensedVolume);
        
        // Play dispense sound
        if (dispenseSound != null)
        {
            AudioSource.PlayClipAtPoint(dispenseSound, transform.position);
        }
        
        Debug.Log($"Stopped dispensing: {dispensedVolume} mL dispensed");
    }
    
    /// <summary>
    /// Dispenses a single drop from the burette.
    /// </summary>
    public void DispenseDrop()
    {
        if (IsEmpty) return;
        
        float dropVolume = 0.05f; // Standard drop volume
        if (currentVolume < dropVolume)
        {
            dropVolume = currentVolume;
        }
        
        currentVolume -= dropVolume;
        currentVolume = Mathf.Max(0f, currentVolume);
        
        UpdateVisualRepresentation();
        
        OnVolumeChanged?.Invoke(this, currentVolume);
        
        // Play drop sound
        if (dropSound != null)
        {
            AudioSource.PlayClipAtPoint(dropSound, transform.position);
        }
        
        Debug.Log($"Dispensed drop: {dropVolume} mL");
    }
    
    /// <summary>
    /// Handles the dispensing process over time.
    /// </summary>
    private void HandleDispensing()
    {
        if (!isDispensing) return;
        
        float elapsedTime = Time.time - dispenseStartTime;
        float dispensedSoFar = elapsedTime * dispenseRate;
        
        if (dispensedSoFar >= targetDispenseVolume)
        {
            StopDispensing();
        }
        else
        {
            // Update visual representation during dispensing
            UpdateLiquidLevel(currentVolume - dispensedSoFar);
        }
    }
    
    /// <summary>
    /// Updates the visual representation of the burette.
    /// </summary>
    private void UpdateVisualRepresentation()
    {
        UpdateLiquidLevel(currentVolume);
        UpdateLiquidColor();
        UpdateMeniscus();
    }
    
    /// <summary>
    /// Updates the liquid level based on current volume.
    /// </summary>
    private void UpdateLiquidLevel(float volume)
    {
        if (liquidLevel == null) return;
        
        float fillRatio = volume / capacity;
        float maxHeight = 0.8f; // Maximum height of liquid in burette
        float currentHeight = maxHeight * fillRatio;
        
        Vector3 newPosition = originalLiquidLevelPosition;
        newPosition.y = originalLiquidLevelPosition.y + currentHeight;
        liquidLevel.localPosition = newPosition;
        
        // Scale the liquid level based on volume
        Vector3 scale = liquidLevel.localScale;
        scale.y = fillRatio;
        liquidLevel.localScale = scale;
    }
    
    /// <summary>
    /// Updates the color of the liquid based on the contained chemical.
    /// </summary>
    private void UpdateLiquidColor()
    {
        if (liquidMaterial == null || liquidLevel == null) return;
        
        if (containedChemical != null)
        {
            liquidMaterial.color = containedChemical.Color;
        }
        else
        {
            liquidMaterial.color = Color.clear;
        }
        
        // Apply material to liquid level
        Renderer liquidRenderer = liquidLevel.GetComponent<Renderer>();
        if (liquidRenderer != null)
        {
            liquidRenderer.material = liquidMaterial;
        }
    }
    
    /// <summary>
    /// Updates the meniscus (curved surface of liquid).
    /// </summary>
    private void UpdateMeniscus()
    {
        if (meniscus == null) return;
        
        // Show meniscus only when there's liquid
        meniscus.gameObject.SetActive(currentVolume > 0f);
        
        if (currentVolume > 0f)
        {
            // Position meniscus at the top of the liquid
            Vector3 meniscusPosition = liquidLevel.position;
            meniscusPosition.y = liquidLevel.position.y + liquidLevel.localScale.y * 0.5f;
            meniscus.position = meniscusPosition;
        }
    }
    
    /// <summary>
    /// Gets the current reading from the burette scale.
    /// </summary>
    public float GetBuretteReading()
    {
        // Convert volume to burette scale reading (usually inverted)
        return capacity - currentVolume;
    }
    
    /// <summary>
    /// Sets the burette reading (for calibration or manual adjustment).
    /// </summary>
    public void SetBuretteReading(float reading)
    {
        if (reading < 0f || reading > capacity)
        {
            Debug.LogWarning($"Invalid burette reading: {reading}");
            return;
        }
        
        currentVolume = capacity - reading;
        UpdateVisualRepresentation();
        
        OnVolumeChanged?.Invoke(this, currentVolume);
    }
    
    /// <summary>
    /// Checks if the burette contains a specific chemical.
    /// </summary>
    public bool ContainsChemical(string chemicalId)
    {
        return containedChemical != null && containedChemical.ChemicalId == chemicalId;
    }
    
    /// <summary>
    /// Configures the burette with apparatus data.
    /// </summary>
    public void Configure(ApparatusData apparatusData)
    {
        if (apparatusData == null) return;
        
        capacity = apparatusData.capacity;
        precision = apparatusData.precision;
        
        Debug.Log($"Burette configured: Capacity={capacity} mL, Precision={precision} mL");
    }
    
    /// <summary>
    /// Gets a summary of the burette's current state.
    /// </summary>
    public string GetBuretteSummary()
    {
        string summary = $"Burette Status:\n";
        summary += $"Capacity: {capacity} mL\n";
        summary += $"Current Volume: {currentVolume:F2} mL\n";
        summary += $"Reading: {GetBuretteReading():F2} mL\n";
        
        if (containedChemical != null)
        {
            summary += $"Chemical: {containedChemical.ChemicalName}\n";
            summary += $"Concentration: {containedChemical.Concentration:F3} mol/L\n";
        }
        else
        {
            summary += $"Chemical: Empty\n";
        }
        
        summary += $"Status: {(IsEmpty ? "Empty" : IsFull ? "Full" : "Partially Filled")}";
        
        return summary;
    }
    
    /// <summary>
    /// Handles interaction when the burette is clicked.
    /// </summary>
    private void OnMouseDown()
    {
        if (!isInteractable) return;
        
        // Toggle dispensing or show burette info
        if (Input.GetKey(KeyCode.LeftControl))
        {
            // Ctrl+Click to dispense drop
            DispenseDrop();
        }
        else
        {
            // Regular click to show info
            Debug.Log(GetBuretteSummary());
        }
    }
    
    /// <summary>
    /// Handles interaction when the burette is dragged.
    /// </summary>
    private void OnMouseDrag()
    {
        if (!isInteractable || IsEmpty) return;
        
        // Dragging can control dispensing rate
        float dragDelta = Input.GetAxis("Mouse Y");
        if (Mathf.Abs(dragDelta) > 0.01f)
        {
            // Adjust dispense rate based on drag
            dispenseRate = Mathf.Clamp(dispenseRate + dragDelta * 0.1f, 0.1f, 5f);
        }
    }
} 