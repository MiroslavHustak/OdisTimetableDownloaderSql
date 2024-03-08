namespace CollectionSplitting

open System
open FsToolkit.ErrorHandling

open SettingsKODIS
open Types.Messages
open ErrorHandling.TryWithRF

module CollectionSplitting =

    let internal splitListByPrefix message (list: string list) : string list list = 
                
        let prefix = fun (item: string) -> item.Substring(0, lineNumberLength)

        (prefix, list)
        ||> List.groupBy //List.groupBy automatically uses the first element of the tuple as the key 
        |> List.map snd
        |> tryWith2 (lazy ())            
        |> function    
            | Ok value -> 
                        value
            | Error _  -> 
                        closeItBaby message message.msg16 
                        [ [] ]   
                        
    let internal splitListByPrefixExplanation message (list: string list) : string list list = 
                
        let prefix = fun (item: string) -> item.Substring(0, lineNumberLength)
        
        list 
        |> List.groupBy (fun item -> prefix item)
        |> List.map (fun item -> snd item)        
        |> tryWith2 (lazy ())            
        |> function    
            | Ok value -> 
                        value
            | Error _  -> 
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
                 closeItBaby message "Chyba při rozdělování listu pro multi-threading."; -1

