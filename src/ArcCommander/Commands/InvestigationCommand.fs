﻿namespace ArcCommander.Commands

open Argu 
open ArcCommander.CLIArguments
open InvestigationContacts
open InvestigationPublications


type InvestigationCommand = 
    
    | [<CliPrefix(CliPrefix.None)>]                     Create      of create_args          : ParseResults<InvestigationCreateArgs>
    | [<CliPrefix(CliPrefix.None)>]                     Update      of update_args          : ParseResults<InvestigationUpdateArgs>
    | [<CliPrefix(CliPrefix.None)>] [<SubCommand()>]    Edit 
    //| [<CliPrefix(CliPrefix.None)>]                     Delete      of delete_args          : ParseResults<InvestigationDeleteArgs>
    | [<CliPrefix(CliPrefix.None)>]                     Person      of person_verbs         : ParseResults<InvestigationPersonCommand>
    | [<CliPrefix(CliPrefix.None)>]                     Publication of publication_verbs    : ParseResults<InvestigationPublicationCommand>
    | [<CliPrefix(CliPrefix.None)>] [<SubCommand()>]    Show

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Create        _ -> InvestigationCreateArgs.deprecationWarning
            | Update        _ -> "Update the ARC's investigation with the given Metdadata"
            | Edit          _ -> "Open an editor window to directly edit the ARC's investigation file"
            //| Delete        _ -> "Delete the ARC's investigation file (DANGER ZONE!)"
            | Person        _ -> "Person functions"
            | Publication   _ -> "Publication functions"
            | Show          _ -> "Get the values of the ARC's investigation"

and InvestigationPersonCommand =

    | [<CliPrefix(CliPrefix.None)>]                     Update      of update_args      : ParseResults<PersonUpdateArgs>
    | [<CliPrefix(CliPrefix.None)>]                     Edit        of edit_args        : ParseResults<PersonEditArgs>
    | [<CliPrefix(CliPrefix.None)>]                     Register    of register_args    : ParseResults<PersonRegisterArgs>
    | [<CliPrefix(CliPrefix.None)>]                     Unregister  of unregister_args  : ParseResults<PersonUnregisterArgs>
    | [<CliPrefix(CliPrefix.None)>]                     Show        of show_args        : ParseResults<PersonShowArgs>
    | [<CliPrefix(CliPrefix.None)>] [<SubCommand()>]    List

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Update        _ -> "Update an existing person in the ARC investigation with the given person metadata. The person is identified by the full name (first name, last name, mid initials)."
            | Edit          _ -> "Open and edit an existing person in the ARC investigation with a text editor. The person is identified by the full name (first name, last name, mid initials)."
            | Register      _ -> "Register a person in the ARC investigation with the given assay metadata"
            | Unregister    _ -> "Unregister a person from the given investigation. The person is identified by the full name (first name, last name, mid initials)."
            | Show          _ -> "Get the metadata of a person registered in the ARC investigation"
            | List          _ -> "List all persons registered in the ARC investigation"

and InvestigationPublicationCommand =

    | [<CliPrefix(CliPrefix.None)>]                     Update      of update_args      : ParseResults<PublicationUpdateArgs>
    | [<CliPrefix(CliPrefix.None)>]                     Edit        of edit_args        : ParseResults<PublicationEditArgs>
    | [<CliPrefix(CliPrefix.None)>]                     Register    of register_args    : ParseResults<PublicationRegisterArgs>
    | [<CliPrefix(CliPrefix.None)>]                     Unregister  of unregister_args  : ParseResults<PublicationUnregisterArgs>
    | [<CliPrefix(CliPrefix.None)>]                     Show        of show_args        : ParseResults<PublicationShowArgs>
    | [<CliPrefix(CliPrefix.None)>] [<SubCommand()>]    List

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Update        _ -> "Update an existing publication in the ARC investigation with the given publication metadata. The publication is identified by the DOI."
            | Edit          _ -> "Open and edit an existing publication in the ARC investigation with a text editor. The publication is identified by the DOI."
            | Register      _ -> "Register a publication in the ARC investigation with the given assay metadata"
            | Unregister    _ -> "Unregister a publication from the given investigation. The publication is identified by the DOI."
            | Show          _ -> "Get the metadata of a publication registered in the ARC investigation"
            | List          _ -> "List all publication registered in the ARC investigation"
