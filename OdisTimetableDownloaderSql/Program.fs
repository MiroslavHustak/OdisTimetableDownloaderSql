﻿open System
open System.Data
open System.Windows
open FSharp.Control
open System.Windows.Forms

open Helpers
open Helpers.TryWithRF
open Helpers.ConsoleFixers

open Types
open Types.DirNames

open Logging.Logging

open BrowserDialogWindow

open Settings.Messages
open Settings.SettingsGeneral

open MainFunctions.WebScraping_DPO
open MainFunctions.WebScraping_MDPO
open MainFunctions.WebScraping_KODISFM
open MainFunctions.WebScraping_KODISFMDataTable

[<TailCall>] 
let rec private pathToFolder () =

    let e = String.Empty
    let (str, value) = openFolderBrowserDialog()    

    match value with
    | false            -> 
                        str       
    | true 
        when 
            (<>) str e -> 
                        Console.Clear()
                        tryWith2 (lazy ()) ()           
                        |> function    
                            | Ok value  -> 
                                         value
                            | Error err -> 
                                         logInfoMsg <| sprintf "043 %s" err
                                         closeItBaby messagesDefault str    
                        e
    | _                -> 
                        Console.Clear()
                        printfn "\nNebyl vybrán adresář. Tak znovu... Anebo klikni na křížek pro ukončení aplikace. \n"                                                                     
                        pathToFolder ()   

[<EntryPoint; STAThread>] // STAThread> musi byt quli openFolderBrowserDialog()
//[<EntryPoint>] 
let main argv =
    
    //*****************************Console******************************  
    
    let updateDate = "15-03-2024"

    let consoleSettings f = 

        tryWithLazy messagesDefault.msgParam1 f ()           
            |> function    
                | Ok (value : Lazy<unit>) -> 
                                           value.Force()
                | Error ex                ->   
                                           ex.Force()
                                           logInfoMsg <| sprintf "045 %s" "Problém s oknem konzole."
                                           closeItBaby messagesDefault "Problém s oknem konzole."  
    
    //Ok jen z duvodu vyuziti funkce tryWithLazy (zmenit ji nemohu quli educational code)
    consoleSettings (Ok <| lazy consoleAppProblemFixer()) 
    consoleSettings (Ok <| lazy consoleWindowSettings())  
           
     
    //*****************************WebScraping******************************   

    let myWebscraping_DPO x =

        Console.Clear()
        printfn "Hromadné stahování aktuálních JŘ ODIS (včetně výluk) dopravce DP Ostrava z webu https://www.dpo.cz"
        printfn "Datum poslední aktualizace SW: %s" updateDate
        printfn "********************************************************************"
        printfn "Nyní je třeba vybrat si adresář pro uložení JŘ dopravce DP Ostrava."
        printfn "Pokud ve vybraném adresáři existuje následující podadresář, jeho obsah bude nahrazen nově staženými JŘ."
        printfn "[%s]" <| ODISDefault.odisDir5
        printfn "%c" <| char(32)
        printfn "Přečti si pozorně výše uvedené a stiskni:"
        printfn "Esc pro ukončení aplikace, ENTER pro výběr adresáře, nebo cokoliv jiného pro návrat na hlavní stránku."

        let pressedKey = Console.ReadKey()

        match pressedKey.Key with
        | ConsoleKey.Enter ->                                                                                     
                            Console.Clear()           
                            match pathToFolder >> Option.ofNull <| () with
                            | Some path ->  
                                         printfn "Skvěle! Adresář byl vybrán. Nyní stiskni cokoliv pro stažení aktuálních JŘ dopravce DP Ostrava."
                                         Console.Clear()

                                         webscraping_DPO path 
                                                                           
                                         printfn "%c" <| char(32)   
                                         printfn "Stiskni Esc pro ukončení aplikace nebo cokoliv jiného pro návrat na hlavní stránku."
                                         Console.ReadKey()             
                            | None      -> 
                                         printfn "%c" <| char(32)   
                                         printfn "No jéje. Vybraný adresář neexistuje."
                                         printfn "Stiskni Esc pro ukončení aplikace nebo cokoliv jiného pro návrat na hlavní stránku."
                                         Console.ReadKey()                                
        | _                ->
                            pressedKey     

    let myWebscraping_MDPO x = 

        Console.Clear()
        printfn "Hromadné stahování aktuálních JŘ ODIS dopravce MDP Opava z webu https://www.mdpo.cz"         
        printfn "JŘ jsou pouze zastávkové - klasické JŘ stáhnete v \"celoODISové\" variantě (volba 3 na úvodní stránce)."   
        printfn "Datum poslední aktualizace SW: %s" updateDate
        printfn "********************************************************************"
        printfn "Nyní je třeba vybrat si adresář pro uložení JŘ dopravce MDP Opava."
        printfn "Pokud ve vybraném adresáři existuje následující podadresář, jeho obsah bude nahrazen nově staženými JŘ."
        printfn "[%s]" <| ODISDefault.odisDir6       
        printfn "%c" <| char(32) 
        printfn "Přečti si pozorně výše uvedené a stiskni:"
        printfn "Esc pro ukončení aplikace, ENTER pro výběr adresáře, nebo cokoliv jiného pro návrat na hlavní stránku."

        let pressedKey = Console.ReadKey()

        match pressedKey.Key with
        | ConsoleKey.Enter ->                                                                                     
                            Console.Clear()           
                            match pathToFolder >> Option.ofNull <| () with
                            | Some path ->  
                                         printfn "Skvěle! Adresář byl vybrán. Nyní stiskni cokoliv pro stažení aktuálních JŘ dopravce MDP Opava."
                                         Console.Clear()

                                         webscraping_MDPO path 
                                                                           
                                         printfn "%c" <| char(32)   
                                         printfn "Stiskni Esc pro ukončení aplikace nebo cokoliv jiného pro návrat na hlavní stránku."
                                         Console.ReadKey()             
                            | None      -> 
                                         printfn "%c" <| char(32)   
                                         printfn "No jéje. Vybraný adresář neexistuje."
                                         printfn "Stiskni Esc pro ukončení aplikace nebo cokoliv jiného pro návrat na hlavní stránku."
                                         Console.ReadKey()                                
        | _                ->
                            pressedKey     
    
    let myWebscraping_KODIS () = 

        Console.Clear()
        printfn "Hromadné stahování JŘ ODIS všech dopravců v systému ODIS z webu https://www.kodis.cz"           
        printfn "Datum poslední aktualizace SW: %s" updateDate
        printfn "********************************************************************"
        printfn "Nyní je třeba vybrat si adresář pro uložení JŘ všech dopravců v systému ODIS."
        printfn "Pokud ve vybraném adresáři existují následující podadresáře, jejich obsah bude nahrazen nově staženými JŘ."
        printfn "%4c[%s]" <| char(32) <| ODISDefault.odisDir1
        printfn "%4c[%s]" <| char(32) <| ODISDefault.odisDir2
        printfn "%4c[%s]" <| char(32) <| ODISDefault.odisDir3
        printfn "%4c[%s]" <| char(32) <| ODISDefault.odisDir4  
        printfn "%c" <| char(32) 
        printfn "Přečti si pozorně výše uvedené a stiskni:"
        printfn "Esc pro ukončení aplikace, ENTER pro výběr adresáře, nebo cokoliv jiného pro návrat na hlavní stránku."

        let pressedKey = Console.ReadKey()

        match pressedKey.Key with
        | ConsoleKey.Enter ->                                                                                     
                            Console.Clear()           
                            match pathToFolder >> Option.ofNull <| () with
                            | Some path ->  
                                         Console.Clear()          
                                         printfn "Skvěle! Adresář byl vybrán. Nyní prosím vyber variantu (číslice plus ENTER, příp. jen ENTER pro kompletně všechno)."
                                         printfn "%c" <| char(32)
                                         printfn "1 = Aktuální JŘ, které striktně platí dnešní den, tj. pokud je např. pro dnešní den"
                                         printfn "%4cplatný pouze určitý jednodenní výlukový JŘ, stáhne se tento JŘ, ne JŘ platný od dalšího dne." <| char(32)
                                         printfn "2 = JŘ (včetně výlukových JŘ), platné až v budoucí době, které se však už nyní vyskytují na webu KODISu."
                                         printfn "3 = Pouze aktuální výlukové JŘ, JŘ NAD a JŘ X linek (krátkodobé i dlouhodobé)."
                                         printfn "4 = JŘ teoreticky dlouhodobě platné bez jakýchkoliv (i dlouhodobých) výluk či NAD."
                                         printfn "%c" <| char(32) 
                                         printfn "Jakákoliv jiná klávesa plus ENTER = KOMPLETNÍ stažení všech variant JŘ.\r"        
                                         printfn "%c" <| char(32) 
                                         printfn "%c" <| char(32) 
                                         printfn "Stačí stisknout pouze ENTER pro KOMPLETNÍ stažení všech variant JŘ. A buď trpělivý, chvíli to potrvá."
                                            
                                         let variant = 
                                             Console.ReadLine()
                                             |> function 
                                                 | "1" -> [ CurrentValidity ]
                                                 | "2" -> [ FutureValidity ]  
                                                 | "3" -> [ ReplacementService ]
                                                 | "4" -> [ WithoutReplacementService ]
                                                 | _   -> [ CurrentValidity; FutureValidity; ReplacementService; WithoutReplacementService ]
                                            
                                         Console.Clear()
                                            
                                         //webscraping_KODISFMDataTable path variant 
                                         webscraping_KODISFM path variant 
                                            
                                         printfn "%c" <| char(32)         
                                         printfn "JŘ s chybějícími údaji o platnosti (např. NAD bez dalších údajů), pokud existovaly, nebyly staženy."
                                         printfn "JŘ s chybnými údaji o platnosti, pokud existovaly, pravděpodobně nebyly staženy (záleží na druhu chyby)."
                                         printfn "%c" <| char(32)   
                                         printfn "Stiskni Esc pro ukončení aplikace nebo cokoliv jiného pro návrat na hlavní stránku."
                                         Console.ReadKey() 
                            | None      -> 
                                         printfn "%c" <| char(32)   
                                         printfn "No jéje. Vybraný adresář neexistuje."
                                         printfn "Stiskni Esc pro ukončení aplikace nebo cokoliv jiného pro návrat na hlavní stránku."
                                         Console.ReadKey()                                
        | _                ->
                            pressedKey              
        
    //[<TailCall>] //kontrolovano jako module function, bez varovnych hlasek
    let rec variant () = 

        let timetableVariant (fn: ConsoleKeyInfo) = 
            let result = 
                match fn.Key with
                | ConsoleKey.Escape -> System.Environment.Exit(0)
                | _                 -> variant ()
            
            tryWith2 (lazy result) ()           
            |> function    
                | Ok value  -> 
                             value
                | Error err -> 
                             messagesDefault.msgParam1 err  
                             logInfoMsg <| sprintf "046 %s" err
                             closeItBaby messagesDefault err

        Console.Clear()

        printfn "Zdravím nadšence do klasických jízdních řádů. Nyní prosím zadejte číslici plus ENTER pro výběr varianty." 
        printfn "%c" <| char(32)  
        printfn "1 = Hromadné stahování jízdních řádů ODIS pouze dopravce DP Ostrava z webu https://www.dpo.cz"
        printfn "2 = Hromadné stahování pouze zastávkových jízdních řádů ODIS dopravce MDP Opava z webu https://www.mdpo.cz"
        printfn "3 = Hromadné stahování jízdních řádů ODIS všech dopravců v systému ODIS z webu https://www.kodis.cz"
        printfn "%c" <| char(32)  
        printfn "Anebo klikni na křížek pro ukončení aplikace."
                
        let s1 = "Není připojení k internetu. Obnov jej, bez něj stahování JŘ opravdu nebude fungovat :-)."
        let s2 = String.Empty
        let str = sprintf "%s %s" s1 s2

        let boxTitle = "No jéje, zase problém ..."
                                                         
        let checkNetConnection timeout =
            [1..10] 
            |> List.filter (fun _ -> CheckNetConnection.checkNetConn(timeout).IsSome) 
            |> List.length >= 8  
        
        let checkNetConnF = checkNetConnection 500
       
        //[<TailCall>] //kontrolovano jako module function, bez varovnych hlasek
        let rec checkConnect checkNetConnP = 

            match checkNetConnP with
            | true  -> 
                     ()
            | false -> 
                     MessageBox.Show
                         (
                             str, 
                             boxTitle, 
                             MessageBoxButtons.OK
                         )  
                         |> ignore
                         
                     checkConnect << checkNetConnection <| 500
        
        checkConnect checkNetConnF
           
        Console.ReadLine()
        |> function 
            | "1" ->
                   myWebscraping_DPO >> timetableVariant <| ()                     
            | "2" ->
                   myWebscraping_MDPO >> timetableVariant <| ()                      
            | "3" ->
                   myWebscraping_KODIS >> timetableVariant <| ()                  
            | _   ->
                   printfn "Varianta nebyla vybrána. Prosím zadej znovu."
                   variant()
                   
    variant()   
    0 // return an integer exit code


