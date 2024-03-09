module Connection

open Microsoft.Data.SqlClient

open ErrorHandling
open ErrorHandling.TryWithRF


[<Literal>] 
let internal connString = @"Data Source=Misa\SQLEXPRESS;Initial Catalog=TimetableDownloader;Integrated Security=True;Encrypt=False"

let internal getConnection message =  
    
    let result = 
        let connection = new SqlConnection(connString)
        connection.Open()
        connection
    
    tryWith2 (lazy ()) result           
    |> function    
        | Ok value -> 
                    value
        | Error _  -> 
                    closeItBaby message message.msg16 
                    new SqlConnection(connString)              

let internal closeConnection (connection: SqlConnection) message =
    
    let result = 
        connection.Close()
        connection.Dispose()

    tryWith2 (lazy ()) result           
    |> function    
        | Ok value -> value
        | Error _  -> closeItBaby message message.msg16 
