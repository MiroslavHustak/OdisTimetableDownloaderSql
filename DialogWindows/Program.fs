﻿module BrowserDialogWindow

open System

let internal openFolderBrowserDialog() = 

    try 
        let folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog() 

        folderBrowserDialog.SelectedPath <- Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        folderBrowserDialog.Description <- "Select a folder"

        let result = folderBrowserDialog.ShowDialog()
        
        match result = System.Windows.Forms.DialogResult.OK with
        | true  -> folderBrowserDialog.SelectedPath, false
        | false -> String.Empty, true         
    with
    | ex -> 
         //(string ex), true
         "Chyba při pokusu o vybrání adresáře.", true
               

    
