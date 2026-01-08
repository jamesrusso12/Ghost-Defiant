# Ambient audio (Option A / Resources auto-load)

You can keep your audio organized under `Assets/Audio/` — **Option A only requires that the clips live under a folder named `Resources` somewhere inside `Assets/`.**

This folder is already in the correct place for auto-loading:

- `Assets/Audio/Resources/Audio/Ambient/`

## Put your clips here (filenames matter)

Place your files here:

- `Assets/Audio/Resources/Audio/Ambient/AmbientMusic.wav` (or `.ogg`)
- `Assets/Audio/Resources/Audio/Ambient/RoomTone.wav` (or `.ogg`)

The script loads them via these `Resources` paths:

- `Audio/Ambient/AmbientMusic`
- `Audio/Ambient/RoomTone`

So the clip filenames must be exactly:

- **AmbientMusic**
- **RoomTone**

## Import settings (quick recommendations)

- **Loop**: On (both)
- **Load Type**:
  - AmbientMusic: Streaming (if it's long)
  - RoomTone: Compressed In Memory (small loops)

If `RoomTone` is missing, the game generates a subtle procedural room tone automatically.


