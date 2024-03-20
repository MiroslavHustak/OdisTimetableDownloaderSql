namespace Database

open System.IO
open Newtonsoft.Json.Linq

open Helpers
open Helpers.CloseApp

open Logging.Logging

module LoggingSQL =
  
    let extractLogEntries () =

        let filePath = "logs/app.log"

        try
            File.ReadAllLines(filePath)
            |> Array.map (fun line -> JArray.Parse(line))       
            |> Array.map 
                (fun item -> 
                            //tady nevadi pripadne String.Empty   
                            let timestamp = string item.[0] //nelze Array.item 0
                            let logName = string item.[1]
                            let message = string item.[2]  
                            //failwith "test extractLogEntries"
                            timestamp, logName, message                     
                ) 
            |> List.ofArray         
            |> List.distinct        
        with
        | ex -> 
              logInfoMsg <| sprintf "102 %s" (string ex.Message)                        
              [] //tady nevadi List.empty jakozto vystup 