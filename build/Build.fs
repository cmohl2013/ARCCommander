﻿open BlackFox.Fake
open Helpers

open BasicTasks
open TestTasks
open PackageTasks
open ReleaseTasks

initializeContext()

// Could also use existing build project template found here: https://github.com/kMutagene/BuildProject

// Make targets accessible
ReleaseNoteTasks.updateReleaseNotes |> ignore
DockerTasks.dockerPublish |> ignore

/// Full release of nuget package, git tag, and documentation for the stable version.
let _release = 
    BuildTask.createEmpty 
        "Release" 
        [clean; build; copyBinaries; runTests; publishBinariesAll]

/// Full release of nuget package, git tag, and documentation for the prerelease version.
let _preRelease = 
    BuildTask.createEmpty 
        "PreRelease" 
        [setPrereleaseTag; clean; build; copyBinaries; runTests; publishBinariesAllPrerelease]

[<EntryPoint>]
let main args = runOrDefault args