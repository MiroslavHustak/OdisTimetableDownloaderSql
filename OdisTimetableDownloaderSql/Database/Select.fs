namespace Database

open Microsoft.Data.SqlClient
open FsToolkit.ErrorHandling

open Helpers

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
                    (fun _ -> seq { (Casting.castAs<string> reader.["CompleteLink"], Casting.castAs<string> reader.["FileToBeSaved"]) }) 
                |> List.ofSeq  
                |> List.map 
                    (fun (link, file) ->
                                       match (link, file) with
                                       | Some link, Some file -> Some (link, file)
                                       | _                    -> None
                    )
                |> List.choose id
              
            finally
                closeConnection connection message
        with
        | ex -> 
              printfn "%s" ex.Message //TODO Result type
              []

