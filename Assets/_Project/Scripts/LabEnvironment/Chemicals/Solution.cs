using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Represents a solution containing multiple chemicals with proper concentration management.
/// Handles dilution, mixing, and pH calculations for complex solutions.
/// </summary>
[System.Serializable]
public class Solution : MonoBehaviour
{
    [Header("Solution Properties")]
    [SerializeField] private string solutionName = "Solution";
    [SerializeField] private float totalVolume = 0f; // mL
    [SerializeField] private float temperature = 25f; // °C
    [SerializeField] private Color solutionColor = Color.clear;
    [SerializeField] private float density = 1f; // g/cm³
    
    [Header("Solution Components")]
    [SerializeField] private List<SolutionComponent> components = new List<SolutionComponent>();
    
    [Header("Visual Settings")]
    [SerializeField] private Material solutionMaterial;
    [SerializeField] private ParticleSystem mixingParticles;
    [SerializeField] private AudioClip mixingSound;
    
    // Events
    public static event Action<Solution> OnSolutionCreated;
    public static event Action<Solution, Chemical> OnChemicalAdded;
    public static event Action<Solution, Chemical> OnChemicalRemoved;
    public static event Action<Solution, float> OnConcentrationChanged;
    public static event Action<Solution, float> OnpHChanged;
    
    // Properties
    public string SolutionName => solutionName;
    public float TotalVolume => totalVolume;
    public float Temperature => temperature;
    public Color SolutionColor => solutionColor;
    public float Density => density;
    public List<SolutionComponent> Components => components;
    public float pH => CalculatepH();
    public float TotalMass => CalculateTotalMass();
    
    private void Awake()
    {
        InitializeSolution();
    }
    
    private void Start()
    {
        UpdateVisualRepresentation();
        OnSolutionCreated?.Invoke(this);
    }
    
    /// <summary>
    /// Initializes the solution with default values.
    /// </summary>
    private void InitializeSolution()
    {
        if (string.IsNullOrEmpty(solutionName))
        {
            solutionName = gameObject.name;
        }
        
        // Initialize solution color based on components
        UpdateSolutionColor();
    }
    
    /// <summary>
    /// Adds a chemical to the solution with specified volume and concentration.
    /// </summary>
    public bool AddChemical(Chemical chemical, float volume, float concentration = -1f)
    {
        if (chemical == null || volume <= 0f) return false;
        
        // Use chemical's default concentration if not specified
        if (concentration < 0f)
        {
            concentration = chemical.Concentration;
        }
        
        // Check for incompatibilities
        if (HasIncompatibleChemicals(chemical))
        {
            Debug.LogWarning($"Cannot add {chemical.ChemicalName}: incompatible with existing chemicals");
            return false;
        }
        
        // Check if chemical already exists in solution
        SolutionComponent existingComponent = components.FirstOrDefault(c => c.chemical == chemical);
        
        if (existingComponent != null)
        {
            // Update existing component
            float newVolume = existingComponent.volume + volume;
            float newConcentration = (existingComponent.concentration * existingComponent.volume + 
                                    concentration * volume) / newVolume;
            
            existingComponent.volume = newVolume;
            existingComponent.concentration = newConcentration;
            existingComponent.mass = newVolume * chemical.Density;
        }
        else
        {
            // Add new component
            SolutionComponent newComponent = new SolutionComponent
            {
                chemical = chemical,
                volume = volume,
                concentration = concentration,
                mass = volume * chemical.Density
            };
            
            components.Add(newComponent);
        }
        
        // Update solution properties
        totalVolume += volume;
        UpdateSolutionProperties();
        
        // Notify events
        OnChemicalAdded?.Invoke(this, chemical);
        OnConcentrationChanged?.Invoke(this, GetTotalConcentration());
        
        // Play mixing effects
        PlayMixingEffects();
        
        return true;
    }
    
    /// <summary>
    /// Removes a chemical from the solution.
    /// </summary>
    public bool RemoveChemical(Chemical chemical, float volume = -1f)
    {
        SolutionComponent component = components.FirstOrDefault(c => c.chemical == chemical);
        if (component == null) return false;
        
        // Remove all of the chemical if volume not specified
        if (volume < 0f)
        {
            volume = component.volume;
        }
        
        if (volume >= component.volume)
        {
            // Remove entire component
            components.Remove(component);
            totalVolume -= component.volume;
        }
        else
        {
            // Reduce component volume
            component.volume -= volume;
            component.mass = component.volume * chemical.Density;
            totalVolume -= volume;
        }
        
        UpdateSolutionProperties();
        
        // Notify events
        OnChemicalRemoved?.Invoke(this, chemical);
        OnConcentrationChanged?.Invoke(this, GetTotalConcentration());
        
        return true;
    }
    
    /// <summary>
    /// Dilutes the solution by adding a specified volume of solvent.
    /// </summary>
    public void Dilute(float volumeOfSolvent)
    {
        if (volumeOfSolvent <= 0f) return;
        
        float dilutionFactor = totalVolume / (totalVolume + volumeOfSolvent);
        
        // Dilute all components
        foreach (SolutionComponent component in components)
        {
            component.concentration *= dilutionFactor;
            component.mass *= dilutionFactor;
        }
        
        totalVolume += volumeOfSolvent;
        UpdateSolutionProperties();
        
        OnConcentrationChanged?.Invoke(this, GetTotalConcentration());
    }
    
    /// <summary>
    /// Mixes this solution with another solution.
    /// </summary>
    public bool MixWith(Solution otherSolution)
    {
        if (otherSolution == null) return false;
        
        // Check for incompatibilities
        if (HasIncompatibleSolutions(otherSolution))
        {
            Debug.LogWarning("Cannot mix solutions: incompatible chemicals detected");
            return false;
        }
        
        // Add all components from the other solution
        foreach (SolutionComponent component in otherSolution.components)
        {
            AddChemical(component.chemical, component.volume, component.concentration);
        }
        
        // Transfer temperature (weighted average)
        float newTemperature = (temperature * totalVolume + 
                              otherSolution.temperature * otherSolution.totalVolume) / 
                              (totalVolume + otherSolution.totalVolume);
        ChangeTemperature(newTemperature);
        
        return true;
    }
    
    /// <summary>
    /// Changes the temperature of the solution.
    /// </summary>
    public void ChangeTemperature(float newTemperature)
    {
        temperature = newTemperature;
        
        // Update all chemical temperatures
        foreach (SolutionComponent component in components)
        {
            if (component.chemical != null)
            {
                component.chemical.ChangeTemperature(temperature);
            }
        }
        
        // Check for temperature-dependent reactions
        CheckForTemperatureReactions();
    }
    
    /// <summary>
    /// Calculates the pH of the solution based on its components.
    /// </summary>
    public float CalculatepH()
    {
        if (components.Count == 0) return 7f;
        
        float totalAcidity = 0f;
        float totalBasicity = 0f;
        
        foreach (SolutionComponent component in components)
        {
            if (component.chemical == null) continue;
            
            float moles = component.concentration * component.volume / 1000f; // Convert to moles
            
            if (component.chemical.IsAcid)
            {
                totalAcidity += moles * Mathf.Pow(10, -component.chemical.pH);
            }
            else if (component.chemical.IsBase)
            {
                totalBasicity += moles * Mathf.Pow(10, -(14 - component.chemical.pH));
            }
        }
        
        if (totalAcidity > totalBasicity)
        {
            return -Mathf.Log10(totalAcidity - totalBasicity);
        }
        else if (totalBasicity > totalAcidity)
        {
            return 14 + Mathf.Log10(totalBasicity - totalAcidity);
        }
        else
        {
            return 7f; // Neutral
        }
    }
    
    /// <summary>
    /// Gets the concentration of a specific chemical in the solution.
    /// </summary>
    public float GetChemicalConcentration(Chemical chemical)
    {
        SolutionComponent component = components.FirstOrDefault(c => c.chemical == chemical);
        return component?.concentration ?? 0f;
    }
    
    /// <summary>
    /// Gets the total concentration of all chemicals in the solution.
    /// </summary>
    public float GetTotalConcentration()
    {
        return components.Sum(c => c.concentration);
    }
    
    /// <summary>
    /// Calculates the total mass of the solution.
    /// </summary>
    public float CalculateTotalMass()
    {
        return components.Sum(c => c.mass);
    }
    
    /// <summary>
    /// Checks if adding a chemical would create incompatibilities.
    /// </summary>
    private bool HasIncompatibleChemicals(Chemical newChemical)
    {
        foreach (SolutionComponent component in components)
        {
            if (component.chemical != null && 
                component.chemical.IsIncompatibleWith(newChemical))
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Checks if mixing with another solution would create incompatibilities.
    /// </summary>
    private bool HasIncompatibleSolutions(Solution otherSolution)
    {
        foreach (SolutionComponent component1 in components)
        {
            foreach (SolutionComponent component2 in otherSolution.components)
            {
                if (component1.chemical != null && component2.chemical != null &&
                    component1.chemical.IsIncompatibleWith(component2.chemical))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    /// <summary>
    /// Checks for temperature-dependent reactions between solution components.
    /// </summary>
    private void CheckForTemperatureReactions()
    {
        for (int i = 0; i < components.Count; i++)
        {
            for (int j = i + 1; j < components.Count; j++)
            {
                if (components[i].chemical != null && components[j].chemical != null)
                {
                    components[i].chemical.CheckForReaction(components[j].chemical);
                }
            }
        }
    }
    
    /// <summary>
    /// Updates solution properties after changes.
    /// </summary>
    private void UpdateSolutionProperties()
    {
        // Update density (weighted average)
        if (totalVolume > 0)
        {
            density = CalculateTotalMass() / totalVolume;
        }
        
        // Update solution color
        UpdateSolutionColor();
        
        // Update pH
        float newpH = CalculatepH();
        if (Mathf.Abs(newpH - pH) > 0.1f)
        {
            OnpHChanged?.Invoke(this, newpH);
        }
    }
    
    /// <summary>
    /// Updates the solution color based on its components.
    /// </summary>
    private void UpdateSolutionColor()
    {
        if (components.Count == 0)
        {
            solutionColor = Color.clear;
            return;
        }
        
        // Blend colors of all components (weighted by concentration)
        Color blendedColor = Color.clear;
        float totalConcentration = GetTotalConcentration();
        
        foreach (SolutionComponent component in components)
        {
            if (component.chemical != null && totalConcentration > 0)
            {
                float weight = component.concentration / totalConcentration;
                blendedColor += component.chemical.Color * weight;
            }
        }
        
        solutionColor = blendedColor;
        UpdateVisualRepresentation();
    }
    
    /// <summary>
    /// Updates the visual representation of the solution.
    /// </summary>
    private void UpdateVisualRepresentation()
    {
        // Update material color
        if (solutionMaterial != null)
        {
            solutionMaterial.color = solutionColor;
        }
        
        // Update renderer color if no material is assigned
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && solutionMaterial == null)
        {
            renderer.material.color = solutionColor;
        }
        
        // Scale based on volume
        if (totalVolume > 0)
        {
            float scale = Mathf.Pow(totalVolume / 100f, 1f/3f); // Cubic root for volume scaling
            transform.localScale = Vector3.one * Mathf.Max(0.1f, scale);
        }
    }
    
    /// <summary>
    /// Plays mixing effects (particles and sound).
    /// </summary>
    private void PlayMixingEffects()
    {
        if (mixingParticles != null)
        {
            mixingParticles.Play();
        }
        
        if (mixingSound != null)
        {
            AudioSource.PlayClipAtPoint(mixingSound, transform.position);
        }
    }
    
    /// <summary>
    /// Gets a summary of the solution's properties.
    /// </summary>
    public string GetSolutionSummary()
    {
        string summary = $"Solution: {solutionName}\n";
        summary += $"Total Volume: {totalVolume:F1} mL\n";
        summary += $"Temperature: {temperature:F1}°C\n";
        summary += $"pH: {pH:F2}\n";
        summary += $"Density: {density:F3} g/cm³\n";
        summary += $"Components ({components.Count}):\n";
        
        foreach (SolutionComponent component in components)
        {
            if (component.chemical != null)
            {
                summary += $"  - {component.chemical.ChemicalName}: {component.concentration:F3} mol/L ({component.volume:F1} mL)\n";
            }
        }
        
        return summary;
    }
    
    /// <summary>
    /// Creates a copy of this solution.
    /// </summary>
    public Solution CreateCopy()
    {
        GameObject copyObject = Instantiate(gameObject);
        Solution copySolution = copyObject.GetComponent<Solution>();
        
        if (copySolution != null)
        {
            copySolution.solutionName = solutionName + " (Copy)";
            copySolution.totalVolume = totalVolume;
            copySolution.temperature = temperature;
            copySolution.density = density;
            
            // Copy components
            copySolution.components.Clear();
            foreach (SolutionComponent component in components)
            {
                copySolution.components.Add(new SolutionComponent
                {
                    chemical = component.chemical,
                    volume = component.volume,
                    concentration = component.concentration,
                    mass = component.mass
                });
            }
            
            copySolution.UpdateSolutionProperties();
        }
        
        return copySolution;
    }
}

/// <summary>
/// Represents a component of a solution with its properties.
/// </summary>
[System.Serializable]
public struct SolutionComponent
{
    public Chemical chemical;
    public float volume;        // mL
    public float concentration; // mol/L
    public float mass;          // g
} 