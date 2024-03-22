namespace DataModelling

open System

module DataModel = 

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