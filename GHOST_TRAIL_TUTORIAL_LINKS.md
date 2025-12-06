# Ghost Trail Effect - Tutorial Resources

Here are some helpful YouTube tutorials for creating ghost trail effects in Unity:

## Trail Renderer Tutorials

1. **Unity Trail Renderer Tutorial**
   - Search: "Unity Trail Renderer tutorial"
   - Trail Renderer is Unity's built-in component for creating trailing effects
   - Good for smooth, continuous trails

2. **Unity Particle System Trails**
   - Search: "Unity particle system trail effect"
   - Particle systems can create more complex, customizable trail effects
   - Good for ethereal, ghostly effects with multiple particles

3. **Unity Shader Graph Trail Effect**
   - Search: "Unity shader graph trail effect"
   - For advanced visual effects with custom shaders
   - Allows for more stylized ghost effects

## Implementation Notes

The `GhostTrail.cs` script I created uses Unity's Trail Renderer component, which is the simplest and most performant option. Here's what you need to do:

### Setup Steps:

1. **In Unity Editor:**
   - Select your Ghost prefab
   - Add the `GhostTrail` component (it will automatically add Trail Renderer)
   - Or manually add Trail Renderer component first, then GhostTrail

2. **Configure the Trail:**
   - The script will auto-setup, but you can customize:
     - `trailMaterial`: Assign a semi-transparent material (white/blue tinted)
     - `startWidth` and `endWidth`: Control trail thickness
     - `time`: How long trail persists
     - `colorGradient`: Fade from visible to transparent

3. **Material Setup:**
   - Create a new Material
   - Use "Unlit/Transparent" shader
   - Set color to white/light blue with alpha around 0.5
   - Assign to `trailMaterial` field

### Recommended YouTube Searches:

- "Unity Trail Renderer tutorial"
- "Unity ghost effect tutorial"
- "Unity particle trail effect"
- "Unity shader graph ghost effect"

The script is already set up to work automatically - just assign a material and adjust the settings in the Inspector!

