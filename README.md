# Domino Simulator

### CS 130 Project for F'18 with Professor Kim.

Team: Robin Zhang, Sinan Cetin, Gladys Ng, Manas Kumar, Brandon Hua

Built with Unity 2018.2.14f1 (64-bit).

### Directory Structure
```
Important folders & subfolders:
- Assets: game's code and imported packages
    - BH: 99% of game code. Represents the BH namespace that our game is in
        - Scripts: our C# game scripts, i.e. 99% of game code
            - We arbitrarily created subfolders here for organization
        - Scenes: Unity game scenes. Our game just uses the SpectatorMode scene
    - Editor: stores the auto-doc Doxygen tool
    - PlayModeTests: our C# test scripts
    - Plugins: SQLite for database integration
    - LWRP, TextMesh Pro, Gizmos: imported packages to make game look nicer. Didn't use yet, as we explained in the Part B report
- Blender: custom domino models. Currently not integrated with the game
- Docs: our Doxygen-generated documentation

Unimportant folders:
- Packages: we won't add packages here. We'll add them inside /Assets
```

### Tests
We tested using Unity's built-in Test Runner. All tests are located in the .cs files in 3 subfolders in the [Assets/PlayModeTests](Assets/PlayModeTests) folder. The 3 subfolders are: [IntegrationTests](Assets/PlayModeTests/IntegrationTests), [RenderingTests](Assets/PlayModeTests/RenderingTests), and [UnitTests](Assets/PlayModeTests/UnitTests). Each folder contains .cs files, where each .cs file is a class containing test cases. (Ignore the .meta files in the same folder.) The most complex tests are the domino manipulation tests in [DominoManipulation.cs](Assets/PlayModeTests/IntegrationTests/DominoManipulation.cs).

#### Test Overview
Each test loads the initial state of the simulator as a scene, with all important components already instantiated in the scene. The tests can access and manipulate these components to perform a test, usually in the form of an assert. Each test is a function marked with [UnityTest] before its declaration.

#### Test example
As requested, here's an in-depth explanation of one test (updated for Part C). The comments describe each step. This test is located in [DominoManipulation.cs](Assets/PlayModeTests/IntegrationTests/DominoManipulation.cs).
```
/// Test goal: User should click a button to add a domino that's tracked by the SelectableManager class.
[UnityTest]
public IEnumerator _Adds_New_Domino_UI()
{
    // Wait for scene loading in the [SetUp] function to finish.
    yield return new WaitForFixedUpdate();

    // Time for the real test! 
    
    // First, check that the scene started with no dominos.
    // We'll find and save the backend "domino manager" component that stores the dominos currently in the simulator.
    SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();

    // dominoManager is the class instance of the component we wanted. Check that dominoManager's list of dominos is empty.
    Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

    // Now add a domino by simulating a click on the Spawn button to enable spawning mode,
    // and a click on the screen to place the domino.
    // The simulations call helper functions in the same file.
    // Note the yield statement, which gives time for game components to process the clicks.
    Utility.ClickUIButton("ButtonSpawn");
    yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f,Screen.height/2f,0));

    // Check that a new domino has been inserted into dominoManager's list of dominos.
    Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 1);
}
```

#### Running Tests
Download the repository and open it in Unity. Open the Test Runner window by navigating to Window/General/Test Runner on the Unity toolbar. The window should automatically be linked to the existing tests. You can run them using the Test Runner's GUI.

### Documentation
We generated documentation with a Unity Doxygen plugin. The home page of our documentation is [here](Docs/html/annotated.html). All the significant application code is under the BH namespace which is listed [here](Docs/html/namespace_b_h.html). All documentation is located in the [/Docs/html](Docs/html) folder.

#### Documention tagging overview
We followed the Doxygen XML format to comment all classes and functions. Below is the format we followed.
```
/// <summary>
/// [our summary of function Foo]
/// </summary>
/// <returns>
/// [our description of what Foo returns]
/// </returns>
/// <param name="[name of Foo's parameter]"> [our description of Foo's parameter] </param>
int Foo (int param1)
{
    ...
}
```
We used the same format for classes and functions.

### Notes
We were using Unity Collab, but stopped halfway through because there were some problems.
