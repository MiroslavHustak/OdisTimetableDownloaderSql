namespace DomainModelling

module DomainModel = 

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

    type DbDataDomainGet = 
        {            
            completeLink : string option
            fileToBeSaved : string option
        }

