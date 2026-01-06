Examples:
	contains a selection of scenes demoing various features
	 

	Simple Scene.unity:
	Contains an simple example scene that:
		1) Contains the requried AdinmoManager in the root of the scene
		2) Applies an AdinmoTexture to an image that will be replaced with an ad
		3) Includes an ExampleCallbacks.cs script that shows examples of how the callbacks can be used to notify that game when textures have been applied.
	
	2D Scene.unity:
	Contains a example 2D scroller game that:
		1) uses sprites to show the ads in the level
		2) uses a image to show a landscape ad in the pause menu

	The Built In folder contains 3D demos using the Built In render pipeline

	3D Scene.unity:
	Contains a sample run around scene that demos various features, including:
		1) A Square placement with a calculated border. When the Ad is a different aspect ratio to the placement, it will fill the background with a calculated colour matching the Ad's border as best it can.
		2) A Square placement with a fixed border. When the Ad is a different aspect ratio to the placement, a fixed blue colour will be used as a background.
		3) A Square placement with a Shrink To Fit border. 
			a) When the Ad is a different aspect ratio to the placement, the aspect ratio of the placement is adjusted to match.
			b) On the placement, a parent object is also defined. This results in the parent object's aspect ratio being adjusted to match the Ad's aspect ratio.
			C) There is a green transparent glass infront of the object with it's layer set to Ignore Raycast. By setting AdinmoManager Object's Click Ignores Layers property to Ignore Raycast, it allows user clicks to ignore the glass and click on the placement.
			   Also, The Occlusion Test Ignores Layers is set to Ignore Layers too, this means the visibility test for the placement can ignore the glass, allowing normal impression recording to occur.
		4) 2 TV shaped placments, one with a lit shader and another with an unlit shader. It is recommended that you use the shaders as shown in this demo, to allow the click emmission border to work correctly.

	IAP demo.unity:
	Contains a placement that regularly received a "Chest of Gold" example IAP, and a "Chest of Gold" with a cross through.
	If you are not using the Unity In App Purchasing package, it will use a fake store (defined in Assets/Adinmo/examples/scripts/StoreHandler_Fake.cs) to simulate the purchase of an IAP
	if you are using Unity In App Purchasing package 4, it will use a store designed to work with that package (defined in Assets/Adinmo/examples/scripts/StoreHandler.cs) to simulate the purchase of the IAP
	if you are using Unity In App Purchasing package 5, it will use a store designed to work with that package (defined in Assets/Adinmo/examples/scripts/StoreHandler_V5.cs) to simulate the purchase of the IAP
	This is the recommended way for interacting with IAP's. This script can be copied and adapted for your own usage.

	Sound.unity:
	Contains a AudioPlacement, showing how to integrate audio ads into your game

	The URP folder contains the same 3D demos, but using the Universal Render Pipeline
			  
Plugins:
	AdinmoPlugin.dll -- a small pure .net plugin that handles all communication with the Adinmo Servers

Prefabs:
	AdinmoManager.prefab must be populated in the root of your scene. You need to add your game key obtained from our website.

Scripts:
	AdinmoTexture.cs is a component that you will add to textures that need to be replaced,  It needs to conain a placement key obtained from our website.

Resources:
	Contains prefabs and textures uesed for debugging placement sizes


