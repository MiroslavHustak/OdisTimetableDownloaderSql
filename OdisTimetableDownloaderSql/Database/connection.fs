namespace Database

module Connection =

    open Microsoft.Data.SqlClient

    open Helpers.TryWithRF

    [<Literal>] 
    let internal connString = @"Data Source=Misa\SQLEXPRESS;Initial Catalog=TimetableDownloader;Integrated Security=True;Encrypt=False"

    let internal getConnection message =  
        
        let connection = new SqlConnection(connString)
        connection.Open()
        connection
        
        |> tryWith2 (lazy ())            
        |> function    
            | Ok value -> 
                        value
            | Error _  -> 
                        closeItBaby message message.msg16 
                        new SqlConnection(connString)              

    let internal closeConnection (connection: SqlConnection) message =
       
        connection.Close()
        connection.Dispose()

        |> tryWith2 (lazy ())            
        |> function    
            | Ok value -> value
            | Error _  -> closeItBaby message message.msg16 
