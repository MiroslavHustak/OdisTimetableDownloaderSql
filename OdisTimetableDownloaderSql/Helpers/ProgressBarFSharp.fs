﻿namespace Helpers

open System

open Types.Messages
open Logging.Logging
open Helpers.CloseApp

module ProgressBarFSharp =

    // ************************************************************************************************************
    // Adapted from C# code under MIT License, Copyright (c) 2017 Daniel Wolf, https://gist.github.com/DanielSWolf
    // ************************************************************************************************************
    
    let private (++) a = (+) a 1

    let inline private updateProgressBar (message: Messages) (currentProgress : int) (totalProgress : int) : unit =

        let bytes = //437 je tzv. Extended ASCII            
            try
                (System.Text.Encoding.GetEncoding(437).GetBytes("█")) 
            with 
            | ex ->
                  logInfoMsg <| sprintf "027 %s" (string ex.Message)
                  [||] 
                   
        let output =          
            try
                System.Text.Encoding.GetEncoding(852).GetChars(bytes)
            with 
            | ex ->
                  logInfoMsg <| sprintf "028 %s" (string ex.Message)
                  [||] 
        
        let progressBar = 

            let barWidth = 50 //nastavit delku dle potreby            
            let percentComplete = (/) ((*) currentProgress 101) ((++) totalProgress) // :-) //101 proto, ze pri deleni 100 to po zaokrouhleni dalo jen 99%                    
            let barFill = (/) ((*) currentProgress barWidth) totalProgress // :-)  messing about
               
            let characterToFill = string (Array.item 0 output) //moze byt baj aji "#"
            
            let bar = 
                try                   
                    String.replicate barFill characterToFill
                with
                | ex -> 
                      logInfoMsg <| sprintf "029 %s" (string ex.Message)
                      String.Empty
                               
            let remaining = 
                try
                    String.replicate (barWidth - (++) barFill) "*"
                with
                | ex -> 
                      logInfoMsg <| sprintf "030 %s" (string ex.Message)
                      String.Empty
              
            sprintf "<%s%s> %d%%" bar remaining percentComplete 

        match (=) currentProgress totalProgress with
        | true  -> message.msgParam8 progressBar
        | false -> message.msgParam9 progressBar
                                 
    let internal progressBarContinuous (message: Messages) (currentProgress : int) (totalProgress : int) : unit =

        match currentProgress < (-) totalProgress 1 with
        | true  -> 
                 updateProgressBar message currentProgress totalProgress
        | false ->              
                 Console.Write("\r" + new string(' ', (-) Console.WindowWidth 1) + "\r")
                 Console.CursorLeft <- 0         
               

                  