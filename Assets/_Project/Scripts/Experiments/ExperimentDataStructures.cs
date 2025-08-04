using System;
using System.Collections.Generic;

/// <summary>
/// Data structures for JSON deserialization of experiment data.
/// </summary>

[System.Serializable]
public class ExperimentData
{
    public string id;
    public string title;
    public string description;
    public string category;
    public string difficulty;
    public int estimatedDuration;
    public ChemicalRequirement[] requiredChemicals;
    public ApparatusData[] requiredApparatus;
    public ExperimentProcedure procedure;
    public ExperimentAssessment assessment;
    public string[] learningObjectives;
    public string[] safetyNotes;
    public string[] tips;
}

[System.Serializable]
public class ChemicalRequirement
{
    public string id;
    public string name;
    public float volume;
    public float concentration;
}

[System.Serializable]
public class ApparatusData
{
    public string id;
    public string name;
    public float capacity;
    public float precision;
    public string purpose;
}

[System.Serializable]
public class ExperimentProcedure
{
    public ExperimentStep[] steps;
}

[System.Serializable]
public class ExperimentStep
{
    public int stepNumber;
    public string title;
    public string description;
    public string instructions;
    public int expectedDuration;
    public string safetyNotes;
    public StepValidationData validation;
}

[System.Serializable]
public class StepValidationData
{
    public string type;
    public ValidationParameters parameters;
}

[System.Serializable]
public class ValidationParameters
{
    public string chemical;
    public float volume;
    public float tolerance;
    public string indicator;
    public string fromColor;
    public string toColor;
    public float[] expectedRange;
    public int drops;
}

[System.Serializable]
public class ExperimentAssessment
{
    public AssessmentCriterion[] criteria;
    public ExpectedResults expectedResults;
    public Calculation[] calculations;
}

[System.Serializable]
public class AssessmentCriterion
{
    public string criterion;
    public string description;
    public int weight;
    public ScoringCriteria scoring;
}

[System.Serializable]
public class ScoringCriteria
{
    public ScoreLevel excellent;
    public ScoreLevel good;
    public ScoreLevel satisfactory;
    public ScoreLevel poor;
}

[System.Serializable]
public class ScoreLevel
{
    public float[] range;
    public string description;
    public int score;
}

[System.Serializable]
public class ExpectedResults
{
    public ResultValue firstEndpoint;
    public ResultValue secondEndpoint;
    public ResultValue concentration;
}

[System.Serializable]
public class ResultValue
{
    public float volume;
    public float value;
    public float tolerance;
    public string description;
}

[System.Serializable]
public class Calculation
{
    public string name;
    public string formula;
    public string description;
}

[System.Serializable]
public class ChemicalData
{
    public string id;
    public string name;
    public string formula;
    public string type;
    public string state;
    public PhysicalProperties physicalProperties;
    public ChemicalProperties chemicalProperties;
    public SafetyData safety;
    public BehaviorData behavior;
    public VisualSettings visualSettings;
}

[System.Serializable]
public class PhysicalProperties
{
    public float molarMass;
    public float density;
    public string color;
    public float meltingPoint;
    public float boilingPoint;
    public float solubility;
}

[System.Serializable]
public class ChemicalProperties
{
    public float concentration;
    public float pH;
    public bool isAcid;
    public bool isBase;
    public bool isOxidizing;
    public bool isReducing;
}

[System.Serializable]
public class SafetyData
{
    public string[] hazards;
    public string safetyNotes;
    public bool requiresVentilation;
    public bool requiresGloves;
}

[System.Serializable]
public class BehaviorData
{
    public bool canReact;
    public bool canMix;
    public bool canEvaporate;
    public float evaporationRate;
}

[System.Serializable]
public class VisualSettings
{
    public string materialColor;
    public string particleEffect;
    public string pourSound;
    public string reactionSound;
}

[System.Serializable]
public class StepValidation
{
    public int stepNumber;
    public string stepTitle;
    public bool isValid;
    public List<string> errors;
    public List<string> warnings;
} 