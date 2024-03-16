namespace Helpers

open System
open System.Net.Http
open FsToolkit.ErrorHandling

open Types
open Types.Messages

open Settings.Messages

open Helpers
open Helpers.Builders
            
module Result = 

    let internal mapErr fOk (fErr: Lazy<'a>) =                          
        function
        | Ok value -> value |> fOk
        | Error _  -> fErr.Force()       
                   
    let internal toOption = 
        function   
        | Ok value -> Some value 
        | Error _  -> None  

    let internal fromOption = 
        function   
        | Some value -> Ok value
        | None       -> Error String.Empty  
        
    let internal sequence aListOfResults = //gets the first error - see the book Domain Modelling Made Functional

        let prepend firstR restR =
            match firstR, restR with
            | Ok first, Ok rest   -> Ok (first::rest) | Error err1, Ok _ -> Error err1
            | Ok _, Error err2    -> Error err2
            | Error err1, Error _ -> Error err1

        let initialValue = Ok [] 
        List.foldBack prepend aListOfResults initialValue

    let internal sequence1 aListOfResults =       
        
        aListOfResults 
        |> List.choose (fun item -> item |> Result.toOption)
        |> List.length
        |> function   
            | 0 -> 
                 let err = 
                     aListOfResults 
                     |> List.map
                         (fun item ->
                                    match item with
                                    | Ok _      -> String.Empty
                                    | Error err -> err
                         ) |> List.head //One exception or None is enough for the calculation to fail
                 Error err
            | _ ->
                 let okList = 
                     aListOfResults 
                     |> List.map
                         (fun item -> 
                                    match item with
                                    | Ok value -> value
                                    | _        -> String.Empty 
                         )   
                 Ok okList 

module TryWithRF =

    let internal optionToResultPrint f fPrint : Result<'a, 'b> = 
        f                      
        |> function   
            | Some value -> Ok value 
            | None       -> Error fPrint    

    let internal tryWithLazy pfPrint f2 f1 : Result<'a, Lazy<unit>> =            
        try
            try                 
                f2
            finally
                f1
        with
        | ex -> Error <| lazy (pfPrint (string ex)) 
                      
    let internal tryWith f2 f1 : Result<'a, string> =            
        try
            try                 
                f2
            finally
                f1
        with
        | ex -> Error "Chyba v průběhu stahování JŘ DPO nebo JŘ MDPO nebo JŘ KODIS."//(string ex)

    let internal tryWith2 (f2 : Lazy<unit>) f1 =
          try
              try          
                  Ok f1 
              finally
                  f2.Force() 
          with
          | ex -> Error <| string ex.Message  

    let internal tryWith3 (f2 : Lazy<unit>) f1 =
        try
            try 
                match f1 with
                | Some value -> Ok value
                | None       -> Error "Není připojení k internetu"                
            finally
                f2.Force() 
        with
        | ex -> Error <| string ex.Message  

    let internal tryWithNone x : 'a option = 
        try           
           x
        with
        | _ -> None
    
    let internal closeItDpo (client: HttpClient) (message: Messages) err = 
        message.msgParam1 err      
        Console.ReadKey() |> ignore 
        client.Dispose()
        System.Environment.Exit(1)  

    let internal closeItBaby (message: Messages) err = 
        message.msgParam1 err      
        Console.ReadKey() |> ignore 
        System.Environment.Exit(1)  

module Option =

    let internal ofBool =                           
        function   
        | true  -> Some ()  
        | false -> None

    let internal toBool = 
        function   
        | Some _ -> true
        | None   -> false

    let internal fromBool value =                               
        function   
        | true  -> Some value  
        | false -> None

    let internal ofNull (value: 'nullableValue) =
        match System.Object.ReferenceEquals(value, null) with //The "value" type can be even non-nullable, and ReferenceEquals will still work.
        | true  -> None
        | false -> Some value                             

    let internal ofObj value =
        match value with
        | null -> None
        | _    -> Some value

    let internal ofNullable (value: System.Nullable<'T>) =
        match value.HasValue with
        | true  -> Some value.Value
        | false -> None

    let internal toResult err = 
        function   
        | Some value -> Ok value 
        | None       -> Error err              

    let internal ofStringOption str = 
        str
        |> Option.bind (fun item -> Option.filter (fun item -> not (item.Equals(String.Empty))) (Some (string item))) 
                             
    let internal ofStringObj (value: 'nullableValue) = //NullOrEmpty

        strictStringCheck None
            {
                let!_ = System.Object.ReferenceEquals(value, null) 
                let value = string value 
                let! _ = String.IsNullOrEmpty(value) 

                return Some value
            }

    let internal ofStringObjXXL (value: 'nullableValue) = //NullOrEmpty, NullOrWhiteSpace
    
        strictStringCheck None
            {
                let!_ = System.Object.ReferenceEquals(value, null) 
                let value = string value 
                let! _ = String.IsNullOrEmpty(value) || String.IsNullOrWhiteSpace(value)
    
                return Some value
            }
       
    //************************************************************************

    (*
    The inline keyword in F# is primarily used for inlining code at the call site, which can lead to 
    performance improvements in situations where performance optimization is crucial. The impact of inlining 
    is most pronounced when working with generic functions and operations that involve value types.  
    Inline functions are particularly useful when working with collections and higher-order functions.
    Inline functions are a powerful feature that allows the F# compiler to generate specialized code for
    a function at the call site. This can result in more efficient and optimized code, especially when working 
    with generic functions.
    *)    
                            
module Casting = 
    
    let inline internal castAs<'a> (o: obj) : 'a option =    //the :? operator in F# is used for type testing     srtp pri teto strukture nefunguje
        match Option.ofNull o with
        | Some (:? 'a as result) -> Some result
        | _                      -> None