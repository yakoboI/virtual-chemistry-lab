using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Represents a chemical substance with its physical and chemical properties.
/// This class handles the behavior and interactions of chemicals in the virtual laboratory.
/// </summary>
[System.Serializable]
public class Chemical : MonoBehaviour
{
    [Header("Chemical Identity")]
    [SerializeField] private string chemicalId;
    [SerializeField] private string chemicalName;
    [SerializeField] private string chemicalFormula;
    [SerializeField] private ChemicalType chemicalType = ChemicalType.Solid;
    [SerializeField] private ChemicalState state = ChemicalState.Solid;
    
    [Header("Physical Properties")]
    [SerializeField] private float molarMass = 1f; // g/mol
    [SerializeField] private float density = 1f; // g/cm³
    [SerializeField] private Color color = Color.white;
    [SerializeField] private float meltingPoint = 0f; // °C
    [SerializeField] private float boilingPoint = 100f; // °C
    [SerializeField] private float solubility = 1f; // g/100mL water
    
    [Header("Chemical Properties")]
    [SerializeField] private float concentration = 1f; // mol/L
    [SerializeField] private float pH = 7f;
    [SerializeField] private bool isAcid = false;
    [SerializeField] private bool isBase = false;
    [SerializeField] private bool isOxidizing = false;
    [SerializeField] private bool isReducing = false;
    
    [Header("Safety Information")]
    [SerializeField] private List<HazardType> hazards = new List<HazardType>();
    [SerializeField] private string safetyNotes = "";
    [SerializeField] private bool requiresVentilation = false;
    [SerializeField] private bool requiresGloves = false;
    
    [Header("Visual Representation")]
    [SerializeField] private Material chemicalMaterial;
    [SerializeField] private ParticleSystem reactionParticles;
    [SerializeField] private AudioClip pourSound;
    [SerializeField] private AudioClip reactionSound;
    
    [Header("Behavior Settings")]
    [SerializeField] private bool canReact = true;
    [SerializeField] private bool canMix = true;
    [SerializeField] private bool canEvaporate = false;
    [SerializeField] private float evaporationRate = 0f;
    
    // Events
    public static event Action<Chemical, Chemical> OnChemicalReaction;
    public static event Action<Chemical, float> OnConcentrationChanged;
    public static event Action<Chemical, ChemicalState> OnStateChanged;
    public static event Action<Chemical, HazardType> OnHazardTriggered;
    
    // Private variables
    private float currentVolume = 0f;
    private float currentMass = 0f;
    private float currentTemperature = 25f; // Room temperature
    private List<Chemical> mixedChemicals = new List<Chemical>();
    private bool isContaminated = false;
    
    // Properties
    public string ChemicalId => chemicalId;
    public string ChemicalName => chemicalName;
    public string ChemicalFormula => chemicalFormula;
    public ChemicalType Type => chemicalType;
    public ChemicalState State => state;
    public float MolarMass => molarMass;
    public float Density => density;
    public Color Color => color;
    public float Concentration => concentration;
    public float pH => pH;
    public bool IsAcid => isAcid;
    public bool IsBase => isBase;
    public bool IsOxidizing => isOxidizing;
    public bool IsReducing => isReducing;
    public List<HazardType> Hazards => hazards;
    public float CurrentVolume => currentVolume;
    public float CurrentMass => currentMass;
    public float CurrentTemperature => currentTemperature;
    public bool IsContaminated => isContaminated;
    public List<Chemical> MixedChemicals => mixedChemicals;
    
    private void Awake()
    {
        InitializeChemical();
    }
    
    private void Start()
    {
        UpdateVisualRepresentation();
    }
    
    private void Update()
    {
        HandleTemperatureEffects();
        HandleEvaporation();
    }
    
    /// <summary>
    /// Initializes the chemical with default values and validates properties.
    /// </summary>
    private void InitializeChemical()
    {
        // Validate required fields
        if (string.IsNullOrEmpty(chemicalId))
        {
            Debug.LogError($"Chemical {gameObject.name} has no ID assigned!");
            chemicalId = gameObject.name;
        }
        
        if (string.IsNullOrEmpty(chemicalName))
        {
            chemicalName = chemicalId;
        }
        
        // Set initial mass based on volume and density
        if (currentVolume > 0)
        {
            currentMass = currentVolume * density;
        }
        
        // Validate pH range
        pH = Mathf.Clamp(pH, 0f, 14f);
        
        // Set acid/base properties based on pH
        if (pH < 7f)
        {
            isAcid = true;
            isBase = false;
        }
        else if (pH > 7f)
        {
            isAcid = false;
            isBase = true;
        }
        else
        {
            isAcid = false;
            isBase = false;
        }
    }
    
    /// <summary>
    /// Updates the visual representation of the chemical.
    /// </summary>
    private void UpdateVisualRepresentation()
    {
        // Update material color
        if (chemicalMaterial != null)
        {
            chemicalMaterial.color = color;
        }
        
        // Update renderer color if no material is assigned
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && chemicalMaterial == null)
        {
            renderer.material.color = color;
        }
        
        // Scale based on volume
        if (currentVolume > 0)
        {
            float scale = Mathf.Pow(currentVolume / 100f, 1f/3f); // Cubic root for volume scaling
            transform.localScale = Vector3.one * Mathf.Max(0.1f, scale);
        }
    }
    
    /// <summary>
    /// Handles temperature-dependent effects on the chemical.
    /// </summary>
    private void HandleTemperatureEffects()
    {
        // State changes based on temperature
        if (currentTemperature >= boilingPoint && state != ChemicalState.Gas)
        {
            ChangeState(ChemicalState.Gas);
        }
        else if (currentTemperature >= meltingPoint && state == ChemicalState.Solid)
        {
            ChangeState(ChemicalState.Liquid);
        }
        else if (currentTemperature < meltingPoint && state == ChemicalState.Liquid)
        {
            ChangeState(ChemicalState.Solid);
        }
        
        // Temperature-dependent reactions
        if (canReact && mixedChemicals.Count > 0)
        {
            foreach (Chemical mixedChemical in mixedChemicals)
            {
                CheckForReaction(mixedChemical);
            }
        }
    }
    
    /// <summary>
    /// Handles evaporation of volatile chemicals.
    /// </summary>
    private void HandleEvaporation()
    {
        if (!canEvaporate || evaporationRate <= 0f) return;
        
        if (state == ChemicalState.Liquid && currentTemperature > 0f)
        {
            float evaporationAmount = evaporationRate * Time.deltaTime * (currentTemperature / 25f);
            RemoveVolume(evaporationAmount);
            
            // Play evaporation particles
            if (reactionParticles != null && !reactionParticles.isPlaying)
            {
                reactionParticles.Play();
            }
        }
    }
    
    /// <summary>
    /// Adds volume to the chemical container.
    /// </summary>
    public void AddVolume(float volume)
    {
        if (volume <= 0f) return;
        
        currentVolume += volume;
        currentMass = currentVolume * density;
        
        UpdateVisualRepresentation();
        OnConcentrationChanged?.Invoke(this, concentration);
        
        // Play pour sound
        if (pourSound != null)
        {
            AudioSource.PlayClipAtPoint(pourSound, transform.position);
        }
    }
    
    /// <summary>
    /// Removes volume from the chemical container.
    /// </summary>
    public void RemoveVolume(float volume)
    {
        if (volume <= 0f) return;
        
        currentVolume = Mathf.Max(0f, currentVolume - volume);
        currentMass = currentVolume * density;
        
        UpdateVisualRepresentation();
        OnConcentrationChanged?.Invoke(this, concentration);
        
        // If volume reaches zero, destroy the chemical
        if (currentVolume <= 0f)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Changes the temperature of the chemical.
    /// </summary>
    public void ChangeTemperature(float newTemperature)
    {
        currentTemperature = newTemperature;
        
        // Trigger temperature-dependent effects
        HandleTemperatureEffects();
    }
    
    /// <summary>
    /// Changes the state of the chemical (solid, liquid, gas).
    /// </summary>
    public void ChangeState(ChemicalState newState)
    {
        if (state == newState) return;
        
        ChemicalState previousState = state;
        state = newState;
        
        Debug.Log($"{chemicalName} changed state from {previousState} to {newState}");
        OnStateChanged?.Invoke(this, newState);
        
        // Handle state-specific effects
        switch (newState)
        {
            case ChemicalState.Gas:
                HandleGasState();
                break;
            case ChemicalState.Liquid:
                HandleLiquidState();
                break;
            case ChemicalState.Solid:
                HandleSolidState();
                break;
        }
    }
    
    /// <summary>
    /// Mixes this chemical with another chemical.
    /// </summary>
    public bool MixWith(Chemical otherChemical)
    {
        if (!canMix || !otherChemical.canMix) return false;
        
        // Check for incompatibilities
        if (IsIncompatibleWith(otherChemical))
        {
            TriggerHazard(HazardType.IncompatibleChemicals);
            return false;
        }
        
        // Add to mixed chemicals list
        if (!mixedChemicals.Contains(otherChemical))
        {
            mixedChemicals.Add(otherChemical);
        }
        
        // Check for immediate reaction
        if (canReact)
        {
            CheckForReaction(otherChemical);
        }
        
        return true;
    }
    
    /// <summary>
    /// Checks if this chemical can react with another chemical.
    /// </summary>
    public void CheckForReaction(Chemical otherChemical)
    {
        if (!canReact || otherChemical == null) return;
        
        // Simple reaction logic - can be expanded
        bool willReact = false;
        string reactionType = "";
        
        // Acid-base reaction
        if ((isAcid && otherChemical.isBase) || (isBase && otherChemical.isAcid))
        {
            willReact = true;
            reactionType = "Neutralization";
        }
        
        // Oxidation-reduction reaction
        if ((isOxidizing && otherChemical.isReducing) || (isReducing && otherChemical.isOxidizing))
        {
            willReact = true;
            reactionType = "Redox";
        }
        
        if (willReact)
        {
            TriggerReaction(otherChemical, reactionType);
        }
    }
    
    /// <summary>
    /// Triggers a chemical reaction with another chemical.
    /// </summary>
    private void TriggerReaction(Chemical otherChemical, string reactionType)
    {
        Debug.Log($"Reaction triggered: {chemicalName} + {otherChemical.chemicalName} = {reactionType}");
        
        // Play reaction effects
        if (reactionParticles != null)
        {
            reactionParticles.Play();
        }
        
        if (reactionSound != null)
        {
            AudioSource.PlayClipAtPoint(reactionSound, transform.position);
        }
        
        // Notify reaction event
        OnChemicalReaction?.Invoke(this, otherChemical);
        
        // Handle reaction products (simplified)
        HandleReactionProducts(otherChemical, reactionType);
    }
    
    /// <summary>
    /// Handles the products of a chemical reaction.
    /// </summary>
    private void HandleReactionProducts(Chemical otherChemical, string reactionType)
    {
        // This is a simplified implementation
        // In a full system, you would have a reaction database
        
        if (reactionType == "Neutralization")
        {
            // Neutralization produces water and salt
            concentration *= 0.5f; // Dilute the solution
            otherChemical.concentration *= 0.5f;
        }
        else if (reactionType == "Redox")
        {
            // Redox reactions may produce gases or precipitates
            if (Random.Range(0f, 1f) > 0.5f)
            {
                // 50% chance of gas production
                TriggerHazard(HazardType.GasProduction);
            }
        }
    }
    
    /// <summary>
    /// Checks if this chemical is incompatible with another chemical.
    /// </summary>
    public bool IsIncompatibleWith(Chemical otherChemical)
    {
        if (otherChemical == null) return false;
        
        // Check for hazard incompatibilities
        foreach (HazardType hazard in hazards)
        {
            if (otherChemical.hazards.Contains(hazard))
            {
                return true;
            }
        }
        
        // Check for specific incompatibilities
        if ((isOxidizing && otherChemical.isReducing) || (isReducing && otherChemical.isOxidizing))
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Triggers a safety hazard.
    /// </summary>
    public void TriggerHazard(HazardType hazardType)
    {
        Debug.LogWarning($"Hazard triggered: {chemicalName} - {hazardType}");
        OnHazardTriggered?.Invoke(this, hazardType);
        
        // Notify safety manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandleSafetyViolation(hazardType.ToString(), 
                $"{chemicalName} triggered {hazardType} hazard");
        }
    }
    
    /// <summary>
    /// Contaminates the chemical with another substance.
    /// </summary>
    public void Contaminate(Chemical contaminant)
    {
        isContaminated = true;
        Debug.LogWarning($"{chemicalName} has been contaminated with {contaminant.chemicalName}");
        
        // Reduce concentration due to contamination
        concentration *= 0.8f;
        OnConcentrationChanged?.Invoke(this, concentration);
    }
    
    // State-specific handling methods
    private void HandleGasState()
    {
        // Gases can escape containers
        canEvaporate = true;
        evaporationRate = 2f;
    }
    
    private void HandleLiquidState()
    {
        // Liquids can flow and mix
        canMix = true;
        canEvaporate = true;
        evaporationRate = 0.1f;
    }
    
    private void HandleSolidState()
    {
        // Solids are stable
        canEvaporate = false;
        evaporationRate = 0f;
    }
    
    /// <summary>
    /// Gets a summary of the chemical's properties.
    /// </summary>
    public string GetChemicalSummary()
    {
        return $"Name: {chemicalName}\n" +
               $"Formula: {chemicalFormula}\n" +
               $"State: {state}\n" +
               $"Concentration: {concentration:F2} mol/L\n" +
               $"pH: {pH:F1}\n" +
               $"Volume: {currentVolume:F1} mL\n" +
               $"Temperature: {currentTemperature:F1}°C";
    }
    
    private void OnDestroy()
    {
        // Clean up mixed chemicals reference
        foreach (Chemical mixedChemical in mixedChemicals)
        {
            if (mixedChemical != null)
            {
                mixedChemical.mixedChemicals.Remove(this);
            }
        }
    }
}

/// <summary>
/// Enumeration of chemical types.
/// </summary>
public enum ChemicalType
{
    Acid,
    Base,
    Salt,
    Organic,
    Inorganic,
    Indicator,
    Solvent
}

/// <summary>
/// Enumeration of chemical states.
/// </summary>
public enum ChemicalState
{
    Solid,
    Liquid,
    Gas
}

/// <summary>
/// Enumeration of hazard types.
/// </summary>
public enum HazardType
{
    Toxic,
    Corrosive,
    Flammable,
    Explosive,
    Oxidizing,
    Irritant,
    Carcinogenic,
    IncompatibleChemicals,
    GasProduction,
    HeatGeneration
} 