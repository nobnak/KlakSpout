# KlakSpout Submodule (for Unity)
## Feature
- Spout's sender texture doesn't depend on Camera (Screen) resolution
- Resizable texture size
- UnityEvent-based
- Support multiple cameras

## Usage
### Import this as submodule in your project:
```
git submodule add https://github.com/nobnak/Gist.git Assets/Packages/Gist
git submodule add https://github.com/nobnak/KlakSpout.git Assets/Packages/KlakSpout
```

### Spout Sender
 - Attach SpoutSender script to a GameObject
   - Set Camera.targetTexture as "Event on update texture" event's target
   - Set name and size of Spout texture
