namespace MainFunctions

module WebScraping_KODISFMDataTable = 

    open System
         
    open Types
    open Types.Messages
    open Settings.Messages
    open Settings.SettingsGeneral
    
    open Helpers.TryWithRF  
    open Helpers.FreeMonads

    open SubmainFunctions
    open SubmainFunctions.KODIS_SubmainDataTable
    
    //FREE MONAD 

    let internal webscraping_KODISFMDataTable pathToDir (variantList: Validity list) = 
            
        let rec interpret message clp  = 

            let errorHandling fn = 
                tryWith2 (lazy ()) fn           
                |> function    
                    | Ok value -> value
                    | Error _  -> closeItBaby message message.msg16       

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
                                                     let downloadAndSaveJson =  
                                                         KODIS_SubmainDataTable.downloadAndSaveJson message  
                                                         in errorHandling downloadAndSaveJson

                                                     let param = next ()
                                                     interpret message param                                                
                                                
            | Free (DownloadSelectedVariantFM next) -> 
                                                     let downloadSelectedVariant = 
                                                         match variantList |> List.length with
                                                         //SingleVariantDownload
                                                         | 1 -> 
                                                              let variant = variantList |> List.head
                                                              KODIS_SubmainDataTable.deleteOneODISDirectory message variant pathToDir                                                        
                                                              let dirList = 
                                                                  KODIS_SubmainDataTable.createOneNewDirectory  //list -> aby bylo mozno pouzit funkci createFolders bez uprav  
                                                                  <| pathToDir 
                                                                  <| KODIS_SubmainDataTable.createDirName variant listODISDefault4 
                                                              KODIS_SubmainDataTable.createFolders message dirList
                                                              KODIS_SubmainDataTable.downloadAndSave message variant (dirList |> List.head)  

                                                         //BulkVariantDownload       
                                                         | _ ->  
                                                              KODIS_SubmainDataTable.deleteAllODISDirectories message pathToDir
                                                              let dirList = KODIS_SubmainDataTable.createNewDirectories pathToDir listODISDefault4
                                                              KODIS_SubmainDataTable.createFolders message dirList 
                                                              (variantList, dirList)
                                                              ||> List.iter2 (fun variant dir -> KODIS_SubmainDataTable.downloadAndSave message variant dir)     
                                                                                                             
                                                         in errorHandling downloadSelectedVariant  

                                                     let param = next ()
                                                     interpret message param

            | Free (EndProcessFM _)                 ->
                                                     let processEndTime =    
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