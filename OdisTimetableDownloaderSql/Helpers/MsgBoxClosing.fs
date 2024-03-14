namespace Helpers

module MsgBoxClosing =

    open System
    open System.Windows.Forms
    open System.Net.NetworkInformation
    open System.Runtime.InteropServices

    open FSharp.Control

    open Types

    [<DllImport("user32.dll", CharSet = CharSet.Auto)>]
    extern int private SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam)

    [<DllImport("user32.dll", SetLastError = true)>]
    extern IntPtr private FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle)

    [<DllImport("user32.dll", SetLastError = true)>]
    extern IntPtr private FindWindow(string lpClassName, string lpWindowName)    

    let [<Literal>] private WM_CLOSE = 16u
    let [<Literal>] private WM_LBUTTONDOWN = 0x0201u
    let [<Literal>] private WM_LBUTTONUP = 0x0202u
    
    let private sendMessageMethod (hwnd: IntPtr) =
            
        let hwndChild = //handle
            match hwnd <> IntPtr.Zero with
            | true  -> FindWindowEx(hwnd, IntPtr.Zero, "Button", "OK")
            | false -> IntPtr.Zero
           
        SendMessage(hwndChild, WM_LBUTTONDOWN, 0, IntPtr.Zero) |> ignore
        SendMessage(hwndChild, WM_LBUTTONUP, 0, IntPtr.Zero) |> ignore
        SendMessage(hwndChild, WM_LBUTTONDOWN, 0, IntPtr.Zero) |> ignore
        SendMessage(hwndChild, WM_LBUTTONUP, 0, IntPtr.Zero) |> ignore
    
    let private sendMessageMethodX (hwndX: IntPtr) =

         match hwndX <> IntPtr.Zero with
         | true  -> SendMessage(hwndX, WM_CLOSE, 0, IntPtr.Zero) |> ignore //WM_CLOSE to je ten krizek
         | false -> ()

    let private clickOnOKButton boxTitle =    
        
        let hwnd = FindWindow("#32770", boxTitle) //kliknuti na OK
        sendMessageMethod hwnd      
        
        let hwndX = FindWindow("#32770", boxTitle) //kliknuti na krizek (pro jistotu)
        sendMessageMethodX hwndX

    let private findMsgBox boxTitle =    
        
        let hwnd = FindWindow("#32770", boxTitle)
        hwnd <> IntPtr.Zero 

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

    let internal processor (waitingTime : int) variant = 
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
                                    let boxTitle = "No jéje, zase problém ..."

                                    match findMsgBox boxTitle with
                                    | true  -> 
                                                () //clickOnOKButton boxTitle
                                    | false ->                                                          
                                                match variant with
                                                | Json -> 
                                                        let s1 = "Není připojení k internetu. Obnov jej, stahování příslušných souborů bude pak pokračovat."
                                                        let s2 = String.Empty
                                                        let str = sprintf "%s %s" s1 s2
                                                              
                                                        let result () =
                                                            MessageBox.Show
                                                                (
                                                                    str, 
                                                                    boxTitle, 
                                                                    MessageBoxButtons.OK
                                                                ) 

                                                        AsyncSeq.initInfinite (fun _ -> result () <> DialogResult.OK)
                                                        |> AsyncSeq.takeWhile ((<>) true) 
                                                        |> AsyncSeq.iterAsync (fun _ -> async { do! Async.Sleep(waitingTime) }) 
                                                        |> Async.StartImmediate 
            
                                                | Pdf  ->
                                                        let s1 = "Není připojení k internetu. Stahování souborů se musí ukončit, i kdyby zrovna došlo k obnovení připojení."
                                                        let s2 = "Zmáčknutím tlačítka Ok vypni tento program. Je pak nutné jej spustit znovu."
                                                        let str = sprintf "%s %s" s1 s2

                                                        let result () = 
                                                            MessageBox.Show
                                                                (
                                                                    str, 
                                                                    boxTitle, 
                                                                    MessageBoxButtons.OK
                                                                )  

                                                        match findMsgBox boxTitle with
                                                        | true  -> 
                                                                () //clickOnOKButton boxTitle
                                                        | false ->                                                                
                                                                match result () = DialogResult.OK with
                                                                | true  -> System.Environment.Exit(1)                                                                               
                                                                | false -> ()                                                      
                                                     
                                        //while result <> DialogResult.OK do
                                        // do! Async.Sleep(120000)

                                    return! loop false // Set isFirst to false to ignore subsequent messages
                               | _         ->                      
                                            return! loop isFirst
                           }

                   loop true // Start with isFirst set to true
           ) 

    let internal closeItMyBaby () = 

        async 
            {                  
                match NetworkInterface.GetIsNetworkAvailable() with
                | true  -> ()
                | false -> (processor 120000 Pdf).Post(First(1)) 

            } |> Async.StartImmediate
    
              
    
      

