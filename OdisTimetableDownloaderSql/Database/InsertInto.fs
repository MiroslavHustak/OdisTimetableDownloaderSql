﻿namespace Database

open System
open System.Data
open FsToolkit.ErrorHandling
open Microsoft.Data.SqlClient

open Types.Messages
open Helpers.Builders
open Helpers.CloseApp

open Logging.Logging

open DomainModelling.Dto
open DomainModelling.DomainModel
open Helpers.TryParserDate
open System.Globalization

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
                               (*   
                               let (startDate, endDate) =   

                                   pyramidOfDoom
                                       {
                                           let! startDate = item.startDate, (DateTime.MinValue, DateTime.MinValue)                                                      
                                           let! endDate = item.endDate, (DateTime.MinValue, DateTime.MinValue)                             
                                          
                                           return (startDate, endDate)
                                       }
                               *)
                               cmdInsert.Parameters.Clear() // Clear parameters for each iteration     
                               cmdInsert.Parameters.AddWithValue("@OldPrefix", item.oldPrefix) |> ignore
                               cmdInsert.Parameters.AddWithValue("@NewPrefix", item.newPrefix) |> ignore

                               parameterStart.Value <- item.startDate
                               cmdInsert.Parameters.Add(parameterStart) |> ignore

                               parameterEnd.Value <- item.endDate                                
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
              logInfoMsg <| sprintf "033 %s" (string ex.Message)
              closeItBaby message (string ex.Message)

    let internal insertLogEntries getConnection closeConnection message =

        let dataToBeInserted = Database.LoggingSQL.extractLogEntries () 

        match dataToBeInserted.Length with
        | 0 -> 
             printfn "Žádné nové log entries"
        | _ -> 
             //let queryDeleteAll = "DELETE FROM LogEntries2"
             
             let queryInsert1 =                 
                 "
                 MERGE INTO LogEntries2 AS target
                 USING 
                 (
                    VALUES (@Timestamp, @Logname, @Message)
                 )
                 AS source ([Timestamp], Logname, [Message])
                 ON target.[Timestamp] = source.[Timestamp]
                 AND target.Logname = source.Logname
                 AND target.[Message] = source.[Message]
                 WHEN MATCHED THEN
                    UPDATE SET target.[Timestamp] = 
                    source.[Timestamp],
                    target.Logname = source.Logname,
                    target.[Message] = source.[Message]
                 WHEN NOT MATCHED BY TARGET THEN
                    INSERT ([Timestamp], Logname, [Message])
                    VALUES (source.[Timestamp], source.Logname, source.[Message]);
                " 

             let queryInsert =                 
                 "
                 INSERT INTO LogEntries2 ([Timestamp], Logname, [Message])
                 VALUES (@Timestamp, @Logname, @Message)                 
                "  
                 
             try
                 let connection: SqlConnection = getConnection message 
                
                 try                        
                     //use cmdDeleteAll = new SqlCommand(queryDeleteAll, connection)             
                     use cmdInsert = new SqlCommand(queryInsert, connection)   
                    
                     let parameterTimeStamp = new SqlParameter()                 
                     parameterTimeStamp.ParameterName <- "@Timestamp"  
                     parameterTimeStamp.SqlDbType <- SqlDbType.DateTime  
                    
                     dataToBeInserted     
                     |> List.iter
                         (fun item -> 
                                    let (timestamp, logName, message) = item 
                                    
                                    let timestamp = 
                                        try 
                                            DateTime.ParseExact(timestamp, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) 
                                        with
                                        | :? System.ArgumentNullException as _ -> DateTime.MinValue                                                                                                                                                                        
                                        | :? System.FormatException as _       -> DateTime.MinValue 
                                        | _                                    -> DateTime.MinValue //TODO pokud mne neco napadne, co lepsiho tady dat
                                    
                                    //cmdDeleteAll.ExecuteNonQuery() |> ignore //number of affected rows
                                    cmdInsert.Parameters.Clear() // Clear parameters for each iteration    
                                    parameterTimeStamp.Value <- timestamp                             
                                    cmdInsert.Parameters.Add(parameterTimeStamp) |> ignore    
                                    cmdInsert.Parameters.AddWithValue("@Logname", logName) |> ignore
                                    cmdInsert.Parameters.AddWithValue("@Message", message) |> ignore
                                              
                                    cmdInsert.ExecuteNonQuery() |> ignore //number of affected rows     
                    
                                                            
                        )                
                     printfn "Log entries vloženy do databáze"     
                                      
                 finally
                     closeConnection connection message
             with
             | ex ->
                   message.msgParam1 <| string ex.Message
                   logInfoMsg <| sprintf "101 %s" (string ex.Message)
                   closeItBaby message (string ex.Message)