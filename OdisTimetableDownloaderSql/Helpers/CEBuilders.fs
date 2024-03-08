﻿namespace CEBuilders
           
module Builders =
   
    [<Struct>]
    type internal MyTypeBuilder (param: string option) =        
         member _.Bind(condition, nextFunc) = 
             match condition with
             | false -> nextFunc() 
             | true  -> param
         member _.Return x = x  
     
    let internal strictStringCheck = MyTypeBuilder 
        
    [<Struct>]
    type internal MyTypeBuilderGeneric (param: obj) =        
         member _.Bind(condition, nextFunc) = 
             match condition with
             | false -> nextFunc() 
             | true  -> param
         member _.Return x = x  
     
    let internal pyramidOfInferno = MyTypeBuilderGeneric 
    
    //**************************************************************************************

    [<Struct>]
    type internal MyBuilder = MyBuilder with    
         member _.Bind(condition, nextFunc) =
             match fst condition with
             | false -> snd condition
             | true  -> nextFunc()  
         member _.Return x = x
         //member _.Zero x = x

    let internal pyramidOfHell = MyBuilder

    //**************************************************************************************

    [<Struct>]
     type internal Builder2 = Builder2 with    
          member _.Bind((optionExpr, err), nextFunc) =
              match optionExpr with
              | Some value -> nextFunc value 
              | _          -> err  
          member _.Return x : 'a = x
          //member _.Zero x = x

     let internal pyramidOfDoom = Builder2

    //**************************************************************************************

    type internal Reader<'e, 'a> = 'e -> 'a

    [<Struct>] 
    type internal ReaderBuilder = ReaderBuilder with
        member __.Bind(m, f) = fun env -> f (m env) env      
        member __.Return x = fun _ -> x
        member __.ReturnFrom x = x
        //member __.Zero x = x

    let internal reader = ReaderBuilder 
    

