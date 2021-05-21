# ROUNDS Mod-Loader

This is a **simple** mod loader for Rounds. It includes a basic wrapper for custom networked events and a framework for adding new Cards.

# Requirements
 - HarmonyLib ([github](https://github.com/pardeike/Harmony))
 - Manually modifying the game's Assembly-CSharp.dll to inject the mod-loader

# Installation
 1. Download the mod-loader from [releases](https://github.com/willis81808/rounds-mod-loader/releases)
 2. Extract the RoundsModLoader.dll into "ROUNDS\Rounds_Data\Managed"
 3. Download HarmonyLib (link above)
 4. Extract 0Harmony.dll (.NET 4.5 version) into "ROUNDS\Rounds_Data\Managed"
 5. If the folder doesn't already exist, create a folder titled "Mods" in "ROUNDS\Rounds_Data"
 5. Drop any mods into the mods folder
 6. Finally, manually patch the Assembly-CSharp.dll file (I recommend using dnSpy) as follows:

Inject the following code-snippet into the "Update" method of the "PlayerManager" class.

	if (!ModLoader.IsInitialized())
	{
		ModLoader.Initialize();
	}

Add the "RoundsModLoader.dll" reference to the assembly with the button shown here:
![dnSpy preview](https://i.imgur.com/yhfza4E.png)

# MDK Usage
To create mods compatible with this mod-loader create a "Class Library (.NET Framework)" VS project, add RoundsModLoader.dll to references. Then you'll create your mod's entry point.

For Example:

    using RoundsMDK;
    using UnityEngine;
    
    public class EntryPoint : IMod
    {
        private static readonly string ModName = "Cursor Fix";
    
        public string Initialize()
        {
            // Confine cursor to game window
            Cursor.lockState = CursorLockMode.Confined;
    
            return ModName;
        }
    }

Build the project in release mode (be sure to target .NET Framework 4.6.1 or higher). Drop it into the "ROUNDS\Rounds_Data\Mods" folder. It will be automatically initialized on the game's startup and display a popup with the returned *ModName*.

# Mod Loader Utilities
The mod loader includes (primarily) two helper utilities you should take note of. The **NetworkingManager** and the **CustomCard** classes.

The **NetworkingManager** abstracts the default Photon networking capabilities away into an easy-to-use interface you can use for communication between clients.
Example usage:
	
	private const string MessageEvent = "YourMod_MessageEvent";

	NetworkingManager.RegisterEvent(MessageEvent, (data) => {
	  ModLoader.BuildInfoPopup("Test Event Message: " + (string)data[0]);    // should print "Test Event Message: Hello World!"
	});

	// send event to other clients only
	NetworkingManager.RaiseEventOthers(MessageEvent, "Hello World!");
	
	// send event to other clients AND yourself
	NetworkingManager.RaiseEvent(MessageEvent, "Hello World!");
