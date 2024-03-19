namespace Database

open Microsoft.Data.SqlClient

open Logging.Logging
open Helpers.CloseApp

module Connection =

    [<Literal>] 
    let internal connString = @"Data Source=Misa\SQLEXPRESS;Initial Catalog=TimetableDownloader;Integrated Security=True;Encrypt=False"

    let internal getConnection message =  
        try
            let connection = new SqlConnection(connString)
            connection.Open()
            connection
        with
        | ex ->
              logInfoMsg <| sprintf "031 %s" (string ex.Message) 
              closeItBaby message message.msg16 
              new SqlConnection(connString)              

    let internal closeConnection (connection: SqlConnection) message =
       
        try
            connection.Close()
            connection.Dispose()
        with
        | ex -> 
              logInfoMsg <| sprintf "032 %s" (string ex.Message) 
              closeItBaby message message.msg16  
