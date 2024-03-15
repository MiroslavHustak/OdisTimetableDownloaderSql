namespace DomainModelling

open System
open System.Data

module Dto = 

    type DbDataDtoGet = 
        {
            completeLink : obj             
            fileToBeSaved : obj 
        }

    type DbDataDtoSend = 
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

