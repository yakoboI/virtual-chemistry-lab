using UnityEngine;
using System;

/// <summary>
/// Controls the behavior of a flask apparatus for containing solutions and reactions.
/// Handles mixing, heating, and solution management.
/// </summary>
public class FlaskController : MonoBehaviour
{
    [Header("Flask Properties")]
    [SerializeField] private float capacity = 250f; // mL
    [SerializeField] private float currentVolume = 0f;
    [SerializeField] private Solution containedSolution;
    
    [Header("Visual Settings")]
    [SerializeField] private Material glassMaterial;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] private Transform liquidLevel;
    [SerializeField] private Transform stirringRod;
    
    [Header("Audio")]
    [SerializeField] private AudioClip pourSound;
    [SerializeField] private AudioClip stirSound;
    [SerializeField] private AudioClip reactionSound;
    
    [Header("Interaction")]
    [SerializeField] private bool isInteractable = true;
    [SerializeField] private bool isStirring = false;
    [SerializeField] private bool isHeating = false;
    [SerializeField] private float temperature = 25f; // °C
    
    // Events
    public static event Action<FlaskController, Solution> OnSolutionAdded;
    public static event Action<FlaskController, float> OnVolumeChanged;
    public static event Action<FlaskController, float> OnTemperatureChanged;
    public static event Action<FlaskController> OnReactionOccurred;
    
    // Properties
    public float Capacity => capacity;
    public float CurrentVolume => currentVolume;
    public Solution ContainedSolution => containedSolution;
    public bool IsEmpty => currentVolume <= 0f;
    public bool IsFull => currentVolume >= capacity;
    public bool IsStirring => isStirring;
    public bool IsHeating => isHeating;
    public float Temperature => temperature;
    
    private Vector3 originalLiquidLevelPosition;
    private float stirringSpeed = 1f;
    private float heatingRate = 2f; // °C per second
    
    private void Awake()
    {
        InitializeFlask();
    }
    
    private void Start()
    {
        UpdateVisualRepresentation();
    }
    
    private void Update()
    {
        HandleStirring();
        HandleHeating();
    }
    
    /// <summary>
    /// Initializes the flask with default settings.
    /// </summary>
    private void InitializeFlask()
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
        
        // Create contained solution
        containedSolution = gameObject.AddComponent<Solution>();
        containedSolution.SolutionName = "Flask Solution";
    }
    
    /// <summary>
    /// Adds a solution to the flask.
    /// </summary>
    public bool AddSolution(Solution solution, float volume)
    {
        if (solution == null || volume <= 0f)
        {
            Debug.LogWarning("Invalid solution or volume for flask");
            return false;
        }
        
        if (currentVolume + volume > capacity)
        {
            Debug.LogWarning($"Volume {volume} mL would exceed flask capacity {capacity} mL");
            return false;
        }
        
        // Mix the solution with the contained solution
        bool success = containedSolution.MixWith(solution);
        
        if (success)
        {
            currentVolume += volume;
            UpdateVisualRepresentation();
            
            // Play pour sound
            if (pourSound != null)
            {
                AudioSource.PlayClipAtPoint(pourSound, transform.position);
            }
            
            OnSolutionAdded?.Invoke(this, solution);
            OnVolumeChanged?.Invoke(this, currentVolume);
            
            Debug.Log($"Added {volume} mL of solution to flask");
        }
        
        return success;
    }
    
    /// <summary>
    /// Adds a chemical directly to the flask.
    /// </summary>
    public bool AddChemical(Chemical chemical, float volume)
    {
        if (chemical == null || volume <= 0f)
        {
            Debug.LogWarning("Invalid chemical or volume for flask");
            return false;
        }
        
        if (currentVolume + volume > capacity)
        {
            Debug.LogWarning($"Volume {volume} mL would exceed flask capacity {capacity} mL");
            return false;
        }
        
        // Add chemical to the contained solution
        bool success = containedSolution.AddChemical(chemical, volume);
        
        if (success)
        {
            currentVolume += volume;
            UpdateVisualRepresentation();
            
            // Play pour sound
            if (pourSound != null)
            {
                AudioSource.PlayClipAtPoint(pourSound, transform.position);
            }
            
            OnVolumeChanged?.Invoke(this, currentVolume);
            
            Debug.Log($"Added {volume} mL of {chemical.ChemicalName} to flask");
        }
        
        return success;
    }
    
    /// <summary>
    /// Removes solution from the flask.
    /// </summary>
    public bool RemoveSolution(float volume)
    {
        if (volume <= 0f || volume > currentVolume)
        {
            Debug.LogWarning("Invalid volume for removal from flask");
            return false;
        }
        
        // Remove volume from contained solution
        float removalRatio = volume / currentVolume;
        
        // Remove chemicals proportionally
        foreach (SolutionComponent component in containedSolution.Components)
        {
            if (component.chemical != null)
            {
                float removeVolume = component.volume * removalRatio;
                containedSolution.RemoveChemical(component.chemical, removeVolume);
            }
        }
        
        currentVolume -= volume;
        currentVolume = Mathf.Max(0f, currentVolume);
        
        UpdateVisualRepresentation();
        
        OnVolumeChanged?.Invoke(this, currentVolume);
        
        Debug.Log($"Removed {volume} mL from flask");
        return true;
    }
    
    /// <summary>
    /// Empties the flask completely.
    /// </summary>
    public void EmptyFlask()
    {
        if (currentVolume <= 0f) return;
        
        float emptiedVolume = currentVolume;
        currentVolume = 0f;
        
        // Clear the contained solution
        containedSolution = gameObject.AddComponent<Solution>();
        containedSolution.SolutionName = "Flask Solution";
        
        UpdateVisualRepresentation();
        
        OnVolumeChanged?.Invoke(this, currentVolume);
        
        Debug.Log($"Flask emptied: {emptiedVolume} mL");
    }
    
    /// <summary>
    /// Starts stirring the flask contents.
    /// </summary>
    public void StartStirring()
    {
        if (IsEmpty) return;
        
        isStirring = true;
        
        // Play stir sound
        if (stirSound != null)
        {
            AudioSource.PlayClipAtPoint(stirSound, transform.position);
        }
        
        Debug.Log("Started stirring flask contents");
    }
    
    /// <summary>
    /// Stops stirring the flask contents.
    /// </summary>
    public void StopStirring()
    {
        isStirring = false;
        Debug.Log("Stopped stirring flask contents");
    }
    
    /// <summary>
    /// Starts heating the flask.
    /// </summary>
    public void StartHeating()
    {
        isHeating = true;
        Debug.Log("Started heating flask");
    }
    
    /// <summary>
    /// Stops heating the flask.
    /// </summary>
    public void StopHeating()
    {
        isHeating = false;
        Debug.Log("Stopped heating flask");
    }
    
    /// <summary>
    /// Handles the stirring animation and effects.
    /// </summary>
    private void HandleStirring()
    {
        if (!isStirring || stirringRod == null) return;
        
        // Rotate stirring rod
        stirringRod.Rotate(0f, 0f, stirringSpeed * 360f * Time.deltaTime);
        
        // Stirring can affect reaction rates
        if (containedSolution != null)
        {
            // Increase reaction rates during stirring
            // This could trigger faster chemical reactions
        }
    }
    
    /// <summary>
    /// Handles heating effects on the flask contents.
    /// </summary>
    private void HandleHeating()
    {
        if (!isHeating) return;
        
        // Increase temperature
        temperature += heatingRate * Time.deltaTime;
        
        // Update solution temperature
        if (containedSolution != null)
        {
            containedSolution.ChangeTemperature(temperature);
        }
        
        OnTemperatureChanged?.Invoke(this, temperature);
        
        // Check for temperature-dependent reactions
        CheckForTemperatureReactions();
    }
    
    /// <summary>
    /// Checks for temperature-dependent reactions in the flask.
    /// </summary>
    private void CheckForTemperatureReactions()
    {
        if (containedSolution == null || containedSolution.Components.Count < 2) return;
        
        // Check if temperature has triggered any reactions
        bool reactionOccurred = false;
        
        // This is a simplified check - in a full system, you'd have a reaction database
        if (temperature > 50f && containedSolution.Components.Count > 1)
        {
            // Simulate temperature-dependent reaction
            reactionOccurred = true;
        }
        
        if (reactionOccurred)
        {
            OnReactionOccurred?.Invoke(this);
            
            // Play reaction sound
            if (reactionSound != null)
            {
                AudioSource.PlayClipAtPoint(reactionSound, transform.position);
            }
            
            Debug.Log("Temperature-dependent reaction occurred in flask");
        }
    }
    
    /// <summary>
    /// Updates the visual representation of the flask.
    /// </summary>
    private void UpdateVisualRepresentation()
    {
        UpdateLiquidLevel();
        UpdateLiquidColor();
        UpdateStirringRod();
    }
    
    /// <summary>
    /// Updates the liquid level based on current volume.
    /// </summary>
    private void UpdateLiquidLevel()
    {
        if (liquidLevel == null) return;
        
        float fillRatio = currentVolume / capacity;
        float maxHeight = 0.7f; // Maximum height of liquid in flask
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
    /// Updates the color of the liquid based on the contained solution.
    /// </summary>
    private void UpdateLiquidColor()
    {
        if (liquidMaterial == null || liquidLevel == null) return;
        
        if (containedSolution != null && containedSolution.Components.Count > 0)
        {
            liquidMaterial.color = containedSolution.SolutionColor;
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
    /// Updates the stirring rod visibility and position.
    /// </summary>
    private void UpdateStirringRod()
    {
        if (stirringRod == null) return;
        
        // Show stirring rod only when there's liquid
        stirringRod.gameObject.SetActive(currentVolume > 0f);
        
        if (currentVolume > 0f)
        {
            // Position stirring rod in the center of the liquid
            Vector3 rodPosition = liquidLevel.position;
            rodPosition.y = liquidLevel.position.y + liquidLevel.localScale.y * 0.3f;
            stirringRod.position = rodPosition;
        }
    }
    
    /// <summary>
    /// Sets the temperature of the flask.
    /// </summary>
    public void SetTemperature(float newTemperature)
    {
        temperature = newTemperature;
        
        // Update solution temperature
        if (containedSolution != null)
        {
            containedSolution.ChangeTemperature(temperature);
        }
        
        OnTemperatureChanged?.Invoke(this, temperature);
    }
    
    /// <summary>
    /// Configures the flask with apparatus data.
    /// </summary>
    public void Configure(ApparatusData apparatusData)
    {
        if (apparatusData == null) return;
        
        capacity = apparatusData.capacity;
        
        Debug.Log($"Flask configured: Capacity={capacity} mL");
    }
    
    /// <summary>
    /// Gets a summary of the flask's current state.
    /// </summary>
    public string GetFlaskSummary()
    {
        string summary = $"Flask Status:\n";
        summary += $"Capacity: {capacity} mL\n";
        summary += $"Current Volume: {currentVolume:F1} mL\n";
        summary += $"Temperature: {temperature:F1}°C\n";
        
        if (containedSolution != null && containedSolution.Components.Count > 0)
        {
            summary += $"Solution pH: {containedSolution.pH:F2}\n";
            summary += $"Components: {containedSolution.Components.Count}\n";
            
            foreach (SolutionComponent component in containedSolution.Components)
            {
                if (component.chemical != null)
                {
                    summary += $"  - {component.chemical.ChemicalName}: {component.volume:F1} mL\n";
                }
            }
        }
        else
        {
            summary += $"Contents: Empty\n";
        }
        
        summary += $"Stirring: {(IsStirring ? "Yes" : "No")}\n";
        summary += $"Heating: {(IsHeating ? "Yes" : "No")}";
        
        return summary;
    }
    
    /// <summary>
    /// Handles interaction when the flask is clicked.
    /// </summary>
    private void OnMouseDown()
    {
        if (!isInteractable) return;
        
        // Toggle stirring or show flask info
        if (Input.GetKey(KeyCode.LeftControl))
        {
            // Ctrl+Click to toggle stirring
            if (IsStirring)
                StopStirring();
            else
                StartStirring();
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            // Shift+Click to toggle heating
            if (IsHeating)
                StopHeating();
            else
                StartHeating();
        }
        else
        {
            // Regular click to show info
            Debug.Log(GetFlaskSummary());
        }
    }
    
    /// <summary>
    /// Handles interaction when the flask is dragged.
    /// </summary>
    private void OnMouseDrag()
    {
        if (!isInteractable) return;
        
        // Dragging can control stirring speed or heating rate
        float dragDelta = Input.GetAxis("Mouse Y");
        if (Mathf.Abs(dragDelta) > 0.01f)
        {
            if (IsStirring)
            {
                // Adjust stirring speed
                stirringSpeed = Mathf.Clamp(stirringSpeed + dragDelta * 0.5f, 0.5f, 3f);
            }
            else if (IsHeating)
            {
                // Adjust heating rate
                heatingRate = Mathf.Clamp(heatingRate + dragDelta * 0.5f, 0.5f, 5f);
            }
        }
    }
} 