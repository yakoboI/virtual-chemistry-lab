using UnityEngine;
using System;

/// <summary>
/// Controls the behavior of a pipette apparatus for precise volume transfers.
/// Handles filling, dispensing, and volume measurements.
/// </summary>
public class PipetteController : MonoBehaviour
{
    [Header("Pipette Properties")]
    [SerializeField] private float capacity = 25f; // mL
    [SerializeField] private float precision = 0.05f; // mL
    [SerializeField] private float currentVolume = 0f;
    [SerializeField] private Chemical containedChemical;
    
    [Header("Visual Settings")]
    [SerializeField] private Material glassMaterial;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] private Transform liquidLevel;
    [SerializeField] private Transform tip;
    
    [Header("Audio")]
    [SerializeField] private AudioClip fillSound;
    [SerializeField] private AudioClip dispenseSound;
    [SerializeField] private AudioClip suctionSound;
    
    [Header("Interaction")]
    [SerializeField] private bool isInteractable = true;
    [SerializeField] private bool isFilled = false;
    [SerializeField] private float lastTransferredVolume = 0f;
    
    // Events
    public static event Action<PipetteController, Chemical> OnPipetteFilled;
    public static event Action<PipetteController, float> OnVolumeChanged;
    public static event Action<PipetteController, float> OnTransferComplete;
    
    // Properties
    public float Capacity => capacity;
    public float Precision => precision;
    public float CurrentVolume => currentVolume;
    public Chemical ContainedChemical => containedChemical;
    public bool IsEmpty => currentVolume <= 0f;
    public bool IsFull => currentVolume >= capacity;
    public bool IsFilled => isFilled;
    public float LastTransferredVolume => lastTransferredVolume;
    
    private Vector3 originalLiquidLevelPosition;
    
    private void Awake()
    {
        InitializePipette();
    }
    
    private void Start()
    {
        UpdateVisualRepresentation();
    }
    
    /// <summary>
    /// Initializes the pipette with default settings.
    /// </summary>
    private void InitializePipette()
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
    /// Fills the pipette with a specific chemical and volume.
    /// </summary>
    public bool FillPipette(Chemical chemical, float volume)
    {
        if (chemical == null || volume <= 0f)
        {
            Debug.LogWarning("Invalid chemical or volume for pipette filling");
            return false;
        }
        
        if (volume > capacity)
        {
            Debug.LogWarning($"Volume {volume} mL exceeds pipette capacity {capacity} mL");
            return false;
        }
        
        // Check if pipette already contains a different chemical
        if (containedChemical != null && containedChemical != chemical)
        {
            Debug.LogWarning("Pipette already contains a different chemical");
            return false;
        }
        
        containedChemical = chemical;
        currentVolume = volume;
        isFilled = true;
        
        UpdateVisualRepresentation();
        
        // Play fill sound
        if (fillSound != null)
        {
            AudioSource.PlayClipAtPoint(fillSound, transform.position);
        }
        
        OnPipetteFilled?.Invoke(this, chemical);
        OnVolumeChanged?.Invoke(this, currentVolume);
        
        Debug.Log($"Pipette filled with {chemical.ChemicalName}: {volume} mL");
        return true;
    }
    
    /// <summary>
    /// Empties the pipette completely.
    /// </summary>
    public void EmptyPipette()
    {
        if (currentVolume <= 0f) return;
        
        float emptiedVolume = currentVolume;
        currentVolume = 0f;
        containedChemical = null;
        isFilled = false;
        
        UpdateVisualRepresentation();
        
        OnVolumeChanged?.Invoke(this, currentVolume);
        
        Debug.Log($"Pipette emptied: {emptiedVolume} mL");
    }
    
    /// <summary>
    /// Transfers the pipette contents to a target container.
    /// </summary>
    public bool TransferToContainer(GameObject targetContainer, float volume = -1f)
    {
        if (IsEmpty || containedChemical == null)
        {
            Debug.LogWarning("Cannot transfer: pipette is empty");
            return false;
        }
        
        // Use full volume if not specified
        if (volume < 0f)
        {
            volume = currentVolume;
        }
        
        if (volume > currentVolume)
        {
            Debug.LogWarning($"Transfer volume {volume} mL exceeds pipette contents {currentVolume} mL");
            return false;
        }
        
        // Find target solution or chemical container
        Solution targetSolution = targetContainer.GetComponent<Solution>();
        Chemical targetChemical = targetContainer.GetComponent<Chemical>();
        
        bool transferSuccess = false;
        
        if (targetSolution != null)
        {
            // Transfer to solution
            transferSuccess = targetSolution.AddChemical(containedChemical, volume);
        }
        else if (targetChemical != null)
        {
            // Transfer to chemical container
            targetChemical.AddVolume(volume);
            transferSuccess = true;
        }
        else
        {
            Debug.LogWarning("Target container is not a valid chemical container");
            return false;
        }
        
        if (transferSuccess)
        {
            // Remove volume from pipette
            currentVolume -= volume;
            currentVolume = Mathf.Max(0f, currentVolume);
            
            // Update pipette state
            if (currentVolume <= 0f)
            {
                containedChemical = null;
                isFilled = false;
            }
            
            lastTransferredVolume = volume;
            
            UpdateVisualRepresentation();
            
            OnVolumeChanged?.Invoke(this, currentVolume);
            OnTransferComplete?.Invoke(this, volume);
            
            // Play dispense sound
            if (dispenseSound != null)
            {
                AudioSource.PlayClipAtPoint(dispenseSound, transform.position);
            }
            
            Debug.Log($"Transferred {volume} mL of {containedChemical?.ChemicalName} to container");
        }
        
        return transferSuccess;
    }
    
    /// <summary>
    /// Draws liquid into the pipette from a source container.
    /// </summary>
    public bool DrawFromContainer(GameObject sourceContainer, float volume)
    {
        if (volume <= 0f || volume > capacity)
        {
            Debug.LogWarning($"Invalid volume for drawing: {volume} mL");
            return false;
        }
        
        // Find source solution or chemical container
        Solution sourceSolution = sourceContainer.GetComponent<Solution>();
        Chemical sourceChemical = sourceContainer.GetComponent<Chemical>();
        
        Chemical sourceChem = null;
        bool drawSuccess = false;
        
        if (sourceSolution != null)
        {
            // Draw from solution (simplified - would need to specify which chemical)
            if (sourceSolution.Components.Count > 0)
            {
                sourceChem = sourceSolution.Components[0].chemical;
                drawSuccess = sourceSolution.RemoveChemical(sourceChem, volume);
            }
        }
        else if (sourceChemical != null)
        {
            // Draw from chemical container
            if (sourceChemical.CurrentVolume >= volume)
            {
                sourceChem = sourceChemical;
                sourceChemical.RemoveVolume(volume);
                drawSuccess = true;
            }
        }
        
        if (drawSuccess && sourceChem != null)
        {
            // Fill pipette with drawn chemical
            FillPipette(sourceChem, volume);
            
            // Play suction sound
            if (suctionSound != null)
            {
                AudioSource.PlayClipAtPoint(suctionSound, transform.position);
            }
            
            Debug.Log($"Drew {volume} mL of {sourceChem.ChemicalName} into pipette");
        }
        
        return drawSuccess;
    }
    
    /// <summary>
    /// Updates the visual representation of the pipette.
    /// </summary>
    private void UpdateVisualRepresentation()
    {
        UpdateLiquidLevel();
        UpdateLiquidColor();
        UpdateTip();
    }
    
    /// <summary>
    /// Updates the liquid level based on current volume.
    /// </summary>
    private void UpdateLiquidLevel()
    {
        if (liquidLevel == null) return;
        
        float fillRatio = currentVolume / capacity;
        float maxHeight = 0.6f; // Maximum height of liquid in pipette
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
    /// Updates the pipette tip appearance.
    /// </summary>
    private void UpdateTip()
    {
        if (tip == null) return;
        
        // Show liquid at tip if pipette is filled
        Renderer tipRenderer = tip.GetComponent<Renderer>();
        if (tipRenderer != null)
        {
            if (isFilled && containedChemical != null)
            {
                tipRenderer.material = liquidMaterial;
            }
            else
            {
                tipRenderer.material = glassMaterial;
            }
        }
    }
    
    /// <summary>
    /// Gets the current reading from the pipette scale.
    /// </summary>
    public float GetPipetteReading()
    {
        return currentVolume;
    }
    
    /// <summary>
    /// Sets the pipette reading (for calibration or manual adjustment).
    /// </summary>
    public void SetPipetteReading(float reading)
    {
        if (reading < 0f || reading > capacity)
        {
            Debug.LogWarning($"Invalid pipette reading: {reading}");
            return;
        }
        
        currentVolume = reading;
        isFilled = reading > 0f;
        
        UpdateVisualRepresentation();
        
        OnVolumeChanged?.Invoke(this, currentVolume);
    }
    
    /// <summary>
    /// Checks if the pipette contains a specific chemical.
    /// </summary>
    public bool ContainsChemical(string chemicalId)
    {
        return containedChemical != null && containedChemical.ChemicalId == chemicalId;
    }
    
    /// <summary>
    /// Configures the pipette with apparatus data.
    /// </summary>
    public void Configure(ApparatusData apparatusData)
    {
        if (apparatusData == null) return;
        
        capacity = apparatusData.capacity;
        precision = apparatusData.precision;
        
        Debug.Log($"Pipette configured: Capacity={capacity} mL, Precision={precision} mL");
    }
    
    /// <summary>
    /// Gets a summary of the pipette's current state.
    /// </summary>
    public string GetPipetteSummary()
    {
        string summary = $"Pipette Status:\n";
        summary += $"Capacity: {capacity} mL\n";
        summary += $"Current Volume: {currentVolume:F2} mL\n";
        summary += $"Last Transfer: {lastTransferredVolume:F2} mL\n";
        
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
    /// Handles interaction when the pipette is clicked.
    /// </summary>
    private void OnMouseDown()
    {
        if (!isInteractable) return;
        
        // Show pipette info
        Debug.Log(GetPipetteSummary());
    }
    
    /// <summary>
    /// Handles interaction when the pipette is dragged.
    /// </summary>
    private void OnMouseDrag()
    {
        if (!isInteractable) return;
        
        // Dragging can control transfer rate or volume
        float dragDelta = Input.GetAxis("Mouse Y");
        if (Mathf.Abs(dragDelta) > 0.01f && isFilled)
        {
            // Adjust transfer volume based on drag
            float transferVolume = Mathf.Clamp(dragDelta * 0.1f, 0f, currentVolume);
            // This could trigger a transfer animation or preview
        }
    }
    
    /// <summary>
    /// Handles collision with other objects for automatic transfer.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!isFilled) return;
        
        // Check if colliding with a valid container
        if (other.GetComponent<Solution>() != null || other.GetComponent<Chemical>() != null)
        {
            // Could trigger automatic transfer or show transfer UI
            Debug.Log($"Pipette near container: {other.name}");
        }
    }
} 