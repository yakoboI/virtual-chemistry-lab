# Virtual Chemistry Laboratory

An interactive 3D virtual chemistry laboratory built with Unity and C#, designed to provide students with hands-on experience in conducting chemistry experiments in a safe, controlled environment.

## 🧪 Project Overview

This virtual lab replicates real chemistry laboratory experiences, allowing students to:
- Perform volumetric analysis experiments
- Conduct chemical kinetics studies
- Carry out qualitative analysis procedures
- Work with two-component systems
- Learn laboratory safety protocols
- Practice proper apparatus handling

## 🎯 Technology Stack

- **Engine**: Unity 2022.3 LTS or later
- **Language**: C# (C-Sharp)
- **Platform**: Cross-platform (Windows, macOS, WebGL, Mobile)
- **Data Format**: JSON for experiment definitions and chemical properties

## 📁 Project Structure

```
/PROJECT_VIRTUAL_LABORATORY/
├── 📂 Assets/                    # Main Unity project folder
│   ├── 📂 _Project/             # Custom project files
│   │   ├── 📂 Art/              # Visual assets
│   │   │   ├── 📂 3DModels/     # Equipment models (.fbx, .obj)
│   │   │   ├── 📂 Materials/    # PBR materials and shaders
│   │   │   ├── 📂 Textures/     # Surface textures and UI elements
│   │   │   └── 📂 Audio/        # Sound effects and ambient audio
│   │   ├── 📂 Data/             # JSON data files
│   │   │   ├── 📂 Chemicals/    # Chemical properties and definitions
│   │   │   ├── 📂 Experiments/  # Experiment procedures and parameters
│   │   │   └── 📂 QualitativeAnalysis/ # QAG guide data
│   │   ├── 📂 Prefabs/          # Reusable game objects
│   │   │   ├── 📂 Apparatus/    # Equipment prefabs (burettes, pipettes)
│   │   │   ├── 📂 Environment/  # Lab environment prefabs
│   │   │   └── 📂 UI/           # User interface prefabs
│   │   └── 📂 Scripts/          # C# code files
│   │       ├── 📂 Core/         # Fundamental systems
│   │       ├── 📂 LabEnvironment/ # Laboratory organization and safety
│   │       ├── 📂 Experiments/  # Experiment logic and assessment
│   │       └── 📂 UI/           # User interface controllers
│   ├── 📂 Scenes/               # Unity scenes
│   └── 📂 ThirdParty/           # External assets
├── 📂 ProjectSettings/          # Unity project configuration
└── 📄 README.md                 # This file
```

## 🔬 Core Features

### Laboratory Environment (Section 2.0)
- **Apparatus Management**: Interactive 3D models of laboratory equipment
- **Chemical Handling**: Realistic chemical properties and reactions
- **Safety Protocols**: Built-in safety checks and warnings
- **Storage Systems**: Proper chemical storage and labeling

### Experiment Modules (Section 3.0)
- **Volumetric Analysis**: Acid-base titrations with double indicators
- **Chemical Kinetics**: Concentration and temperature studies
- **Qualitative Analysis**: Systematic identification of ions
- **Two-Component Systems**: Phase diagram experiments

### Assessment System
- **Real-time Feedback**: Immediate validation of experimental procedures
- **Grading Algorithms**: Automated assessment of results
- **Performance Tracking**: Detailed analytics and progress reports

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3 LTS or later
- Basic knowledge of C# programming
- Understanding of chemistry laboratory procedures

### Installation
1. Clone this repository
2. Open the project in Unity
3. Import required assets and dependencies
4. Configure project settings for your target platform

### Development Workflow
1. **Data-Driven Design**: Modify JSON files to update experiments
2. **Modular Architecture**: Add new experiment types by extending base classes
3. **Component-Based**: Reuse apparatus prefabs across different experiments

## 📊 Data Structure

### Chemical Definition (JSON)
```json
{
  "id": "na2co3",
  "name": "Sodium Carbonate",
  "formula": "Na₂CO₃",
  "molarMass": 105.99,
  "concentration": 0.1,
  "hazards": ["irritant"],
  "color": "#FFFFFF",
  "density": 2.54
}
```

### Experiment Definition (JSON)
```json
{
  "id": "3.1.2_double_indicator",
  "title": "Volumetric Analysis with Double Indicator",
  "requiredChemicals": ["na2co3", "hcl"],
  "requiredApparatus": ["burette", "pipette", "conical_flask"],
  "steps": [...],
  "assessment": {
    "expectedTitre": 25.0,
    "tolerance": 0.5,
    "maxScore": 100
  }
}
```

## 🎨 UI/UX Design

- **Modern Interface**: Clean, intuitive design following material design principles
- **Responsive Layout**: Adapts to different screen sizes and orientations
- **Accessibility**: High contrast modes and screen reader support
- **Multilingual**: Support for multiple languages

## 🔒 Safety Features

- **Real-time Validation**: Checks for unsafe combinations and procedures
- **Warning System**: Visual and audio alerts for potential hazards
- **Emergency Procedures**: Built-in safety protocols and emergency responses
- **Training Mode**: Guided tutorials for proper laboratory techniques

## 📈 Assessment and Analytics

- **Performance Tracking**: Detailed logs of student interactions
- **Error Analysis**: Identification of common mistakes and misconceptions
- **Progress Reports**: Comprehensive feedback on experimental skills
- **Competency Mapping**: Alignment with curriculum objectives

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Implement your changes following the established architecture
4. Add comprehensive tests for new functionality
5. Submit a pull request with detailed documentation

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For technical support or questions about the virtual laboratory:
- Create an issue in the GitHub repository
- Contact the development team
- Refer to the documentation in the `/docs` folder

## 🔮 Future Enhancements

- **VR/AR Support**: Immersive virtual reality experiences
- **Multiplayer Mode**: Collaborative laboratory sessions
- **Advanced Simulations**: Complex reaction mechanisms
- **AI Integration**: Intelligent tutoring and adaptive learning
- **Mobile Optimization**: Enhanced mobile device support

---

**Built with ❤️ for chemistry education** 