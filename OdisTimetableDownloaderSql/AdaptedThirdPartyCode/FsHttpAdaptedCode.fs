namespace FsHttpAdaptedCode

module FsHttpAdaptedCode = 
    
    open System
    open System.IO
    open System.Threading

    open System.Windows.Forms
    open System.Net.NetworkInformation

    open FsHttp
    open FSharp.Control
    open FsToolkit.ErrorHandling

    open Types

     //************** My code addition start ****************
    let private processor () = 
        MailboxProcessor.Start
            (fun inbox 
                ->
                   let rec loop isFirst =
                       async
                           {
                               let! msg = inbox.Receive()
                               match msg with
                               | First(x) when isFirst 
                                           ->
                                            let result = MessageBox.Show("Není připojení k internetu. Až se obnoví, stahování příslušných souborů bude pokračovat.", "No jéje, zase problém ...", MessageBoxButtons.OK) 

                                            match result = DialogResult.OK with 
                                            | true  -> () //System.Environment.Exit(1)
                                            | false -> ()

                                            return! loop false // Set isFirst to false to ignore subsequent messages
                               | _         ->                      
                                            return! loop isFirst
                           }

                   loop true // Start with isFirst set to true
           ) 

    //For educational purposes
    let private agent () = 
        MailboxProcessor.Start
            (fun inbox 
                ->          
                let rec messageLoop () =
                    async
                        {
                            let! msg = inbox.Receive() 
                            return! messageLoop ()
                        }
                    
                messageLoop ()
            )                                                                                                                                                      

    let internal responseSaveFileAsync counterAndProgressBarPost path response = 

        let cts = new CancellationTokenSource()
        let token = cts.Token    

        //************** My code addition end  ****************

        //Third part code from the F# HTTP library by @SchlenkR and @dawedawe
        //https://github.com/fsprojects/FsHttp/blob/0526657d7c456aa2ab70352ab2919222a04c82d5/src/FsHttp/Helper.fs
        //https://github.com/fsprojects/FsHttp/blob/0526657d7c456aa2ab70352ab2919222a04c82d5/src/FsHttp/Extensions.fs
        //https://github.com/fsprojects/FsHttp/blob/0526657d7c456aa2ab70352ab2919222a04c82d5/src/FsHttp/Response.fs

        let copyToCallbackAsync (target: Stream) callback (source: Stream) =
                                                              
                async
                    {
                        let buffer = Array.create 1024 (byte 0)
                        let logTimeSpan = TimeSpan.FromSeconds 1.5

                        let mutable continueLooping = true
                        let mutable overallBytesCount = 0
                        let mutable lastNotificationTime = DateTime.Now 
                                                                                                                                
                        counterAndProgressBarPost() 
                                                                                             
                        while continueLooping do 

                            async 
                                {
                                    while true do
                                        match not <| NetworkInterface.GetIsNetworkAvailable() with
                                        | true  -> (processor ()).Post(First(1))                                                                                                                                            
                                        | false -> () 
                                        do! Async.Sleep(2000)
                                } |> Async.StartChild |> ignore
                                                                                                                         
                            let! readCount =                                                                                        
                                source.ReadAsync(buffer, 0, buffer.Length) |> Async.AwaitTask
                                                                                
                            do target.Write(buffer, 0, readCount)
                            do overallBytesCount <- overallBytesCount + readCount
                            let now = DateTime.Now   

                            match (now - lastNotificationTime) > logTimeSpan with
                            | true  -> 
                                     do callback overallBytesCount 
                                     do lastNotificationTime <- now
                            | false -> 
                                     () 

                            do continueLooping <- readCount > 0                                                                                   
                                                                            
                        callback overallBytesCount                                                                    
                    }         

        let copyToCallbackTAsync (target: Stream) callback (source: Stream) =    
            copyToCallbackAsync target callback source |> Async.StartAsTask

        let copyToAsync target source =

            async 
                {
                    Fsi.logfn "Download response received - starting download..."

                    do!
                        source
                        |> copyToCallbackAsync
                            target
                            (fun read ->    
                                       let mbRead = float read / 1024.0 / 1024.0
                                       Fsi.logfn "%f MB" mbRead
                            )

                    Fsi.logfn "Download finished."
                }
                                                    
        let saveFileAsync fileName source =

            async
                {
                    Fsi.logfn "Download response received (file: %s) - starting download..." fileName
                   
                    //************** My code addition start ****************

                    //Tokem for educational purposes
                    (*
                    match not <| getIsNetworkAvailable () with
                    | true  -> cts.Cancel()                                                                                                                                                            
                    | false -> ()  

                    match token.IsCancellationRequested with
                    | true  ->  (processor ()).Post(First(1))                                                                                               
                    | false -> ()    
                    *)
                                      
                   //************** My code addition end  ****************

                    use fs = File.Open(fileName, FileMode.Create, FileAccess.Write)
                    do! source |> copyToAsync fs
                    Fsi.logfn "Download finished."
                }
                                               
        let toStreamTAsync (cancellationToken: CancellationToken) (response: Response) = 
                        
            response.content.ReadAsStreamAsync(cancellationToken)

        let toStreamAsync response =
            toStreamTAsync token response |> Async.AwaitTask
                                                    
        let saveFileAsync (fileName: string) response =
            async 
                { 
                    let fullFileName = Path.GetFullPath fileName
                    let dir = Path.GetDirectoryName fullFileName
                                                
                    match Directory.Exists dir |> not with
                    | true -> Directory.CreateDirectory dir |> ignore
                    | false -> ()
                                                
                    let! stream = response |> toStreamAsync
                    do! stream |> saveFileAsync fileName
                }

        let saveFileTAsync (fileName: string) cancellationToken response =             
            Async.StartAsTask (saveFileAsync fileName response, cancellationToken = cancellationToken)

        response |> saveFileTAsync path token 

                                                         