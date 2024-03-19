namespace MainFunctions

module WebScraping_KODISFM = 

    open System
         
    open Types
    open Types.Messages
    open Settings.Messages
    open Settings.SettingsGeneral
    
    open Logging.Logging

    open Helpers.CloseApp  
    open Helpers.FreeMonads  
    
    open Database.InsertInto
    open Database.Connection

    open SubmainFunctions
    open SubmainFunctions.KODIS_Submain
        
    //FREE MONAD 

    let internal webscraping_KODISFM pathToDir (variantList: Validity list) = 
            
        let rec interpret message clp  = 

            let errorHandling fn = 
                try
                    fn
                with
                | ex ->
                      logInfoMsg <| sprintf "050 %s" (string ex.Message)
                      closeItBaby message message.msg16           

            //function //CommandLineProgram<unit> -> unit
            match clp with
            | Pure x                                -> 
                                                     x //nevyuzito

            | Free (StartProcessFM next)            -> 
                                                     let processStartTime =    
                                                        Console.Clear()
                                                        let processStartTime = sprintf "Začátek procesu: %s" <| DateTime.Now.ToString("HH:mm:ss") 
                                                            in message.msgParam7 processStartTime 
                                                        in errorHandling processStartTime

                                                     let param = next ()
                                                     interpret message param

            | Free (DownloadAndSaveJsonFM next)     ->                                                 
                                                     //let downloadAndSaveJson =  
                                                        // downloadAndSaveJson message  
                                                        // in errorHandling downloadAndSaveJson

                                                     let param = next ()
                                                     interpret message param                                                
                                                
            | Free (DownloadSelectedVariantFM next) -> 
                                                     let downloadSelectedVariant = 
                                                         match variantList |> List.length with
                                                         //SingleVariantDownload
                                                         | 1 -> 
                                                              let variant = variantList |> List.head
                                                              deleteOneODISDirectory message variant pathToDir                                                        
                                                              let dirList = 
                                                                  createOneNewDirectory  //list -> aby bylo mozno pouzit funkci createFolders bez uprav  
                                                                  <| pathToDir 
                                                                  <| createDirName variant listODISDefault4 
                                                              createFolders message dirList
                                                              KODIS_Submain.downloadAndSave message variant (dirList |> List.head)  

                                                         //BulkVariantDownload       
                                                         | _ ->  
                                                              deleteAllODISDirectories message pathToDir
                                                              let dirList = createNewDirectories pathToDir listODISDefault4
                                                              createFolders message dirList 
                                                              (variantList, dirList)
                                                              ||> List.iter2 (fun variant dir -> KODIS_Submain.downloadAndSave message variant dir)     
                                                                                                             
                                                         in errorHandling downloadSelectedVariant  

                                                     let param = next ()
                                                     interpret message param

            | Free (EndProcessFM _)                 ->
                                                     let processEndTime =    
                                                         insertLogEntries getConnection closeConnection message 
                                                         let processEndTime = sprintf "Konec procesu: %s" <| DateTime.Now.ToString("HH:mm:ss")                       
                                                             in message.msgParam7 processEndTime
                                                         in errorHandling processEndTime
        cmdBuilder
            {
                let! _ = Free (StartProcessFM Pure)
                let! _ = Free (DownloadAndSaveJsonFM Pure)
                let! _ = Free (DownloadSelectedVariantFM Pure)

                return! Free (EndProcessFM Pure)
            } |> interpret messagesDefault 

        //*****************************************************************************************************************************************

        //CurrentValidity = JR striktne platne k danemu dni, tj. pokud je napr. na dany den vylukovy JR, stahne se tento JR, ne JR platny dalsi den
        //FutureValidity = JR platne v budouci dobe, ktere se uz vyskytuji na webu KODISu
        //ReplacementService = pouze vylukove JR, JR NAD a JR X linek
        //WithoutReplacementService = JR dlouhodobe platne bez jakykoliv vyluk. Tento vyber neobsahuje ani dlouhodobe nekolikamesicni vyluky, muze se ale hodit v pripade, ze zakladni slozka s JR obsahuje jedno ci dvoudenni vylukove JR.     