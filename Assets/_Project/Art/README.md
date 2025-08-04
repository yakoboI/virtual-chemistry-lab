# Virtual Chemistry Lab - Art Assets

This directory contains all the art assets for the Virtual Chemistry Laboratory application, including 3D models, materials, textures, and audio files.

## Directory Structure

```
Art/
├── 3DModels/          # 3D model files (.fbx)
├── Materials/         # Material files (.mat)
├── Textures/          # Texture files (.png)
├── Audio/            # Audio files (.wav)
├── ArtAssetManager.cs # Asset management system
├── ModelConfig.json   # 3D model configuration
├── MaterialConfig.json # Material configuration
├── TextureConfig.json # Texture configuration
├── AudioConfig.json   # Audio configuration
└── README.md         # This file
```

## 3D Models

### Laboratory Glassware
- **Beaker.fbx** - Standard laboratory beaker (100ml capacity)
- **Flask.fbx** - Erlenmeyer flask (250ml capacity)
- **Burette.fbx** - Precision burette for titrations (50ml capacity)
- **Pipette.fbx** - Precision pipette for liquid transfer (10ml capacity)
- **TestTube.fbx** - Standard test tube (15ml capacity)

### Support Equipment
- **RingStand.fbx** - Adjustable ring stand for supporting equipment
- **Clamp.fbx** - Adjustable clamp for securing equipment

### Model Properties
All 3D models include:
- Detailed geometry with UV mapping
- Collision meshes for interaction
- LOD (Level of Detail) levels for performance
- Proper scaling and positioning
- Animation support where applicable

## Materials

### Laboratory Materials
- **Glass.mat** - Transparent glass material with high reflectivity
- **Metal.mat** - Metallic material for equipment stands and clamps
- **Liquid.mat** - Transparent liquid material with fluid properties
- **Rubber.mat** - Matte rubber material for stoppers and padding
- **Plastic.mat** - Semi-glossy plastic material for containers

### Material Properties
- Proper shader assignments
- Transparency settings where needed
- Metallic and smoothness values
- Color and emission settings
- Normal map support

## Textures

### Surface Textures
- **GlassTexture.png** - Transparent glass surface with reflections
- **MetalTexture.png** - Metallic surface with industrial finish
- **LiquidTexture.png** - Liquid surface with transparency
- **RubberTexture.png** - Matte rubber surface
- **PlasticTexture.png** - Semi-glossy plastic surface

### Texture Properties
- 512x512 resolution for optimal performance
- Proper color space settings
- Mip map generation
- Compression settings
- UV mapping coordinates

## Audio Files

### Laboratory Equipment Sounds
- **glass_clink.wav** - Glass equipment placement/movement
- **liquid_pour.wav** - Liquid pouring between containers
- **burette_drop.wav** - Single drop from burette
- **endpoint_reached.wav** - Titration endpoint detection
- **indicator_change.wav** - pH indicator color change

### Safety Equipment Sounds
- **emergency_alarm.wav** - Emergency alarm for safety violations
- **ventilation_fan.wav** - Ventilation system background sound
- **fire_suppression.wav** - Fire suppression system activation

### Assessment Sounds
- **assessment_complete.wav** - Experiment assessment completion
- **score_update.wav** - Score update during assessment

### Audio Properties
- High-quality 44.1kHz sample rate
- 16-bit depth for clarity
- Stereo channels for spatial audio
- Proper volume and priority settings
- 3D spatial blending for immersion

## Asset Management

### ArtAssetManager.cs
The `ArtAssetManager` script provides:
- Centralized asset loading and caching
- Memory management for large assets
- Asset pooling for performance
- Dynamic loading/unloading
- Asset validation and error handling

### Configuration Files
- **ModelConfig.json** - 3D model metadata and settings
- **MaterialConfig.json** - Material properties and shader settings
- **TextureConfig.json** - Texture import and compression settings
- **AudioConfig.json** - Audio import and playback settings

## Usage Guidelines

### Performance Considerations
- Use LOD levels for distant objects
- Implement object pooling for frequently used assets
- Compress textures appropriately for target platform
- Stream audio files for large sound effects

### Asset Organization
- Keep related assets in appropriate subdirectories
- Use consistent naming conventions
- Maintain proper file references in configuration
- Document any special requirements or dependencies

### Quality Standards
- All 3D models should be optimized for real-time rendering
- Materials should use appropriate shaders for the target platform
- Textures should be properly sized and compressed
- Audio files should be normalized and properly formatted

## Integration with Game Systems

### Audio Integration
- Audio files are referenced by the `AudioManager` system
- Spatial audio is supported for 3D positioning
- Volume and priority settings are configurable
- Looping and one-shot playback modes available

### Material Integration
- Materials are automatically applied to 3D models
- Dynamic material switching is supported
- Transparency and blending modes are configurable
- Normal maps and other effects are supported

### Model Integration
- Models are loaded through the `ArtAssetManager`
- Collision detection is automatically set up
- Animation systems are integrated where needed
- LOD switching is handled automatically

## Future Enhancements

### Planned Additions
- Additional laboratory equipment models
- Procedural texture generation
- Dynamic material effects
- Advanced audio spatialization
- Asset streaming for large environments

### Optimization Opportunities
- Texture atlasing for better performance
- Mesh instancing for repeated objects
- Audio compression for mobile platforms
- Asset bundling for faster loading

## Support and Maintenance

### Asset Updates
- Regular review and optimization of existing assets
- Addition of new assets as needed
- Performance monitoring and improvements
- Quality assurance testing

### Documentation
- Keep this README updated with new assets
- Document any special requirements or dependencies
- Maintain asset version control
- Provide usage examples and best practices

---

For questions or issues regarding art assets, please refer to the main project documentation or contact the development team. 