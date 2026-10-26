# Unity setup: unsaved Untitled scene

User evidence: Unity2022.3.50f1 Windows/DX11, fresh Untitled scene, menu Schlafwald/1.Projekt einrichten. Error: InvalidOperationException: Cannot create a new scene additively with an untitled scene unsaved.

ProjectSetup.CreateMainScene created Main additively without checking loaded unnamed scenes. The default scene can be clean but still have no saved path. Run now checks loaded scene paths before changing project settings and uses Unity's save dialog. Cancelling returns before setup changes. Existing Main and authored scenes remain untouched. Batch mode fails explicitly if a loaded unnamed scene prevents safe creation; no user scene is discarded. Failed Main saves are no longer followed by closing the scene.

Immediate workaround in an already downloaded copy: Ctrl+S, save the current empty scene under Assets as Startszene.unity (new filename), then rerun setup. Open Assets/Schlafwald/Scenes/Main.unity after successful setup, then Play.

Manual verification required in user's Editor: (1) fresh clean Untitled -> setup asks to save; (2) cancel -> no generated settings/Main; (3) save -> setup completes; (4) repeat with existing Main -> authored contents remain; (5) Play -> inspect Console and actual rendering. Repository diff check passed. This cloud environment still has no executable activated Unity Editor; compilation/EditMode/PlayMode/rendering for this change were not run here.
