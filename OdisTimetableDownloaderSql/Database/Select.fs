namespace Database

open System
open FsToolkit.ErrorHandling
open Microsoft.Data.SqlClient

open Helpers
open Types.Messages

open DomainModelling.DtoGet
open DomainModelling.DomainModel
open TransformationLayers.TransormationLayerGet

module Select =

    let internal select getConnection closeConnection message pathToDir itvfCall =
        
        try
            let connection: SqlConnection = getConnection message
                     
            try  
                //query je tady volani ITVF
                let query = sprintf "SELECT * FROM %s" itvfCall

                use cmdCallITVFunction = new SqlCommand(query, connection)          
                
                let reader = cmdCallITVFunction.ExecuteReader() 
                
                //V pripade pouziti Oracle zkontroluj skutecny typ sloupce v .NET   
                //let columnType = reader.GetFieldType(reader.GetOrdinal("OperatorID"))
                //printfn "Column Type: %s" columnType.Name
    
                Seq.initInfinite (fun _ -> reader.Read() && reader.HasRows = true)
                |> Seq.takeWhile ((=) true) 
                |> Seq.collect
                    (fun _ -> seq { reader.["CompleteLink"], reader.["FileToBeSaved"] }) 
                |> List.ofSeq  
                |> List.map 
                    (fun (link, file) ->                                       
                                       let record : DbDataDtoGet = 
                                           {
                                               completeLink = link
                                               fileToBeSaved = file
                                           }

                                       let result = dbDataTransferLayerGet record

                                       (result.completeLink, result.fileToBeSaved)
                                       |> function
                                           | Some link, Some file -> 
                                                                   Some (link, file)
                                           | _                    ->
                                                                   failwith "Chyba při čtení z databáze" //zcela vyjimecne //TODO, kdyz bude cas, predelat na result type 
                                                                   None
                    )
                |> List.choose id
              
            finally
                closeConnection connection message
        with
        | ex -> 
              message.msgParam1 <| string ex.Message
              Console.ReadKey() |> ignore
              System.Environment.Exit(1)
              []

