namespace TransformationLayers

open Helpers
open DomainModelling.DtoGet
open DomainModelling.DomainModel

module TransormationLayerGet =
        
    let internal dbDataTransferLayerGet (dbDataDtoGet: DbDataDtoGet) : DbDataDomainGet  =
        {      
            completeLink = Casting.castAs<string> dbDataDtoGet.completeLink
            fileToBeSaved = Casting.castAs<string> dbDataDtoGet.fileToBeSaved
        }

module TransormationLayerSend =
        
    let internal dbDataTransferLayerSend (dbDataDomain: DbDataDomainSend) : DbDataDtoSend =
        {
            oldPrefix = dbDataDomain.oldPrefix
            newPrefix = dbDataDomain.newPrefix
            startDate = dbDataDomain.startDate
            endDate = dbDataDomain.endDate
            totalDateInterval = dbDataDomain.totalDateInterval
            suffix = dbDataDomain.suffix
            jsGeneratedString = dbDataDomain.jsGeneratedString
            completeLink = dbDataDomain.completeLink
            fileToBeSaved = dbDataDomain.fileToBeSaved
        }