This is a file describing the stuff that I (Brandon) imported. This also
describes all the steps I took in doing so.

//////////////
// PACKAGES //
//////////////

(Starting from an empty project in Unity 2018.2.14)

Window -> Package Manager -> All -> ...
    1.  Postprocessing -> Install
    2.  Shader Graph -> Install
    3.  Render-pipelines.lightweight -> Install
    4.  Cinemachine -> Install
    
Why I imported those:
    1.  Postprocessing: eye-candy (but at the cost of performance)
    2.  Shader Graph: cool effects without having to write actual shader code
    3.  Render-pipelines.lightweight: required by Shader Graph; also better
        lighting IMO
    4.  Cinemachine: will be useful if we do implement an auto-camera feature
    
Setup/guides for later
    Postprocessing:  https://github.com/Unity-Technologies/PostProcessing/wiki/Quick-start
    Shader Graph + render pipeline:  https://www.youtube.com/watch?v=CKhK2aaUA5U
    
I then created lightweight render pipeline (LWRP) asset "LWRPAsset" under
"Assets/LWRP" and then assigned it in the Graphics settings:
    1)  Edit -> Project Settings -> Graphics -> dragged "LWRPAsset" into the
        "Scriptable Render Pipeline Settings" box
    2)  Edit -> Render Pipeline -> Upgrade Project Materials to LightWeight
        Materials
        
A consequence of using LWRP is that creating materials is slightly
different. I believe you cannot use standard shaders anymore, so you have to
use the shaders under the "LightweightPipeline" menu instead. Hopefully this
won't be an issue!

////////////
// ASSETS //
////////////

Because they are really big, I've uploaded .zips of sound packs on the Google
Drive instead of importing them into the project directly. They are located
under the "Unity-related" folder.

///////////
// OTHER //
///////////

I've imported all the things I was working on in the folder "Assets/BH". I've
been working in the scene "Assets/BH/Scenes/Testing" if any of you want to see
or review/judge how I've set stuff up. Note: any folder other than "BH" was
generated while I was importing stuff or was already there.

Somewhere during the process, I was prompted to import "TextMesh Pro" so I
clicked yes to import.
