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
We tested using Unity's built-in Test Runner. All tests are located in the .cs files in the [Assets/PlayModeTests](Assets/PlayModeTests) folder. There are currently 2 .cs files with tests: [DominoManipulation.cs](Assets/PlayModeTests/DominoManipulation.cs) and [SceneInitCorrect.cs](Assets/PlayModeTests/SceneInitCorrect.cs).

#### Test Overview
Each test loads the initial state of the simulator as a scene, with all important components already instantiated in the scene. The tests can access and manipulate these components to perform a test, usually in the form of an assert. Each test is a function marked with [UnityTest] before its declaration.

#### Test example
As requested, here's an in-depth explanation of one test. The comments describe each step. This test is located in [DominoManipulation.cs](Assets/PlayModeTests/DominoManipulation.cs).
```
/// Test goal: User should click a button to add a domino that's tracked by the SelectableManager class.
[UnityTest]
public IEnumerator _Adds_New_Domino()
{
    // Load initial state of simulator, including component instantiations.
    SceneManager.LoadScene("SpectatorMode");
    // Loading completes on the next frame update, so wait until then.
    yield return new WaitForFixedUpdate();

    // Time for the real test! 
    
    // First, check that the scene started with no dominos.
    // We'll find and save the backend "domino manager" component that stores the dominos currently in the simulator. 
    SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
    // dominoManager is the class instance of the component we wanted. Check that dominoManager's list of dominos is empty.
    Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

    // Calls a local helper function in the same file to simulate the user clicking the UI Add Button.
    // (This helper finds the button component the same way we found dominoManager.)
    ClickUIButton("ButtonAdd");

    // Check that a new domino has been inserted into dominoManager's list of dominos.
    Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 1);
}
```
To Shaghayegh: the Part B report has summaries of the other test cases!

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
We will primarily be using Unity Collab for version control, while backing up our work onto this Github repository occasionally.
