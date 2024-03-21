﻿//namespace Helpers

[<RequireQualifiedAccess>]
module List.Parallel

open System
open FSharp.Control
open FsToolkit.ErrorHandling
open Microsoft.FSharp.Quotations
open FSharp.Quotations.Evaluator.QuotationEvaluationExtensions

open Microsoft.FSharp.Reflection
open System.IO

//Just for fun :-)
//Functions seem to be as fast as Array.Parallel.iter/map for non-CPU-bound operations
//But I have not tested too intensively....

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
    
                  match acc.Equals(numParts) with
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
       
    let listToParallel list = list |> List.iter action
                 
    let l = list |> List.length
    let numberOfThreads = numberOfThreads l   
                                   
    let myList = splitListIntoEqualParts numberOfThreads list                             
                          
    fun i -> <@ async { return listToParallel (%%expr myList |> List.item %%(expr i)) } @>
    |> List.init myList.Length
    |> List.map _.Compile()      
    |> Async.Parallel  
    |> Async.RunSynchronously 
    |> ignore

let iter2<'a, 'b> (mapping: 'a -> 'b -> unit) (xs1: 'a list) (xs2: 'b list) = 
    
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

let map (action: 'a -> 'b) (list: 'a list) =
   
    let listToParallel (list : 'a list) = list |> List.map action 
            
    let l = list |> List.length
    let numberOfThreads = numberOfThreads l   
                                   
    let myList : 'a list list = splitListIntoEqualParts numberOfThreads list 
    
    fun i -> <@ async { return listToParallel (%%expr myList |> List.item %%(expr i)) } @>
    |> List.init myList.Length
    |> List.map _.Compile()       
    |> Async.Parallel      
    |> Async.RunSynchronously
    |> List.ofArray
    |> List.concat
 
let map2<'a, 'b, 'c> (mapping: 'a -> 'b -> 'c) (xs1: 'a list) (xs2: 'b list) =   
        
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