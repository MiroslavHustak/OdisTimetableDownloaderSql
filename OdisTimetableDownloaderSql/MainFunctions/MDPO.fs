module WebScraping_MDPO

open System
open System.IO

open MDPO_Submain
open SettingsGeneral

open Types.Messages
open Types.DirNames
open Messages.Messages

//open Messages.MessagesMocking

open ErrorHandling.TryWithRF

//************************Main code*******************************************************************************

type private State =  //not used
    { 
        TimetablesDownloadedAndSaved: unit
    }

let private stateDefault = 
    {          
        TimetablesDownloadedAndSaved = ()
    }

type private Actions =
    | StartProcess
    | DeleteOneODISDirectory
    | CreateFolders
    | FilterDownloadSave    
    | EndProcess

type private Environment = 
    {
        filterTimetables: string -> Messages -> Map<string, string>
        downloadAndSaveTimetables: Messages -> string -> Map<string, string> -> unit
    }

//quli client neni default
let private environment: Environment =
    { 
        filterTimetables = filterTimetables 
        downloadAndSaveTimetables = downloadAndSaveTimetables       
    }    

let internal webscraping_MDPO pathToDir =  

     //tryWith block is in the main() function  

    let stateReducer (state: State) (message: Messages) (action: Actions) (environment: Environment) =

        let dirList pathToDir = [ sprintf"%s\%s"pathToDir ODISDefault.odisDir6 ]

        let errorHandling fn = 
            tryWith2 (lazy ()) fn           
            |> function    
                | Ok value -> value
                | Error _  -> closeItBaby message message.msg16      

        match action with                                                   
        | StartProcess           -> 
                                  let processStartTime =  
                                      Console.Clear()
                                      let processStartTime = sprintf "Začátek procesu: %s" <| DateTime.Now.ToString("HH:mm:ss") 
                                          in message.msgParam7 processStartTime 
                                      in errorHandling processStartTime                      

        | DeleteOneODISDirectory ->                                     
                                  let dirName = ODISDefault.odisDir6                                    
                                  let myDeleteFunction =  
                                      //rozdil mezi Directory a DirectoryInfo viz Unique_Identifier_And_Metadata_File_Creator.sln -> MainLogicDG.fs
                                      let dirInfo = new DirectoryInfo(pathToDir)    
                                          in 
                                          dirInfo.EnumerateDirectories()
                                          |> Seq.filter (fun item -> item.Name = dirName) 
                                          |> Seq.iter _.Delete(true)//(fun item -> item.Delete(true)) //trochu je to hack, ale nemusim se zabyvat tryHead, bo moze byt empty kolekce    
                                      in errorHandling myDeleteFunction  
                                  message.msg12() 
                                    
        | CreateFolders          -> 
                                  let myFolderCreation = 
                                      dirList pathToDir
                                      |> List.iter (fun dir -> Directory.CreateDirectory(dir) |> ignore)                    
                                      in errorHandling myFolderCreation           
                              
        | FilterDownloadSave     -> 
                                  //filtering timetable links, downloading and saving timetables in the pdf format 
                                  let filterDownloadSave = 
                                      let pathToSubdir = dirList pathToDir |> List.head    
                                      match pathToSubdir |> Directory.Exists with 
                                      | false ->                                              
                                               message.msgParam5 pathToSubdir   
                                               message.msg1()                                                
                                      | true  -> 
                                               environment.filterTimetables pathToSubdir message
                                               |> environment.downloadAndSaveTimetables message pathToSubdir                                       
                                      in errorHandling filterDownloadSave           
                                                                                
        | EndProcess             -> 
                                  let processEndTime =    
                                      let processEndTime = sprintf "Konec procesu: %s" <| DateTime.Now.ToString("HH:mm:ss")                       
                                          in message.msgParam7 processEndTime
                                      in errorHandling processEndTime           
                                 
    stateReducer stateDefault messagesDefault StartProcess environment
    stateReducer stateDefault messagesDefault DeleteOneODISDirectory environment
    stateReducer stateDefault messagesDefault CreateFolders environment
    stateReducer stateDefault messagesDefault FilterDownloadSave environment
    stateReducer stateDefault messagesDefault EndProcess environment