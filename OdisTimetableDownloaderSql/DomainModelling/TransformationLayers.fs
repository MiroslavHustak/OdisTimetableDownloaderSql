namespace TransformationLayers

open System
open System.Data

open Helpers
open DomainModelling.Dto
open DomainModelling.DomainModel

open Helpers.TryParserDate

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
            startDate = parseDate dbDataDomain.startDate
            endDate = parseDate dbDataDomain.endDate
            totalDateInterval = dbDataDomain.totalDateInterval
            suffix = dbDataDomain.suffix
            jsGeneratedString = dbDataDomain.jsGeneratedString
            completeLink = dbDataDomain.completeLink
            fileToBeSaved = dbDataDomain.fileToBeSaved
        }