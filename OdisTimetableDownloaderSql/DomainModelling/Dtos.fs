namespace DomainModelling

open System
open System.Data

module Dto = 

    type DbDataDtoGet = 
        {
            completeLink : obj             
            fileToBeSaved : obj 
        }

    type DtDataDtoGet = 
        {           
            newPrefix : obj 
            startDate : obj
            endDate : obj 
            completeLink : obj 
            fileToBeSaved : obj 
        } 

    type DbDataDtoSend = 
        {
            oldPrefix : string 
            newPrefix : string 
            startDate : DateTime  
            endDate : DateTime   
            totalDateInterval : string 
            suffix : string 
            jsGeneratedString : string 
            completeLink : string 
            fileToBeSaved : string 
        }

    type DtDataDtoSend = 
        {
            oldPrefix : string 
            newPrefix : string 
            startDate : DateTime  
            endDate : DateTime   
            totalDateInterval : string 
            suffix : string 
            jsGeneratedString : string 
            completeLink : string 
            fileToBeSaved : string  
        }

