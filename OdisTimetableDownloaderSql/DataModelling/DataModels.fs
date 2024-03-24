namespace DataModelling

open System

open Types

//Type-driven design

module DataModel = 

    type DbDataGet = 
        {            
            completeLink : CompleteLinkOpt
            fileToBeSaved : FileToBeSavedOpt
        }

    type DtDataGet = 
        {           
            newPrefix : NewPrefix  
            startDate : StartDateDt
            endDate : EndDateDt 
            completeLink : CompleteLink 
            fileToBeSaved : FileToBeSaved  
        } 

    type DbDataSend = 
        {
            oldPrefix : OldPrefix 
            newPrefix : NewPrefix 
            startDate : StartDate 
            endDate : EndDate 
            totalDateInterval : TotalDateInterval 
            suffix : Suffix 
            jsGeneratedString : JsGeneratedString 
            completeLink : CompleteLink 
            fileToBeSaved : FileToBeSaved
        }

    type DtDataSend = 
        {
            oldPrefix : OldPrefix 
            newPrefix : NewPrefix 
            startDate : StartDateDtOpt 
            endDate : EndDateDtOpt 
            totalDateInterval : TotalDateInterval 
            suffix : Suffix 
            jsGeneratedString : JsGeneratedString 
            completeLink : CompleteLink 
            fileToBeSaved : FileToBeSaved 
        }

    (*
    type DbDataGet = 
        {            
            completeLink : string option
            fileToBeSaved : string option
        }

    type DtDataGet = 
        {           
            newPrefix : string  
            startDate : DateTime
            endDate : DateTime 
            completeLink : string 
            fileToBeSaved : string  
        } 

    type DbDataSend = 
        {
            oldPrefix : string 
            newPrefix : string 
            startDate : string 
            endDate : string 
            totalDateInterval : string 
            suffix : string 
            jsGeneratedString : string 
            completeLink : string 
            fileToBeSaved : string 
        }

    type DtDataSend = 
        {
            oldPrefix : string 
            newPrefix : string 
            startDate : DateTime option 
            endDate : DateTime option  
            totalDateInterval : string 
            suffix : string 
            jsGeneratedString : string 
            completeLink : string 
            fileToBeSaved : string 
        }  
    
    
    
    *)