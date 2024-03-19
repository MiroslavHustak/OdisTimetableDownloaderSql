﻿namespace Database

open Microsoft.Data.SqlClient

open Logging.Logging
open Helpers.CloseApp

module Connection =    

    let internal getConnection message =  

        let connString = @"Data Source=Misa\SQLEXPRESS;Initial Catalog=TimetableDownloader;Integrated Security=True;Encrypt=False"

        try            
            let connection = new SqlConnection(connString)
            connection.Open()
            connection
        with
        | ex ->
              logInfoMsg <| sprintf "031 %s" (string ex.Message) 
              closeItBaby message message.msg16 
              new SqlConnection(connString)     
              
    let internal getConnection2 message =  

        let connString2 = @"Data Source=Misa\SQLEXPRESS;Initial Catalog=Logging;Integrated Security=True;Encrypt=False"

        try
            let connection = new SqlConnection(connString2)
            connection.Open()
            connection
        with
        | ex ->
              logInfoMsg <| sprintf "031A %s" (string ex.Message) 
              closeItBaby message message.msg16 
              new SqlConnection(connString2)     

    let internal closeConnection (connection: SqlConnection) message =  
        
        try
            connection.Close()
            connection.Dispose()
        with
        | ex -> 
              logInfoMsg <| sprintf "032 %s" (string ex.Message) 
              closeItBaby message message.msg16  
