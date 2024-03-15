namespace Database

open System
open System.Data
open FsToolkit.ErrorHandling
open Microsoft.Data.SqlClient

open Types.Messages
open Helpers.Builders
open DomainModelling.Dto
open DomainModelling.DomainModel


module InsertInto = 

    let internal insert getConnection closeConnection (dataToBeInserted : DbDataDtoSend list) message =

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
                
                dataToBeInserted     
                |> List.iter
                    (fun item -> 
                               let (startDate, endDate) =   

                                   pyramidOfDoom
                                       {
                                           let! startDate = item.startDate, (DateTime.MinValue, DateTime.MinValue)                                                      
                                           let! endDate = item.endDate, (DateTime.MinValue, DateTime.MinValue)                             
                                          
                                           return (startDate, endDate)
                                       }

                               cmdInsert.Parameters.Clear() // Clear parameters for each iteration     
                               cmdInsert.Parameters.AddWithValue("@OldPrefix", item.oldPrefix) |> ignore
                               cmdInsert.Parameters.AddWithValue("@NewPrefix", item.newPrefix) |> ignore

                               parameterStart.Value <- startDate
                               cmdInsert.Parameters.Add(parameterStart) |> ignore

                               parameterEnd.Value <- endDate                                
                               cmdInsert.Parameters.Add(parameterEnd) |> ignore

                               cmdInsert.Parameters.AddWithValue("@TotalDateInterval", item.totalDateInterval) |> ignore
                               cmdInsert.Parameters.AddWithValue("@VT_Suffix", item.suffix) |> ignore
                               cmdInsert.Parameters.AddWithValue("@JS_GeneratedString", item.jsGeneratedString) |> ignore
                               cmdInsert.Parameters.AddWithValue("@CompleteLink", item.completeLink) |> ignore
                               cmdInsert.Parameters.AddWithValue("@FileToBeSaved", item.fileToBeSaved) |> ignore       
                           
                               cmdInsert.ExecuteNonQuery() |> ignore //number of affected rows                               
                    )                
            finally
                closeConnection connection message
        with
        | ex ->
              message.msgParam1 <| string ex.Message
              Console.ReadKey() |> ignore
              System.Environment.Exit(1)

             