namespace Settings

open System
open Types.Messages
open Types.MessagesMocking

module Messages =

    [<Literal>] 
    let formatBody = "\n%s%s"
        
    let internal messagesDefault : Messages = 
        {
            msg1 = fun () -> printfn "Zase se někdo vrtal v listu s odkazy a cestami. Je nutná jejich kontrola. Zmáčkni cokoliv pro ukončení programu." 
            msg2 = fun () -> printfn "Probíhá stahování a ukládání JSON souborů ze stránek KODISu."
            msg3 = fun () -> printfn "Dokončeno stahování a ukládání JSON souborů ze stránek KODISu."
            msg4 = fun () -> printfn "Probíhá filtrace odkazů na neplatné jízdní řády."
            msg5 = fun () -> printfn "Chyba v průběhu stahování JSON souborů."  
            msg6 = fun () -> printfn "Chyba v průběhu stahování JSON souborů." 
            msg7 = fun () -> printfn "Chyba v průběhu stahování JSON souborů."  
            msg8 = fun () -> printfn "Chyba v průběhu stahování JSON souborů."  
            msg9 = fun () -> printfn "Chyba při filtraci odkazů na JŘ." 
            msg10 = fun () -> printfn "Dokončena filtrace odkazů na neplatné jízdní řády."
            msg11 = fun () -> printfn "Provedeno odstranění všech starých JŘ ve zvolené variantě, pokud existovaly. \nChvíli strpení, prosím ..."
            msg12 = fun () -> printfn "Provedeno odstranění starých JŘ ve zvolené variantě, pokud existovaly. \nChvíli strpení, prosím ..."
            msg13 = fun () -> printfn "Pravděpodobně někdo odstranil daný adresář v průběhu práce tohoto programu."  
            msg14 = fun () -> printfn "Nadřel jsem se, ale úkol jsem úspěšně dokončil :-)"
            msg15 = fun () -> printfn "Chvíli strpení, prosím..." 
            msg16 = "Chyba v některé fázi procesu programu pro stahování JŘ. Ověř, zdali nepoužíváš adresáře, které se odstraňují či do kterých se zapisuje, například zdali nemáš otevřený některý JŘ v dotčených adresářích. "   

            msgParam1 = printfn formatBody "No jéje, někde nastala chyba. Zmáčkni cokoliv pro ukončení programu a zkus to znovu. Popis chyby: " 
            msgParam2 = printfn formatBody "Jízdní řád s tímto odkazem se nepodařilo stáhnout: \n"  
            msgParam3 = printfn "Probíhá stahování příslušných JŘ a jejich ukládání do [%s]."  
            msgParam4 = printfn "Dokončeno stahování příslušných JŘ a jejich ukládání do [%s]. \nChvíli strpení, prosím..."  
            msgParam5 = printfn "Adresář [%s] neexistuje, příslušné JŘ do něj určené nemohly být staženy." 
            msgParam6 = printfn "Chyba v řetězci [%s]." 
            msgParam7 = printfn"%s" 
            msgParam8 = printfn "%s\n" 
            msgParam9 = printf "%s\r" 
            msgParam10 = printfn "Parsování neproběhlo korektně u tohoto řetězce: %s"  
            msgParam11 = printfn "Soubor %s nenalezen" 
            msgParam12 = printfn "Adresář %s nenalezen"
        }
   
module MessagesMocking =       

    let internal messagesDefaultMocking : MessagesMocking = 
        {
            msg1 = fun () -> () 
            msg2 = fun () -> ()
            msg3 = fun () -> ()
            msg4 = fun () -> ()
            msg5 = fun () -> ()
            msg6 = fun () -> ()
            msg7 = fun () -> ()
            msg8 = fun () -> ()
            msg9 = fun () -> ()
            msg10 = fun () -> ()
            msg11 = fun () -> ()
            msg12 = fun () -> ()
            msg13 = fun () -> ()  
            msg14 = fun () -> ()
            msg15 = fun () -> ()
            msg16 = String.Empty

            msgParam1 = fun (input: string) -> ()                     
            msgParam2 = fun (input: string) -> ()    
            msgParam3 = fun (input: string) -> ()    
            msgParam4 = fun (input: string) -> ()     
            msgParam5 = fun (input: string) -> ()   
            msgParam6 = fun (input: string) -> ()   
            msgParam7 = fun (input: string) -> ()   
            msgParam8 = fun (input: string) -> ()    
            msgParam9 = fun (input: string) -> ()   
            msgParam10 = fun (input: string) -> ()    
            msgParam11 = fun (input: string) -> ()  
            msgParam12 = fun (input: string) -> () 
        }
   