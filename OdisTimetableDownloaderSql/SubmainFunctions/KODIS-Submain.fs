namespace SubmainFunctions

open TransformationLayers.TransormationLayerSend

module KODIS_Submain =

    open System
    open System.IO
    open System.Net
    open System.Threading
    open System.Net.NetworkInformation
    open System.Text.RegularExpressions

    open FsHttp
    open FSharp.Data
    open FSharp.Control
    open FsToolkit.ErrorHandling
    open Microsoft.FSharp.Quotations
    open FSharp.Quotations.Evaluator.QuotationEvaluationExtensions

    open Settings.SettingsKODIS
    open Settings.SettingsGeneral
    
    open Types
    open Types.Messages

    open Helpers
    open Helpers.Builders
    open Helpers.TryWithRF    
    open Helpers.MsgBoxClosing
    open Helpers.ProgressBarFSharp  
    open Helpers.CollectionSplitting

    open Database.Select
    open Database.InsertInto
    open Database.Connection

    open DomainModelling.DomainModel

    
    //DO NOT DIVIDE this module into parts in line with the main design pattern yet - KODIS keeps making unpredictable changes or amendments

    type internal KodisTimetables = JsonProvider<pathJson> 

    //*************************Helpers************************************************************

    //space for helpers

    let inline private expr (param : 'a) = Expr.Value(param)  
    
    //********************* infinite checking for Json files download ******************************
    
    //Cancellation tokens for educational purposes
    let private cts = new CancellationTokenSource() //TODO podumat, kaj zrobit cts.Dispose()
    let private tokenJson = cts.Token 

    AsyncSeq.initInfinite (fun _ -> tokenJson.IsCancellationRequested)
    |> AsyncSeq.takeWhile ((=) false) 
    |> AsyncSeq.iterAsync
        (fun _ ->
                async
                    {
                        match not <| NetworkInterface.GetIsNetworkAvailable() with
                        | true  -> (processor 120000 Json).Post(First(1))                                                                                                                                            
                        | false -> () 
                        do! Async.Sleep(5000) 
                    }
        )   
    |> Async.StartImmediate   

    //************************Main code***********************************************************
           
    let internal downloadAndSaveJson message = //FsHttp
               
        let l = jsonLinkList |> List.length

        let counterAndProgressBar =
            MailboxProcessor.Start
                (fun inbox 
                    ->
                     let rec loop n =
                         async
                             { 
                                 let! msg = inbox.Receive()                                    
                                 match msg with
                                 | Incr i             ->
                                                       progressBarContinuous message n l                                                        
                                                       return! loop (n + i)
                                 | Fetch replyChannel ->
                                                       replyChannel.Reply n 
                                                       return! loop n
                         }
                     loop 0
                )
       
        let updateJson listTuple =    
        
            Console.Write("\r" + new string(' ', (-) Console.WindowWidth 1) + "\r")
            Console.CursorLeft <- 0            
   
            let (jsonLinkList1, pathToJsonList1) = listTuple     

            (jsonLinkList1, pathToJsonList1)
            ||> List.map2
                 (fun (uri: string) path
                     ->                       
                      async
                          {    
                              //failwith "Simulated exception"  
                              use! response = get >> Request.sendAsync <| uri 

                              match response.statusCode with
                              | HttpStatusCode.OK ->
                                                   //Adapted FsHttp library function
                                                   //do! responseSaveFileAsync counterAndProgressBarPost path response |> Async.AwaitTask
                                                   
                                                   counterAndProgressBar.Post(Incr 1)
                                                   
                                                   do! response.SaveFileAsync >> Async.AwaitTask <| path  //Original library function                                                                                                          
                                                   return Ok ()                                   
                              | _                 ->  
                                                   return Error String.Empty      
                      }                         
                      |> Async.Catch 
                      |> Async.RunSynchronously
                      |> Result.ofChoice                                  
                 ) 
                 |> Result.sequence 
                 |> function
                     | Ok _    -> ()
                     | Error _ -> failwith "Chyba v průběhu stahování JSON souborů pro JŘ KODIS."                                 
                                 
        message.msg2()      
        
        let fSharpAsyncParallel message =  

            message.msg15()

            let numberOfThreads1 = numberOfThreads message l

            let myList =       
                (splitListIntoEqualParts numberOfThreads1 jsonLinkList, splitListIntoEqualParts numberOfThreads1 pathToJsonList)
                ||> List.zip                 
                        
            fun i -> <@ async { return updateJson (%%expr myList |> List.item %%(expr i)) } @>
            |> List.init numberOfThreads1 
            |> List.map _.Compile()       
            |> Async.Parallel 
            |> Async.Catch //zachytilo failwith "Simulated exception"
            |> Async.RunSynchronously
            |> Result.ofChoice           
            |> function
                | Ok _      ->                             
                             message.msg3() 
                             message.msg4()
                | Error err ->
                             closeItBaby message (string err.Message)  

        fSharpAsyncParallel message      
   
    let private digThroughJsonStructure message = //prohrabeme se strukturou json souboru //printfn -> additional 4 parameters
    
        let kodisTimetables : Reader<string list, string array> = 

            reader //Reader monad for educational purposes only, no real benefit here  
                {
                    let! pathToJsonList = fun env -> env 

                    let result = 
                        pathToJsonList 
                        |> Array.ofList 
                        |> Array.collect 
                            (fun pathToJson 
                                ->   
                                 let kodisJsonSamples = KodisTimetables.Parse(File.ReadAllText pathToJson) |> Option.ofNull 
                                 //let kodisJsonSamples = kodisJsonSamples.GetSample() |> Option.ofObj  //v pripade jen jednoho json               
                                 kodisJsonSamples 
                                 |> function 
                                     | Some value -> 
                                                   value |> Array.map _.Timetable //quli tomuto je nutno Array
                                     | None       -> 
                                                   message.msg5()                                                          
                                                   closeItBaby message message.msg16 
                                                   [||]    
                            ) 
                        
                    return
                        tryWith2 (lazy ()) result           
                        |> function    
                            | Ok value -> 
                                        value
                            | Error _  -> 
                                        closeItBaby message message.msg16 
                                        [||]               
                }

        let kodisAttachments : Reader<string list, string array> = //Reader monad for educational purposes only, no real benefit here
        
            reader 
                {
                    let! pathToJsonList = fun env -> env 
                    
                    let result = 

                        pathToJsonList
                        |> Array.ofList 
                        |> Array.collect  //vzhledem ke komplikovanosti nepouzivam Result.sequence pro Array.collect
                            (fun pathToJson 
                                -> 
                                 let fn1 (value: JsonProvider<pathJson>.Attachment array) = 
                                     value
                                     |> Array.Parallel.map (fun item -> item.Url |> Option.ofStringObjXXL)
                                     |> Array.choose id //co neprojde, to beze slova ignoruju

                                 let fn2 (item: JsonProvider<pathJson>.Vyluky) =  //quli tomuto je nutno Array     
                                     item.Attachments |> Option.ofNull        
                                     |> function 
                                         | Some value ->
                                                       value |> fn1
                                         | None       -> 
                                                       message.msg6() 
                                                       closeItBaby message message.msg16 
                                                       [||]                 

                                 let fn3 (item: JsonProvider<pathJson>.Root) =  //quli tomuto je nutno Array 
                                     item.Vyluky |> Option.ofNull  
                                     |> function 
                                         | Some value ->
                                                       value |> Array.collect fn2 
                                           | None     ->
                                                       message.msg7() 
                                                       closeItBaby message message.msg16 
                                                       [||] 
                                                      
                                 let kodisJsonSamples = KodisTimetables.Parse(File.ReadAllText pathToJson) |> Option.ofNull  
                                                      
                                 kodisJsonSamples 
                                 |> function 
                                     | Some value -> 
                                                   value |> Array.collect fn3 
                                     | None       -> 
                                                   message.msg8() 
                                                   closeItBaby message message.msg16 
                                                   [||]                                 
                            ) 
                
                    return
                        tryWith2 (lazy ()) result           
                        |> function    
                            | Ok value -> 
                                        value
                            | Error _  -> 
                                        closeItBaby message message.msg16 
                                        [||]               
                }
        
        let addOn () = 
            [
                //pro pripad, kdyby KODIS strcil odkazy do uplne jinak strukturovaneho jsonu, tudiz by neslo pouzit dany type provider, anebo kdyz je vubec do jsonu neda (nize uvedene odkazy)
                //@"https://kodis-files.s3.eu-central-1.amazonaws.com/76_2023_10_09_2023_10_20_v_f2b77c8fad.pdf"
                //@"https://kodis-files.s3.eu-central-1.amazonaws.com/64_2023_10_09_2023_10_20_v_02e6717b5c.pdf" 
                //@"https://kodis-files.s3.eu-central-1.amazonaws.com/timetables/119_2024_03_03_2024_12_09.pdf"               
            ] |> List.toArray 
   
        (Array.append (Array.append <| kodisAttachments pathToJsonList <| kodisTimetables pathToJsonList) <| addOn()) |> Array.distinct 
        //(Array.append <| kodisAttachments () <| kodisTimetables ()) |> Array.distinct 

        //kodisAttachments() |> Set.ofArray //over cas od casu
        //kodisTimetables() |> Set.ofArray //over cas od casu

    let private filterTimetables message param (pathToDir: string) diggingResult = 

        //*************************************Helpers for SQL columns********************************************

        let extractSubstring (input: string) =
                        
            let pattern = @"202[3-9]_[0-1][0-9]_[0-3][0-9]_202[4-9]_[0-1][0-9]_[0-3][0-9]"
            let regex = new Regex(pattern) 
            let matchResult = regex.Match(input)
        
            match matchResult.Success with
            | true  -> input 
            | false -> String.Empty 

            |> tryWith2 (lazy ())            
            |> function    
                | Ok value -> value
                | Error _  -> String.Empty            
        
        let extractSubstring1 (input: string) =

            let pattern = @"202[3-9]_[0-1][0-9]_[0-3][0-9]_202[4-9]_[0-1][0-9]_[0-3][0-9]"
            let regex = new Regex(pattern) 
            let matchResult = regex.Match(input)
        
            match matchResult.Success with
            | true  -> matchResult.Value
            | false -> String.Empty

            |> tryWith2 (lazy ())            
            |> function    
                | Ok value -> value
                | Error _  -> String.Empty       

        let extractStartDate (input: string) =
             let result = 
                 match input.Equals(String.Empty) with
                 | true  -> String.Empty
                 | _     -> input.[0..min 9 (input.Length - 1)] 
             result.Replace("_", "-")
         
        let extractEndDate (input: string) =
            let result = 
                match input.Equals(String.Empty) with
                | true  -> String.Empty
                | _     -> input.[max 0 (input.Length - 10)..]
            result.Replace("_", "-")

        let splitString (input: string) =   
            match input.StartsWith(pathKodisAmazonLink) with
            | true  -> [pathKodisAmazonLink; input.Substring(pathKodisAmazonLink.Length)]
            | false -> [pathKodisAmazonLink; input]

        //*************************************Splitting Kodis links into SQL columns********************************************
        let splitKodisLink input =

            let oldPrefix = 
                Regex.Split(input, extractSubstring1 input) 
                |> Array.toList
                |> List.item 0
                |> splitString
                |> List.item 1
                |> tryWith2 (lazy ())            
                |> function    
                    | Ok value -> value
                    | Error _  -> String.Empty 

            let totalDateInterval = extractSubstring1 input

            let partAfter =
                Regex.Split(input, totalDateInterval)
                |> Array.toList
                |> List.item 1    
                |> tryWith2 (lazy ())            
                |> function    
                    | Ok value -> value
                    | Error _  -> String.Empty 
        
            let vIndex = partAfter.IndexOf "_v"
            let tIndex = partAfter.IndexOf "_t"

            let suffix = 
                match [vIndex; tIndex].Length = -2 with
                | false when vIndex <> -1 -> partAfter.Substring(0, vIndex + 2)
                | false when tIndex <> -1 -> partAfter.Substring(0, tIndex + 2)
                | _                       -> String.Empty
           
            let jsGeneratedString =
                match [vIndex; tIndex].Length = -2 with
                | false when vIndex <> -1 -> partAfter.Substring(vIndex + 2)
                | false when tIndex <> -1 -> partAfter.Substring(tIndex + 2)
                | _                       -> partAfter
        
            let newPrefix (oldPrefix: string) =

                let conditions =
                    [
                        fun () -> oldPrefix.Contains("AE") && oldPrefix.Length = 3
                        fun () -> oldPrefix.Contains("S") && oldPrefix.Length = 3
                        fun () -> oldPrefix.Contains("S") && oldPrefix.Length = 4
                        fun () -> oldPrefix.Contains("R") && oldPrefix.Length = 3
                        fun () -> oldPrefix.Contains("R") && oldPrefix.Length = 4
                        fun () -> oldPrefix.Contains("NAD") && oldPrefix.Length = 5
                        fun () -> oldPrefix.Contains("NAD") && oldPrefix.Length = 6
                        fun () -> oldPrefix.Contains("NAD") && oldPrefix.Length = 7
                        fun () -> oldPrefix.Contains("X") && oldPrefix.Length = 4
                        fun () -> oldPrefix.Contains("X") && oldPrefix.Length = 5
                        fun () -> oldPrefix.Contains("X") && oldPrefix.Length = 6
                    ]

                match List.filter (fun condition -> condition()) conditions with
                | [ _ ] -> 
                         let index = conditions |> List.findIndex (fun item -> item () = true) //neni treba tryFind, bo v [ _ ] je vzdy neco
                     
                         match index with
                         | 0  -> 
                               sprintf "_%s" oldPrefix
                         | 1  ->
                               sprintf "_%s" oldPrefix
                         | 2  ->
                               sprintf "%s" oldPrefix
                         | 3  ->
                               sprintf "_%s" oldPrefix
                         | 4  ->
                               sprintf "%s" oldPrefix
                         | 5  ->
                               sprintf "%s" oldPrefix
                         | 6  ->
                               oldPrefix.Replace("NAD_", "NAD_00")
                         | 7  ->
                               oldPrefix.Replace("NAD_", "NAD_0")
                         | 8  -> 
                               let s1 = oldPrefix
                               let s2 = sprintf "X_00%s" s1.[2..]
                               oldPrefix.Replace(s1, s2)
                         | 9  ->
                               let s1 = oldPrefix
                               let s2 = sprintf "X_0%s" s1.[2..]
                               oldPrefix.Replace(s1, s2)
                         | 10 ->
                               sprintf "%s" oldPrefix
                         | _  ->
                               sprintf "%s" oldPrefix

                | _     ->
                         match oldPrefix.Length with                    
                         | 2  -> sprintf "00%s" oldPrefix
                         | 3  -> sprintf "0%s" oldPrefix                  
                         | _  -> oldPrefix

            let input = 
                match input.Contains("_t") with 
                | true  -> input.Replace(pathKodisAmazonLink, sprintf"%s%s" pathKodisAmazonLink @"timetables/").Replace("_t.pdf", ".pdf") 
                | false -> input   
        
            let fileToBeSaved = sprintf "%s%s%s.pdf" (newPrefix oldPrefix) totalDateInterval suffix

            {
                oldPrefix = oldPrefix
                newPrefix = newPrefix oldPrefix
                startDate = extractStartDate totalDateInterval
                endDate = extractEndDate totalDateInterval
                totalDateInterval = totalDateInterval
                suffix = suffix
                jsGeneratedString = jsGeneratedString
                completeLink = input
                fileToBeSaved = fileToBeSaved
            }
            |> dbDataTransferLayerSend  

     
        //**********************Filtering and SQL data inserting********************************************************
        let dataToBeInserted =           
            diggingResult       
            |> Array.Parallel.map 
                (fun item -> 
                           let item = extractSubstring item      //"https://kodis-files.s3.eu-central-1.amazonaws.com/timetables/2_2023_03_13_2023_12_09.pdf                 
                           match item.Contains @"timetables/" with
                           | true  -> item.Replace("timetables/", String.Empty).Replace(".pdf", "_t.pdf")
                           | false -> item  
                )  
            |> Array.toList
            |> List.sort //jen quli testovani
            |> List.filter
                (fun item -> 
                           let cond1 = (item |> Option.ofStringObjXXL).IsSome
                           let cond2 = item |> Option.ofNull |> Option.ofStringOption |> Option.toBool //for learning purposes - compare with (not String.IsNullOrEmpty(item))
                           cond1 && cond2 
                )         
            |> List.map 
                (fun item -> splitKodisLink item) 

        insert getConnection closeConnection dataToBeInserted message
        
        //**********************Cesty pro soubory pro aktualni a dlouhodobe platne a pro ostatni********************************************************
        let createPathsForDownloadedFiles list =
            
            list 
            |> List.map
                (fun (link, file) 
                    -> 
                     let path =                                         
                         let (|IntType|StringType|OtherType|) (param : 'a) = //zatim nevyuzito, mozna -> TODO podumat nad refactoringem nize uvedeneho 
                             match param.GetType() with
                             | typ when typ = typeof<int>    -> IntType   
                             | typ when typ = typeof<string> -> StringType  
                             | _                             -> OtherType                                                      
                                                
                         let pathToDir = sprintf "%s\\%s" pathToDir file //pro ostatni
                                           
                         match pathToDir.Contains("JR_ODIS_aktualni_vcetne_vyluk") || pathToDir.Contains("JR_ODIS_teoreticky_dlouhodobe_platne_bez_vyluk") with 
                         | true ->   
                                 true //pro aktualni a dlouhodobe platne
                                 |> function
                                     | true when file.Substring(0, 1) = "0"  -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 0 sortedLines)
                                     | true when file.Substring(0, 1) = "1"  -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 0 sortedLines)
                                     | true when file.Substring(0, 1) = "2"  -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 1 sortedLines)
                                     | true when file.Substring(0, 1) = "3"  -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 2 sortedLines)
                                     | true when file.Substring(0, 1) = "4"  -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 3 sortedLines)
                                     | true when file.Substring(0, 1) = "5"  -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 4 sortedLines)
                                     | true when file.Substring(0, 1) = "6"  -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 5 sortedLines)
                                     | true when file.Substring(0, 1) = "7"  -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 6 sortedLines)
                                     | true when file.Substring(0, 1) = "8"  -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 7 sortedLines)
                                     | true when file.Substring(0, 1) = "9"  -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 8 sortedLines)
                                     | true when file.Substring(0, 1) = "S"  -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 9 sortedLines)
                                     | true when file.Substring(0, 1) = "R"  -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 10 sortedLines)
                                     | true when file.Substring(0, 2) = "_S" -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 9 sortedLines)
                                     | true when file.Substring(0, 2) = "_R" -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 10 sortedLines)
                                     | _                                     -> pathToDir.Replace("_vyluk", sprintf "%s\\%s\\" <| "_vyluk" <| List.item 11 sortedLines)                                                           
                         | _    -> 
                                 pathToDir    
                     link, path 
                )          

        let selectDataFromDb = 
            match param with 
            | CurrentValidity           -> "dbo.ITVF_GetLinksCurrentValidity()" |> select getConnection closeConnection message pathToDir |> createPathsForDownloadedFiles
            | FutureValidity            -> "dbo.ITVF_GetLinksFutureValidity()" |> select getConnection closeConnection message pathToDir |> createPathsForDownloadedFiles
            | ReplacementService        -> "dbo.ITVF_GetLinksReplacementService()" |> select getConnection closeConnection message pathToDir |> createPathsForDownloadedFiles   
            | WithoutReplacementService -> "dbo.ITVF_GetLinksWithoutReplacementService()" |> select getConnection closeConnection message pathToDir |> createPathsForDownloadedFiles 

        let selectDataFromDb1 = 
            match param with 
            | CurrentValidity           -> DataTable.InsertInto.filter dataToBeInserted CurrentValidity |> createPathsForDownloadedFiles
            | FutureValidity            -> DataTable.InsertInto.filter dataToBeInserted FutureValidity |> createPathsForDownloadedFiles
            | ReplacementService        -> DataTable.InsertInto.filter dataToBeInserted ReplacementService |> createPathsForDownloadedFiles  
            | WithoutReplacementService -> DataTable.InsertInto.filter dataToBeInserted WithoutReplacementService |> createPathsForDownloadedFiles 
        
        selectDataFromDb
 
    let internal deleteAllODISDirectories message pathToDir = 

        let deleteIt : Reader<string list, unit> = 
    
            reader //Reader monad for educational purposes only, no real benefit here  
                {
                    let! getDefaultRecordValues = fun env -> env

                    let l = getDefaultRecordValues |> List.length

                    let numberOfThreads1 = numberOfThreads message l

                    let myList =                         
                        //rozdil mezi Directory a DirectoryInfo viz Unique_Identifier_And_Metadata_File_Creator.sln -> MainLogicDG.fs
                        let dirInfo = new DirectoryInfo(pathToDir)  
                        //smazeme pouze adresare obsahujici stare JR, ostatni ponechame                           
                        dirInfo.EnumerateDirectories() 
                        |> Seq.filter (fun item -> getDefaultRecordValues |> List.contains item.Name) //prunik dvou kolekci (plus jeste Seq.distinct pro unique items)
                        |> Seq.distinct 
                        |> Seq.toList
                        |> splitListIntoEqualParts numberOfThreads1 

                    let myDeleteFunction (list : DirectoryInfo list) = list |> List.iter _.Delete(true)    
                
                    fun i -> <@ async { return myDeleteFunction (%%expr myList |> List.item %%(expr i)) } @>
                    |> List.init myList.Length
                    |> List.map _.Compile()       
                    |> Async.Parallel 
                    |> Async.Catch  //Async.Catch by mel stacit  
                    |> Async.RunSynchronously
                    |> Result.ofChoice  
                    |> function
                        | Ok _    -> ()                                                                      
                        | Error _ -> closeItBaby message message.msg16 
                                     
                    return ()
                }

        deleteIt listODISDefault4
    
        message.msg10() 
        message.msg11()     
 
    let internal createNewDirectories pathToDir : Reader<string list, string list> =
        //Reader monad for educational purposes only, no real benefit here
        reader { let! getDefaultRecordValues = fun env -> env in return getDefaultRecordValues |> List.map (fun item -> sprintf"%s\%s"pathToDir item) } 

    let internal createDirName variant : Reader<string list, string> = //Reader monad for educational purposes only, no real benefit here

        reader
            {
                let! getDefaultRecordValues = fun env -> env

                return 
                    match variant with 
                    | CurrentValidity           -> getDefaultRecordValues |> List.item 0
                    | FutureValidity            -> getDefaultRecordValues |> List.item 1
                    | ReplacementService        -> getDefaultRecordValues |> List.item 2                                
                    | WithoutReplacementService -> getDefaultRecordValues |> List.item 3
            } 

    let internal deleteOneODISDirectory message variant pathToDir =

        //smazeme pouze jeden adresar obsahujici stare JR, ostatni ponechame

        let deleteIt : Reader<string list, unit> =  

            reader //Reader monad for educational purposes only, no real benefit here  
                {   
                    let! getDefaultRecordValues = fun env -> env

                    let myDeleteFunction = 
                        //rozdil mezi Directory a DirectoryInfo viz Unique_Identifier_And_Metadata_File_Creator.sln -> MainLogicDG.fs
                        let dirInfo = new DirectoryInfo(pathToDir)        
                            in
                            dirInfo.EnumerateDirectories()
                            |> Seq.filter (fun item -> item.Name = createDirName variant getDefaultRecordValues) 
                            |> Seq.iter _.Delete(true) //trochu je to hack, ale nemusim se zabyvat tryHead, bo moze byt empty kolekce                 
                
                    return 
                        tryWith2 (lazy ()) myDeleteFunction           
                        |> function    
                            | Ok value -> value
                            | Error _  -> closeItBaby message message.msg16 
                }

        deleteIt listODISDefault4    

        message.msg10() 
        message.msg11()   
 
     //list -> aby bylo mozno pouzit funkci createFolders bez uprav
    let internal createOneNewDirectory pathToDir dirName = [ sprintf"%s\%s"pathToDir dirName ] 
 
    let internal createFolders message dirList =  
        
        dirList
        |> List.iter
            (fun (dir: string) 
                ->                
                 match dir.Contains("JR_ODIS_aktualni_vcetne_vyluk") || dir.Contains("JR_ODIS_teoreticky_dlouhodobe_platne_bez_vyluk") with 
                 | true ->    
                         sortedLines 
                         |> List.iter
                             (fun item -> 
                                        let dir = dir.Replace("_vyluk", sprintf "%s\\%s" "_vyluk" item)
                                        Directory.CreateDirectory(dir) |> ignore
                             )           
                 | _    -> 
                         Directory.CreateDirectory(sprintf "%s" dir) |> ignore           
            )              
       |> tryWith2 (lazy ())            
       |> function    
           | Ok value -> value                                    
           | Error _  -> closeItBaby message message.msg16                                    

    let private downloadAndSaveTimetables message pathToDir =     //FsHttp
        
        message.msgParam3 pathToDir  

        let asyncDownload (counterAndProgressBar : MailboxProcessor<Msg>) list =   
            
            cts.Cancel()
            
            list 
            |> List.iter 
                (fun (uri, (pathToFile: string)) 
                    ->                         
                     async
                         {    
                             match not <| NetworkInterface.GetIsNetworkAvailable() with
                             | true  ->                                    
                                      (processor 0 Pdf).Post(First(1)) 
                                      Thread.Sleep(600000)
                             | false ->  
                                      //failwith "Simulated exception"  
                                      counterAndProgressBar.Post(Incr 1)
                                       
                                      let get uri =
                                          http 
                                              {
                                                  config_timeoutInSeconds 120  //for educational purposes
                                                  GET(uri) 
                                              }    
                                     
                                      use! response = get >> Request.sendAsync <| uri  
                                     
                                      match response.statusCode with
                                      | HttpStatusCode.OK -> return! response.SaveFileAsync >> Async.AwaitTask <| pathToFile      //Original FsHttp library function                                                                                                 
                                      | _                 -> return message.msgParam8 "Chyba v průběhu stahování JŘ KODIS."       //nechame chybu projit v loop                                                                                                                                  
                         } 
                         |> Async.Catch
                         |> Async.RunSynchronously  
                         |> Result.ofChoice                      
                         |> function
                             | Ok _    -> ()                                                                                 
                             | Error _ -> message.msgParam2 uri  //nechame chybu projit v loop => nebude Result.sequence
                )  

        reader
            {   
                return! 
                    (fun (env: (string*string) list)
                        ->                           
                         let l = env |> List.length

                         let numberOfThreads1 = numberOfThreads message l

                         let counterAndProgressBar =
                             MailboxProcessor.Start
                                (fun inbox 
                                    ->
                                     let rec loop n =
                                         async
                                             { 
                                                 let! msg = inbox.Receive()
                                                 match msg with
                                                 | Incr i             -> 
                                                                       progressBarContinuous message n l  
                                                                       return! loop (n + i)
                                                 | Fetch replyChannel ->
                                                                       replyChannel.Reply n 
                                                                       return! loop n
                                             }
                                     loop 0
                                )

                         match env.Length >= numberOfThreads1 with 
                         | false ->  
                                  asyncDownload counterAndProgressBar env
                                  message.msgParam4 pathToDir  
                         | true  ->                                
                                  let myList = splitListIntoEqualParts numberOfThreads1 env                             
                              
                                  fun i -> <@ async { return asyncDownload counterAndProgressBar (%%expr myList |> List.item %%(expr i)) } @>
                                  |> List.init myList.Length
                                  |> List.map _.Compile()       
                                  |> Async.Parallel 
                                  |> Async.Catch 
                                  |> Async.RunSynchronously
                                  |> Result.ofChoice  
                                  |> function
                                      | Ok _    -> message.msgParam4 pathToDir
                                      | Error _ -> message.msgParam7 "Chyba při paralelním stahování JŘ."  //nechame chybu projit v loop                                            
                    )                        
            } 
            
    let internal downloadAndSave message variant dir = 

        match dir |> Directory.Exists with 
        | false -> 
                 message.msgParam5 dir 
                 message.msg13()                                                
        | true  ->
                 let digThroughJsonStructure = 
                     tryWith2 (lazy ()) (digThroughJsonStructure message)           
                     |> function    
                         | Ok value -> 
                                     value
                         | Error _  -> 
                                     closeItBaby message message.msg16 
                                     [||]
                                 
                 let filterTimetables = 
                     tryWith2 (lazy ()) (filterTimetables message variant dir digThroughJsonStructure)           
                     |> function    
                         | Ok value -> 
                                     value
                         | Error _  -> 
                                     closeItBaby message message.msg16 
                                     []                                

                 tryWith2 (lazy ()) (downloadAndSaveTimetables message dir filterTimetables)           
                 |> function    
                     | Ok value -> value                                
                     | Error _  -> closeItBaby message message.msg16     