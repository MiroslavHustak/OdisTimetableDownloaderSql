namespace Helpers

open System.Net.NetworkInformation
open System.Windows

module ProgressBarFSharp =

    // ************************************************************************************************************
    // Adapted from C# code under MIT License, Copyright (c) 2017 Daniel Wolf, https://gist.github.com/DanielSWolf
    // ************************************************************************************************************

    open System

    open Types.Messages

    open Helpers
    open Helpers.TryWithRF

    let private (++) a = (+) a 1

    let inline private updateProgressBar (message: Messages) (currentProgress : int) (totalProgress : int) : unit =

        let bytes = //437 je tzv. Extended ASCII            
            match tryWith2 (lazy ()) (System.Text.Encoding.GetEncoding(437).GetBytes("█")) with Ok value -> value | Error _ -> [||]
                   
        let output =          
            match tryWith2 (lazy ()) (System.Text.Encoding.GetEncoding(852).GetChars(bytes)) with Ok value -> value | Error _ -> [||]
        
        let progressBar = 

            let barWidth = 50 //nastavit delku dle potreby            
            let percentComplete = (/) ((*) currentProgress 101) ((++) totalProgress) // :-) //101 proto, ze pri deleni 100 to po zaokrouhleni dalo jen 99%                    
            let barFill = (/) ((*) currentProgress barWidth) totalProgress // :-)  messing about
               
            let characterToFill = string (Array.item 0 output) //moze byt baj aji "#"
            
            let bar = 
                match tryWith2 (lazy ()) (String.replicate barFill characterToFill) with Ok value -> value | Error _ -> String.Empty
                
            let remaining = 
                match tryWith2 (lazy ()) ( String.replicate (barWidth - (++) barFill) "*") with Ok value -> value | Error _ -> String.Empty
              
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
               

                  