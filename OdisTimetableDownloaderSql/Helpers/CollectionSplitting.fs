namespace Helpers

open System
open FsToolkit.ErrorHandling

open Types.Messages
open Logging.Logging

open Helpers.CloseApp
open Settings.SettingsKODIS

module CollectionSplitting =

    let internal splitListByPrefix message (list: string list) : string list list = 
                
        let prefix = fun (item: string) -> item.Substring(0, lineNumberLength)

        try
            (prefix, list)
            ||> List.groupBy //List.groupBy automatically uses the first element of the tuple as the key 
            |> List.map snd
        with
        | ex ->    
              logInfoMsg <| sprintf "024 %s" (string ex.Message) 
              closeItBaby message message.msg16 
              [ [] ]   
                        
    let internal splitListByPrefixExplanation message (list: string list) : string list list = 
                
        let prefix = fun (item: string) -> item.Substring(0, lineNumberLength)
        try
            list 
            |> List.groupBy (fun item -> prefix item)
            |> List.map (fun item -> snd item)        
        with
        | ex ->    
              logInfoMsg <| sprintf "025 %s" (string ex.Message) 
              closeItBaby message message.msg16 
              [ [] ]   
    
    let internal splitListIntoEqualParts (numParts: int) (originalList: 'a list) =   //almost equal parts :-)    
            
        //[<TailCall>] vyzkouseno separatne, bez varovnych hlasek
        let rec splitAccumulator remainingList partsAccumulator acc =
    
            match remainingList with
            | [] -> 
                    partsAccumulator |> List.rev 
            | _  ->                     
                    let currentPartLength =
    
                        let partLength list n = 

                            let totalLength = list |> List.length 
                            let partLength = totalLength / n    
                              
                            totalLength % n > 0
                            |> function
                                | true  -> partLength + 1
                                | false -> partLength 
    
                        match (=) acc numParts with
                        | true  -> partLength originalList numParts    
                        | false -> partLength remainingList acc                                 
        
                    let (part, rest) = remainingList |> List.splitAt currentPartLength 

                    splitAccumulator rest (part :: partsAccumulator) (acc - 1)
                      
        splitAccumulator originalList [] numParts
    
    let internal numberOfThreads message l =  
        
        let numberOfThreads = Environment.ProcessorCount //nesu exceptions
        
        match numberOfThreads > 0 with //non-nullable
        | true  ->                            
                 match l >= numberOfThreads with
                 | true             -> numberOfThreads
                 | false when l > 0 -> l
                 | _                -> 0  
        | false ->
                 logInfoMsg <| sprintf "026 %s" "Chyba při rozdělování listu pro multi-threading." 
                 closeItBaby message "Chyba při rozdělování listu pro multi-threading."; -1

