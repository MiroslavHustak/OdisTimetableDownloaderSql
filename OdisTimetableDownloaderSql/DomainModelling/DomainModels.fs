namespace DomainModelling

open System

module DomainModel = 

    type DbDataDomainGet = 
        {            
            completeLink : string option
            fileToBeSaved : string option
        }

    type DtDataDomainGet = 
        {           
            newPrefix : string  
            startDate : DateTime
            endDate : DateTime 
            completeLink : string 
            fileToBeSaved : string  
        } 

    type DbDataDomainSend = 
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

    type DtDataDomainSend = 
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