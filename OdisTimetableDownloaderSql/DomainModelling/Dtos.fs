namespace DomainModelling

module DtoGet = 

    type DbDataDtoGet = 
        {
            completeLink : obj             
            fileToBeSaved : obj 
        }

    type DbDataDtoSend = 
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

