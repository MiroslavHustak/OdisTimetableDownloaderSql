[<RequireQualifiedAccess>]
module List.Parallel

open System
open FSharp.Control
open FsToolkit.ErrorHandling
open Microsoft.FSharp.Quotations
open FSharp.Quotations.Evaluator.QuotationEvaluationExtensions

//Just for fun :-)
//Functions seem to be as fast as Array.Parallel.iter/iter2/map/map2 for non-CPU-bound operations
//But I have not tested too intensively....

open System.Linq

let private expr (param : 'a) = Expr.Value(param)  

let private splitListIntoEqualParts (numParts: int) (originalList: 'a list) =   //well, almost equal parts :-)           

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
    
let private numberOfThreads l =  
        
    let numberOfThreads = Environment.ProcessorCount 
        
    match numberOfThreads > 0 with 
    | true  ->                            
             match l >= numberOfThreads with
             | true             -> numberOfThreads
             | false when l > 0 -> l
             | _                -> 0  
    | false ->
             failwith "Cannot count the number of processors available to the current process"
             -1

let iter action list =  
        
    let listToParallel (list : 'a list) = list |> List.iter action            
            
    let l = list |> List.length
    let numberOfThreads = numberOfThreads l   
                                   
    let myList = splitListIntoEqualParts numberOfThreads list                             
                          
    fun i -> <@ async { return listToParallel (%%expr myList |> List.item %%(expr i)) } @>
    |> List.init myList.Length
    |> List.map _.Compile()       
    |> Async.Parallel  
    |> Async.RunSynchronously 
    |> ignore

let iter2<'T1, 'T2> (mapping: 'T1 -> 'T2 -> unit) (xs1: 'T1 list) (xs2: 'T2 list) = 
    
    let listToParallel (xs1, xs2) = (xs1, xs2) ||> List.iter2 mapping    

    let l = xs1 |> List.length    
    let numberOfThreads = numberOfThreads l    
        
    let myList =       
        (splitListIntoEqualParts numberOfThreads xs1, splitListIntoEqualParts numberOfThreads xs2)  
        ||> List.zip                 
                                               
    fun i -> <@ async { return listToParallel (%%expr myList |> List.item %%(expr i)) } @>
    |> List.init myList.Length
    |> List.map _.Compile()       
    |> Async.Parallel  
    |> Async.RunSynchronously
    |> ignore

let map action list =  

    let listToParallel (list : 'a list) = list |> List.map action            
            
    let l = list |> List.length
    let numberOfThreads = numberOfThreads l   
                                   
    let myList = splitListIntoEqualParts numberOfThreads list                             
                          
    fun i -> <@ async { return listToParallel (%%expr myList |> List.item %%(expr i)) } @>
    |> List.init myList.Length
    |> List.map _.Compile()       
    |> Async.Parallel      
    |> Async.RunSynchronously
    |> List.ofArray
    |> List.concat
 
let map2<'T1, 'T2, 'R> (mapping: 'T1 -> 'T2 -> 'R) (xs1: 'T1 list) (xs2: 'T2 list) =   
        
    let listToParallel (xs1, xs2) = (xs1, xs2) ||> List.map2 mapping    

    let l = xs1 |> List.length        
    let numberOfThreads = numberOfThreads l    
        
    let myList =       
        (splitListIntoEqualParts numberOfThreads xs1, splitListIntoEqualParts numberOfThreads xs2)  
        ||> List.zip                 
                                               
    fun i -> <@ async { return listToParallel (%%expr myList |> List.item %%(expr i)) } @>
    |> List.init myList.Length
    |> List.map _.Compile()       
    |> Async.Parallel  
    |> Async.RunSynchronously
    |> List.ofArray
    |> List.concat