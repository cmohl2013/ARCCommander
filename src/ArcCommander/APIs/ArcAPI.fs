﻿namespace ArcCommander.APIs

open System
open System.IO

open ArcCommander
open ArcCommander.CLIArguments

open ARCtrl
open ARCtrl.NET
open ARCtrl.ISA

[<AutoOpen>]
module ARCExtensions = 
    type ARC with
        member this.Write(arcPath) =
            this.GetWriteContracts()
            |> Array.iter (Contract.fulfillWriteContract arcPath)

module API =
    
    module ARC = 
        
        let getProcesses (arc : ARC) =
            arc.ISA.Value.ToInvestigation().Studies 
            |> Option.defaultValue [] |> List.collect (fun s -> 
                s.Assays
                |> Option.defaultValue [] |> List.collect (fun a -> 
                    a.ProcessSequence |> Option.defaultValue []
                )
            )


/// ArcCommander API functions that get executed by top level subcommand verbs.
module ArcAPI = 

    let version _ =
        
        let log = Logging.createLogger "ArcVersionLog"

        log.Info($"Start Arc Version")
        
        let ver = Reflection.Assembly.GetExecutingAssembly().GetName().Version
        
        log.Debug($"v{ver.Major}.{ver.Minor}.{ver.Build}")

    /// Initializes the ARC-specific folder structure.
    let init (arcConfiguration : ArcConfiguration) (arcArgs : ArcParseResults<ArcInitArgs>) =

        let log = Logging.createLogger "ArcInitLog"
        
        log.Info("Start Arc Init")

        let workDir = GeneralConfiguration.getWorkDirectory arcConfiguration

        let editor              = arcArgs.TryGetFieldValue ArcInitArgs.EditorPath
        let gitLFSThreshold     = arcArgs.TryGetFieldValue ArcInitArgs.GitLFSByteThreshold 
        let branch              = arcArgs.TryGetFieldValue ArcInitArgs.Branch
        let repositoryAddress   = arcArgs.TryGetFieldValue ArcInitArgs.RepositoryAddress 
        let identifier =    
            arcArgs.TryGetFieldValue ArcInitArgs.Identifier
            |> Option.defaultValue (DirectoryInfo(workDir).Name)       

        log.Trace("Create Directory")

        Directory.CreateDirectory workDir |> ignore

        log.Trace("Initiate folder structure")

        let isa = ArcInvestigation.create(identifier)
        ARC(isa).Write(workDir)     

        log.Trace("Set configuration")

        match editor with
        | Some editorValue -> 
            let path = IniData.getLocalConfigPath workDir
            IniData.setValueInIniPath path "general.editor" editorValue
        | None -> ()

        match gitLFSThreshold with
        | Some gitLFSThresholdValue -> 
            let path = IniData.getLocalConfigPath workDir
            IniData.setValueInIniPath path "general.gitlfsbytethreshold" gitLFSThresholdValue
        | None -> ()

        log.Trace("Init Git repository")

        try

            GitHelper.executeGitCommand workDir $"init -b {branch}"

            if arcArgs.ContainsFlag ArcInitArgs.Gitignore then
                log.Warn("The default GitIgnore is an experimental feature. Be careful and double check that all your wanted files are being tracked.")
                let gitignoreAppPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "defaultGitignore")
                let gitignoreArcPath = Path.Combine(workDir, ".gitignore")
                log.Trace($"Copy .gitignore from {gitignoreAppPath} to {gitignoreArcPath}")
                File.Copy(gitignoreAppPath, gitignoreArcPath)

            log.Trace("Add remote repository")
            match repositoryAddress with
            | None -> ()
            | Some remote ->
                GitHelper.executeGitCommand workDir $"remote add origin {remote}"
                //GitHelper.executeGitCommand workDir $"branch -u origin/{branch} {branch}"

        with 
        | e -> 

            log.Error($"Git could not be set up. Please try installing Git cli and run `arc git init`.\n\t{e}")

    /// Update the investigation file with the information from the other files and folders.
    let update (arcConfiguration : ArcConfiguration) =

        let log = Logging.createLogger "ArcUpdateLog"
        
        log.Info("Start Arc Update")

        ARC.load(arcConfiguration).Write(arcConfiguration)

    /// Export the complete ARC as a JSON object.
    let export (arcConfiguration : ArcConfiguration) (arcArgs : ArcParseResults<ArcExportArgs>) =
    
        let log = Logging.createLogger "ArcExportLog"

        log.Info("Start Arc Export")
       
        let workDir = GeneralConfiguration.getWorkDirectory arcConfiguration
                    
        let arc = ARC.load workDir

        let output =
            if arcArgs.ContainsFlag ProcessSequence then
                API.ARC.getProcesses arc
                |> ARCtrl.ISA.Json.ProcessSequence.toString
            else 
                arc.ISA.Value
                |> ARCtrl.ISA.Json.ArcInvestigation.toString

        match arcArgs.TryGetFieldValue Output with
        | Some p -> System.IO.File.WriteAllText(p, output)
        | None -> ()

        log.Debug(output)
        
    /// Convert the complete ARC to a target format.
    let convert (arcConfiguration : ArcConfiguration) (arcArgs : ArcParseResults<ArcConvertArgs>) =
    
        let log = Logging.createLogger "ArcConvertLog"

        log.Fatal("Convert command currently disabled.")

        //log.Info("Start Arc Convert")
       
        //let workDir = GeneralConfiguration.getWorkDirectory arcConfiguration

        //let nameRoot = getFieldValueByName "Target" arcArgs
        //let converterName = $"arc-convert-{nameRoot}"
        //let repoOwner =  "nfdi4plants"
        //let repoName = "converters"

        //log.Info (converterName)

        //let assayIdentifier = tryGetFieldValueByName "AssayIdentifier" arcArgs
        //let studyIdentifier = tryGetFieldValueByName "StudyIdentifier" arcArgs

        //log.Info("Fetch converter dll")
       
        //let assembly = 
        //    if nameRoot.Contains ".dll" then
        //        System.Reflection.Assembly.LoadFile nameRoot
        //    else
        //        let dll = ArcConversion.getDll repoOwner repoName $"{converterName}.dll"
        //        System.Reflection.Assembly.Load dll
        //let converter = 
        //    ArcConversion.callMethodOfAssembly converterName "create" assembly :?> ARCconverter
        //log.Info("Load ARC")

        //let i,s,a = ArcConversion.getISA studyIdentifier assayIdentifier workDir

        //log.Info("Run conversion")
           
        //match converter with
        //| ARCtoCSV f -> 
        //    ArcConversion.handleCSV i s a workDir nameRoot converter
        //| ARCtoTSV f -> 
        //    ArcConversion.handleTSV i s a workDir nameRoot converter
        //| ARCtoXLSX f -> 
        //    ArcConversion.handleXLSX i s a workDir nameRoot converter
        //| ARCtoJSON f -> 
        //    ArcConversion.handleJSON i s a workDir nameRoot converter
        //| _ -> failwith "no other converter defined"
        //|> function 
        //   | Ok messages ->
        //        log.Info $"Successfully converted to {nameRoot}"
        //        ArcConversion.writeMessages workDir messages
        //   | Error messages ->
        //        ArcConversion.writeMessages workDir messages
        //        log.Error $"Arc could not be converted to {nameRoot}, as some required values could not be retreived"
        //        if ArcConversion.promptYesNo "Do you want missing fields to be written back into ARC? (y/n)" then
        //            ArcConversion.handleTransformations workDir converterName messages