namespace Database

open System
open System.Data
open FsToolkit.ErrorHandling
open Microsoft.Data.SqlClient

open Types.Messages

module InsertInto = 

    let internal insert getConnection closeConnection list message =

        let queryDeleteAll = "DELETE FROM TimetableLinks"
         
        let queryInsert = 
             "           
             INSERT INTO TimetableLinks 
                (
                    OldPrefix, NewPrefix, StartDate, EndDate, 
                    TotalDateInterval,VT_Suffix, JS_GeneratedString, 
                    CompleteLink, FileToBeSaved
                ) 
             VALUES
                (
                    @OldPrefix, @NewPrefix, @StartDate, @EndDate, 
                    @TotalDateInterval, @VT_Suffix, @JS_GeneratedString, 
                    @CompleteLink, @FileToBeSaved
                );
        "                
        try
            let connection: SqlConnection = getConnection message 
            
            try                 
                use cmdDeleteAll = new SqlCommand(queryDeleteAll, connection)             
                use cmdInsert = new SqlCommand(queryInsert, connection)   
                
                let parameterStart = new SqlParameter()                 
                parameterStart.ParameterName <- "@StartDate"  
                parameterStart.SqlDbType <- SqlDbType.Date  

                let parameterEnd = new SqlParameter() 
                parameterEnd.ParameterName <- "@EndDate"  
                parameterEnd.SqlDbType <- SqlDbType.Date  

                cmdDeleteAll.ExecuteNonQuery() |> ignore //number of affected rows
                
                list      
                |> List.iter
                    (fun item ->                        
                               cmdInsert.Parameters.Clear() // Clear parameters for each iteration     
                               cmdInsert.Parameters.AddWithValue("@OldPrefix", item |> List.item 0) |> ignore
                               cmdInsert.Parameters.AddWithValue("@NewPrefix", item |> List.item 1) |> ignore

                               parameterStart.Value <- item |> List.item 2
                               cmdInsert.Parameters.Add(parameterStart) |> ignore

                               parameterEnd.Value <- item |> List.item 3                                
                               cmdInsert.Parameters.Add(parameterEnd) |> ignore

                               cmdInsert.Parameters.AddWithValue("@TotalDateInterval", item |> List.item 4) |> ignore
                               cmdInsert.Parameters.AddWithValue("@VT_Suffix", item |> List.item 5) |> ignore
                               cmdInsert.Parameters.AddWithValue("@JS_GeneratedString", item |> List.item 6) |> ignore
                               cmdInsert.Parameters.AddWithValue("@CompleteLink", item |> List.item 7) |> ignore
                               cmdInsert.Parameters.AddWithValue("@FileToBeSaved", item |> List.item 8) |> ignore
    
                               cmdInsert.ExecuteNonQuery() |> ignore //number of affected rows
                               
                    )                
            finally
                closeConnection connection message
        with
        | ex ->
              message.msgParam1 <| string ex.Message
              Console.ReadKey() |> ignore
              System.Environment.Exit(1)

             