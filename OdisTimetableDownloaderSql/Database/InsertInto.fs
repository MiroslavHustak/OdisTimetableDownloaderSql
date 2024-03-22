namespace Database

open System
open System.Data
open System.Globalization
open FsToolkit.ErrorHandling
open Microsoft.Data.SqlClient

open Settings.Messages

open Helpers.Builders
open Helpers.CloseApp
open Helpers.TryParserDate

open Logging.Logging

open DataModelling.Dto
open DataModelling.DataModel

module InsertInto = 

    let internal insert getConnection closeConnection (dataToBeInserted : DbDtoSend list) =

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
            let connection: SqlConnection = getConnection () 
            
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
                closeConnection connection 
        with
        | ex ->
              msgParam1 <| string ex.Message
              logInfoMsg <| sprintf "033 %s" (string ex.Message)
              closeItBaby (string ex.Message)

    let internal insertLogEntries getConnection2 closeConnection =

        let dataToBeInserted = Database.LoggingSQL.extractLogEntries () 

        match dataToBeInserted.Length with
        | 0 -> 
             ()
        | _ -> 
             //let queryDeleteAll = "DELETE FROM LogEntries2"
             
             //https://learn.microsoft.com/en-us/sql/t-sql/statements/merge-transact-sql?view=sql-server-ver16
             //v tabulce budou jen nove hodnoty, hodnoty, ktere tam uz jsou, se z logu nebudou znovu nacitat         
             let queryInsert =        
                 "
                 MERGE INTO LogEntries2 AS target
                 USING (VALUES (@Timestamp, @Logname, @Message))
                 AS source ([Timestamp], Logname, [Message])
                 ON target.[Timestamp] = source.[Timestamp]
                     AND target.Logname = source.Logname
                     AND target.[Message] = source.[Message]
                 WHEN MATCHED THEN
                 UPDATE SET target.[Timestamp] = source.[Timestamp],
                    target.Logname = source.Logname,
                    target.[Message] = source.[Message]
                 WHEN NOT MATCHED BY target THEN
                    INSERT ([Timestamp], Logname, [Message])
                    VALUES (source.[Timestamp], source.Logname, source.[Message]);                 
                " 

             let queryInsert1 =  //nepouzivano               
                 "
                 INSERT INTO LogEntries2 ([Timestamp], Logname, [Message])
                 VALUES (@Timestamp, @Logname, @Message)                 
                "  
                 
             try
                 let connection: SqlConnection = getConnection2 () 
                
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
                                        | :? System.ArgumentNullException as _ -> DateTime.MinValue //TODO pokud mne neco napadne, co lepsiho tady dat                                                                                                                                                                       
                                        | :? System.FormatException as _       -> DateTime.MinValue //TODO pokud mne neco napadne, co lepsiho tady dat
                                        | _                                    -> DateTime.MinValue //TODO pokud mne neco napadne, co lepsiho tady dat
                                    
                                    //cmdDeleteAll.ExecuteNonQuery() |> ignore //number of affected rows
                                    cmdInsert.Parameters.Clear() // Clear parameters for each iteration    
                                    parameterTimeStamp.Value <- timestamp                             
                                    cmdInsert.Parameters.Add(parameterTimeStamp) |> ignore    

                                    cmdInsert.Parameters.AddWithValue("@Logname", logName) |> ignore
                                    cmdInsert.Parameters.AddWithValue("@Message", message) |> ignore
                                              
                                    cmdInsert.ExecuteNonQuery() |> ignore //number of affected rows                                                                
                        )                
                     
                     msg19 ()   
                                      
                 finally
                     closeConnection connection 
             with
             | ex ->
                   msgParam1 <| string ex.Message
                   logInfoMsg <| sprintf "101 %s" (string ex.Message)
                   closeItBaby (string ex.Message)