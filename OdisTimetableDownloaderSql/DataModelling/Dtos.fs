namespace DataModelling

open System
open System.Data

module Dto = 

    type DbDtoGet = 
        {
            completeLink : obj             
            fileToBeSaved : obj 
        }

    type DtDtoGet = 
        {           
            newPrefix : obj 
            startDate : obj
            endDate : obj 
            completeLink : obj 
            fileToBeSaved : obj 
        } 

    type DbDtoSend = 
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

    type DtDtoSend = 
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