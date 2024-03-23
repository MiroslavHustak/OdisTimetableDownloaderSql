namespace MainFunctions

open System

open Types
open Settings.Messages
open Settings.SettingsGeneral

open Logging.Logging
    
open Helpers.CloseApp  
open Helpers.FreeMonads

open SubmainFunctions
open SubmainFunctions.KODIS_SubmainDataTable

module WebScraping_KODISFMDataTable = 
    
    //FREE MONAD 

    let internal webscraping_KODISFMDataTable pathToDir (variantList: Validity list) = 
            
        let rec interpret clp  = 

            let errorHandling fn = 
                try
                    fn
                with
                | ex ->
                      logInfoMsg <| sprintf "Err049 %s" (string ex.Message)
                      closeItBaby msg16           

            //function //CommandLineProgram<unit> -> unit
            match clp with
            | Pure x                                -> 
                                                     x //nevyuzito

            | Free (StartProcessFM next)            -> 
                                                     let processStartTime =    
                                                         Console.Clear()
                                                         let processStartTime = 
                                                             try                                                                                                                                
                                                                 sprintf "Začátek procesu: %s" <| DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") 
                                                             with
                                                             | ex ->       
                                                                   logInfoMsg <| sprintf "Err503 %s" (string ex.Message)
                                                                   sprintf "Začátek procesu nemohl býti ustanoven."   
                                                             in msgParam7 processStartTime 
                                                         in errorHandling processStartTime

                                                     let param = next ()
                                                     interpret param

            | Free (DownloadAndSaveJsonFM next)     ->                                                 
                                                     let downloadAndSaveJson =  
                                                         KODIS_SubmainDataTable.downloadAndSaveJson ()  
                                                         in errorHandling downloadAndSaveJson

                                                     let param = next ()
                                                     interpret param                                                
                                                
            | Free (DownloadSelectedVariantFM next) -> 
                                                     let downloadSelectedVariant = 
                                                         match variantList |> List.length with
                                                         //SingleVariantDownload
                                                         | 1 -> 
                                                              let variant = variantList |> List.head
                                                              KODIS_SubmainDataTable.deleteOneODISDirectory variant pathToDir                                                        
                                                              let dirList = 
                                                                  KODIS_SubmainDataTable.createOneNewDirectory  //list -> aby bylo mozno pouzit funkci createFolders bez uprav  
                                                                  <| pathToDir 
                                                                  <| KODIS_SubmainDataTable.createDirName variant listODISDefault4 
                                                              KODIS_SubmainDataTable.createFolders dirList
                                                              KODIS_SubmainDataTable.downloadAndSave variant (dirList |> List.head)  

                                                         //BulkVariantDownload       
                                                         | _ ->  
                                                              KODIS_SubmainDataTable.deleteAllODISDirectories pathToDir
                                                              let dirList = KODIS_SubmainDataTable.createNewDirectories pathToDir listODISDefault4
                                                              KODIS_SubmainDataTable.createFolders dirList 
                                                              (variantList, dirList)
                                                              ||> List.iter2 (fun variant dir -> KODIS_SubmainDataTable.downloadAndSave variant dir)     
                                                                                                             
                                                         in errorHandling downloadSelectedVariant  

                                                     let param = next ()
                                                     interpret param

            | Free (EndProcessFM _)                 ->
                                                     let processEndTime =    
                                                         let processEndTime = 
                                                             try                                                                                                                                
                                                                sprintf "Konec procesu: %s" <| DateTime.Now.ToString("HH:mm:ss")  
                                                             with
                                                             | ex ->       
                                                                   logInfoMsg <| sprintf "Err504 %s" (string ex.Message)
                                                                   sprintf "Konec procesu nemohl býti ustanoven."   
                                                             in msgParam7 processEndTime
                                                         in errorHandling processEndTime
        cmdBuilder
            {
                let! _ = Free (StartProcessFM Pure)
                let! _ = Free (DownloadAndSaveJsonFM Pure)
                let! _ = Free (DownloadSelectedVariantFM Pure)

                return! Free (EndProcessFM Pure)
            } |> interpret 

        //*****************************************************************************************************************************************

        //CurrentValidity = JR striktne platne k danemu dni, tj. pokud je napr. na dany den vylukovy JR, stahne se tento JR, ne JR platny dalsi den
        //FutureValidity = JR platne v budouci dobe, ktere se uz vyskytuji na webu KODISu
        //ReplacementService = pouze vylukove JR, JR NAD a JR X linek
        //WithoutReplacementService = JR dlouhodobe platne bez jakykoliv vyluk. Tento vyber neobsahuje ani dlouhodobe nekolikamesicni vyluky, muze se ale hodit v pripade, ze zakladni slozka s JR obsahuje jedno ci dvoudenni vylukove JR.     