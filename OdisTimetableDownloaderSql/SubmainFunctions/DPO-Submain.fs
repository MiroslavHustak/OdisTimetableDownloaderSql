module DPO_Submain

open System
open System.IO
open System.Net
open System.Net.Http

open FSharp.Data
open FsToolkit.ErrorHandling

open SettingsDPO
open SettingsGeneral
open ProgressBarFSharp
open Messages.Messages
//open Messages.MessagesMocking

open Types.DirNames
open Types.ErrorTypes

open Types.Messages
open Messages.Messages

open ErrorHandling
open ErrorHandling.TryWithRF

//************************Submain functions************************************************************************

let internal client (printToConsole1 : Lazy<unit>) (printToConsole2: string -> unit) : HttpClient = 
    
    let f = Some (new HttpClient()) //|> Option.ofNull -> priste to uz tak nerobit, aplikovat Result type   
    
    tryWithLazy printToConsole2 (optionToResultPrint f printToConsole1) ()           
    |> function    
        | Ok value  ->
                     value 
        | Error err -> 
                     err.Force()
                     new System.Net.Http.HttpClient()  

//[<TailCall>]
let internal filterTimetables pathToDir (message: Messages) = 

    let getLastThreeCharacters input =
        match String.length input <= 3 with
        | true  -> 
                 message.msgParam6 input 
                 input 
        | false -> 
                 input.Substring(input.Length - 3)

    let removeLastFourCharacters input =
        match String.length input <= 4 with
        | true  -> 
                 message.msgParam6 input 
                 String.Empty
        | false ->
                 input.[..(input.Length - 5)]                    
    
    let urlList = 
        [
            pathDpoWebTimetablesBus      
            pathDpoWebTimetablesTrBus
            pathDpoWebTimetablesTram
        ]
    
    urlList
    |> List.collect 
        (fun url -> 
                  let document = FSharp.Data.HtmlDocument.Load(url) //neni nullable, nesu exn
                  
                  document.Descendants "a"
                  |> Seq.choose 
                      (fun htmlNode    ->
                                        htmlNode.TryGetAttribute("href") //inner text zatim nepotrebuji, cisla linek mam resena jinak  
                                        |> Option.map (fun a -> string <| htmlNode.InnerText(), string <| a.Value()) //priste to uz tak nerobit, u string zrob Option.ofNull, atd.                                         
                      )  
                  |> Seq.filter
                      (fun (_ , item2) ->
                                        item2.Contains @"/jr/" && item2.Contains ".pdf" && not (item2.Contains "AE-eng.pdf") 
                      )
                  |> Seq.map 
                      (fun (_ , item2) ->  
                                        let linkToPdf = sprintf"%s%s" pathDpoWeb item2  //https://www.dpo.cz // /jr/2023-04-01/024.pdf 

                                        let adaptedLineName =
                                            let s (item2: string) = item2.Replace(@"/jr/", String.Empty).Replace(@"/", "?").Replace(".pdf", String.Empty) 
                                            let rec x s =                                                                            
                                                match (getLastThreeCharacters s).Contains("?") with
                                                | true  -> x (sprintf "%s%s" s "_")                                                                             
                                                | false -> s
                                            (x << s) item2
                                        
                                        let lineName = 
                                            let s adaptedLineName = sprintf"%s_%s" (getLastThreeCharacters adaptedLineName) adaptedLineName  
                                            let s1 s = removeLastFourCharacters s 
                                            sprintf"%s%s" <| (s >> s1) adaptedLineName <| ".pdf"
                                            
                                        let pathToFile = 
                                            let lineName = 
                                                match item2.Contains("NAD") with
                                                | true when item2.Contains("NAD1") -> @"NAD1.pdf"
                                                | true when item2.Contains("NAD2") -> @"NAD2.pdf"
                                                | true when item2.Contains("NAD3") -> @"NAD3.pdf"
                                                | true when item2.Contains("NAD4") -> @"NAD4.pdf"
                                                | true when item2.Contains("NAD5") -> @"NAD5.pdf"
                                                | true when item2.Contains("NAD6") -> @"NAD6.pdf"
                                                | true when item2.Contains("NAD7") -> @"NAD7.pdf"
                                                | true when item2.Contains("NAD8") -> @"NAD8.pdf"
                                                | true when item2.Contains("NAD9") -> @"NAD9.pdf"
                                                | _                                -> lineName
                                            
                                            sprintf "%s/%s" pathToDir lineName
                                        linkToPdf, pathToFile
                      )
                  |> Seq.toList
                  |> List.distinct
        ) 

let internal downloadAndSaveTimetables client (message: Messages) (pathToDir: string) (filterTimetables: (string*string) list) =  

    let downloadFileTaskAsync (client: Http.HttpClient) (uri: string) (path: string) : Async<Result<unit, string>> =  
       
        async
            {                      
                try    
                    match File.Exists(path) with
                    | true  -> 
                             return Ok () 
                    | false -> 
                             let! response = client.GetAsync(uri) |> Async.AwaitTask
                        
                             match response.IsSuccessStatusCode with //true if StatusCode was in the range 200-299; otherwise, false.
                             | true  -> 
                                      let! stream = response.Content.ReadAsStreamAsync() |> Async.AwaitTask    
                                      use fileStream = new FileStream(path, FileMode.CreateNew) 
                                      do! stream.CopyToAsync(fileStream) |> Async.AwaitTask
                                      
                                      return Ok ()

                             | false -> 
                                      let errorType = 
                                          match response.StatusCode with
                                          | HttpStatusCode.BadRequest          -> Error connErrorCodeDefault.BadRequest
                                          | HttpStatusCode.InternalServerError -> Error connErrorCodeDefault.InternalServerError
                                          | HttpStatusCode.NotImplemented      -> Error connErrorCodeDefault.NotImplemented
                                          | HttpStatusCode.ServiceUnavailable  -> Error connErrorCodeDefault.ServiceUnavailable
                                          | HttpStatusCode.NotFound            -> Error uri  
                                          | _                                  -> Error connErrorCodeDefault.CofeeMakerUnavailable   
                                          
                                      return errorType     
                with                                                         
                | ex ->                        
                      closeItDpo client message (string ex) //"Chyba v průběhu stahování JŘ DPO."//(string ex) 
                      return Error String.Empty    
            }   
    
    message.msgParam3 pathToDir 
    
    let downloadTimetables (client: HttpClient) = 
        
        let l = filterTimetables |> List.length
        
        filterTimetables 
        |> List.iteri
            (fun i (link, pathToFile) -> 
                                       //vzhledem k nutnosti propustit chybu pri nestahnuti JR (message.msgParam2 link) nepouzito Result.sequence   
                                       let mapErr3 err =                  
                                           function
                                           | Ok value  ->
                                                        value    
                                                        |> List.tryFind (fun item -> (=) err item)
                                                        |> function
                                                            | Some err -> closeItDpo client message err                                                                      
                                                            | None     -> message.msgParam2 link 
                                           | Error err ->
                                                        closeItDpo client message err              

                                       let mapErr2 = 
                                           function
                                           | Ok value  -> value |> ignore
                                           | Error err -> mapErr3 err (Ok listConnErrorCodeDefault) //Ok je legacy drivejsiho reflection a Result.sequence
                                                 
                                       async                                                
                                           {   
                                               progressBarContinuous message i l  //progressBarContinuous  
                                               return! downloadFileTaskAsync client link pathToFile                                                                                                                               
                                           } 
                                           |> Async.Catch
                                           |> Async.RunSynchronously
                                           |> Result.ofChoice  
                                           |> Result.mapErr mapErr2 (lazy message.msgParam2 link)                                                   
            ) 

    downloadTimetables client     
   
    message.msgParam4 pathToDir