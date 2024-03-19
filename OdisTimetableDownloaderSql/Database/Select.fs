namespace Database

open System
open FsToolkit.ErrorHandling
open Microsoft.Data.SqlClient

open Helpers
open Helpers.CloseApp

open Types.Messages
open Logging.Logging

open DomainModelling.Dto
open DomainModelling.DomainModel
open TransformationLayers.TransformationLayerGet

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
                    (fun (link, file)
                        ->                                       
                         let record : DbDataDtoGet = 
                             {
                                 completeLink = link
                                 fileToBeSaved = file
                             }

                         let result = dbDataTransformLayerGet record

                         (result.completeLink, result.fileToBeSaved)
                         |> function
                             | Some link, Some file 
                                 -> 
                                  Ok (link, file)
                             | _                   
                                 ->
                                  //failwith "Chyba při čtení z databáze" 
                                  Error "Chyba při čtení z databáze"
                    )
                |> Result.sequence
                |> function   
                    | Ok value  -> 
                                 value
                    | Error err -> 
                                 logInfoMsg <| sprintf "019 %s" err
                                 closeItBaby Settings.Messages.messagesDefault err
                                 []
            finally
                closeConnection connection message
        with
        | ex -> 
              logInfoMsg <| sprintf "020 %s" (string ex.Message)
              closeItBaby Settings.Messages.messagesDefault "Chyba při čtení z databáze"             
              []

