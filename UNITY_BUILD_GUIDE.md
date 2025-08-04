# 🎮 Unity WebGL Build Guide

## **Create Interactive Web Version**

### **Prerequisites:**
- Unity 2022.3 LTS installed
- WebGL build support module

### **Build Steps:**

1. **Open Unity Hub**
2. **Add Project**: Select your virtual-chemistry-lab folder
3. **Open Project** in Unity Editor

4. **Build Settings**:
   - File → Build Settings
   - Platform: WebGL
   - Click "Switch Platform"

5. **Add Scenes**:
   - Add Open Scenes
   - Add all scenes from Assets/Scenes/
   - Set MainMenu.unity as first scene

6. **Player Settings**:
   - Resolution: 1920x1080
   - WebGL Memory: 512MB
   - Compression: Disabled (for faster loading)

7. **Build**:
   - Click "Build"
   - Choose folder: `Builds/WebGL/`
   - Wait for build completion

8. **Deploy**:
   - Upload Builds/WebGL/ contents to GitHub Pages
   - Or drag to Netlify for instant hosting

### **Alternative: Quick Build**
```bash
# If you have Unity CLI installed
unity-builder build --platform WebGL --projectPath . --buildPath Builds/WebGL/
```

---
**Status**: ⏳ Ready to build
**Next**: Install Unity and create WebGL build 