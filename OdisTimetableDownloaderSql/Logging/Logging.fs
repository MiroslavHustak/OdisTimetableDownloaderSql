namespace Logging

open System
open Newtonsoft.Json
open NReco.Logging.File
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection

module Logging =     

    // Function to format log entry as JSON array
    let private formatLogEntry (msg: LogMessage) =

        try
            let sb = System.Text.StringBuilder()
            use sw = new System.IO.StringWriter(sb)
            use jsonWriter = new JsonTextWriter(sw)

            jsonWriter.WriteStartArray()
            jsonWriter.WriteValue(string DateTime.Now)
            //jsonWriter.WriteValue(string msg.LogLevel)
            jsonWriter.WriteValue(msg.LogName)
            //jsonWriter.WriteValue(msg.EventId.Id)
            jsonWriter.WriteValue(msg.Message)
            jsonWriter.WriteEndArray()

            string sb    
        with
        | ex -> 
              printfn "%s" "Je třeba zavolat programátora, tato chyba není zaznamenána v log file. Error 2000."
              printfn "%s" <| string ex.Message //proste s tim nic nezrobime, kdyz to nebude fungovat...
              String.Empty
               

    //***************************Log files******************************
    
    // Create a new instance of LoggerFactory
    let private loggerFactory = 
        LoggerFactory.Create(
            fun builder ->                                        
                         builder.AddFile(
                             "logs/app.log", 
                             fun fileLoggerOpts
                                 ->
                                  fileLoggerOpts.FormatLogEntry <- formatLogEntry
                             ) |> ignore
        )
       
    // Create a logger instance
    let private logger = loggerFactory.CreateLogger("TimetableDownloader")
   
    let internal logInfoMsg msg = logger.LogInformation(msg)