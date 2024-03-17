namespace Database

open Microsoft.Data.SqlClient

open Logging.Logging
open Helpers.TryWithRF

module Connection =

    [<Literal>] 
    let internal connString = @"Data Source=Misa\SQLEXPRESS;Initial Catalog=TimetableDownloader;Integrated Security=True;Encrypt=False"

    let internal getConnection message =  
        
        let connection = new SqlConnection(connString)
        connection.Open()
        connection
        
        |> tryWith2 (lazy ())            
        |> function    
            | Ok value  -> 
                         value
            | Error err -> 
                         logInfoMsg <| sprintf "031 %s" err 
                         closeItBaby message message.msg16 
                         new SqlConnection(connString)              

    let internal closeConnection (connection: SqlConnection) message =
       
        connection.Close()
        connection.Dispose()

        |> tryWith2 (lazy ())            
        |> function    
            | Ok value  -> 
                         value
            | Error err -> 
                         logInfoMsg <| sprintf "032 %s" err 
                         closeItBaby message message.msg16  
