namespace MainFunctions

module WebScraping_DPO =

    open System
    open System.IO
    open System.Net
    
    open Helpers.TryWithRF

    open Types.Messages
    open Types.DirNames
    
    open Settings.Messages
    open Settings.SettingsGeneral    

    open SubmainFunctions.DPO_Submain
    
    //************************Main code********************************************************************************

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
            filterTimetables: string -> Messages -> (string*string) list
            downloadAndSaveTimetables: Http.HttpClient -> Messages -> string -> (string*string) list -> unit
            client: Http.HttpClient 
        }

    //quli client neni default
    let private environment: Environment =
        { 
            filterTimetables = filterTimetables
            downloadAndSaveTimetables = downloadAndSaveTimetables
            client = client (lazy (messagesDefault.msgParam7 "Error4")) messagesDefault.msgParam1 
        }    

    let internal webscraping_DPO pathToDir =  

         //tryWith block is in the main() function  

        let stateReducer (state: State) (message: Messages) (action: Actions) (environment: Environment) =

            let dirList pathToDir = [ sprintf"%s\%s"pathToDir ODISDefault.odisDir5 ]

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
                                      let dirName = ODISDefault.odisDir5                                    
                                      let myDeleteFunction =  
                                          //rozdil mezi Directory a DirectoryInfo viz Unique_Identifier_And_Metadata_File_Creator.sln -> MainLogicDG.fs
                                          let dirInfo = new DirectoryInfo(pathToDir)   
                                              in 
                                              dirInfo.EnumerateDirectories()
                                              |> Seq.filter (fun item -> item.Name = dirName) 
                                              |> Seq.iter _.Delete(true) //trochu je to hack, ale nemusim se zabyvat tryHead, bo moze byt empty kolekce 
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
                                                   |> environment.downloadAndSaveTimetables environment.client message pathToSubdir   
                                                   environment.client.Dispose()
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

        environment.client.Dispose()