namespace TransformationLayers

open System
open System.Data

open Helpers
open Helpers.Builders
open Helpers.TryParserDate

open DomainModelling.Dto
open DomainModelling.DomainModel

module TransformationLayerGet =
        
    let internal dbDataTransformLayerGet (dbDataDtoGet : DbDataDtoGet) : DbDataDomainGet =
        {      
            completeLink = Casting.castAs<string> dbDataDtoGet.completeLink
            fileToBeSaved = Casting.castAs<string> dbDataDtoGet.fileToBeSaved
        }

    let internal dtDataTransformLayerGet (dtDataDtoGet : DtDataDtoGet) : DtDataDomainGet =
        
        try
            {      
                newPrefix = Convert.ToString(dtDataDtoGet.newPrefix) //u datatable bohuzel nelze Casting.castAs<string>, musi se pouzit Convert
                startDate = Convert.ToDateTime(dtDataDtoGet.startDate)
                endDate = Convert.ToDateTime(dtDataDtoGet.endDate)
                completeLink = Convert.ToString(dtDataDtoGet.completeLink)
                fileToBeSaved = Convert.ToString(dtDataDtoGet.fileToBeSaved)
            } 
        with
        | _ -> failwith "Chyba při čtení z datatable" //zcela vyjimecne //TODO predelat na result type az se bude zmobilnovat 

module TransformationLayerSend =
        
    let internal dbDataTransformLayerSend (dbDataDomain : DbDataDomainSend) : DbDataDtoSend =
        {
            oldPrefix = dbDataDomain.oldPrefix
            newPrefix = dbDataDomain.newPrefix
            startDate = match parseDate dbDataDomain.startDate with Some value -> value | None -> DateTime.MinValue
            endDate = match parseDate dbDataDomain.endDate with Some value -> value | None -> DateTime.MinValue
            totalDateInterval = dbDataDomain.totalDateInterval
            suffix = dbDataDomain.suffix
            jsGeneratedString = dbDataDomain.jsGeneratedString
            completeLink = dbDataDomain.completeLink
            fileToBeSaved = dbDataDomain.fileToBeSaved
        } 

    let internal dtDataTransformLayerSend (dtDataDomain : DtDataDomainSend) : DtDataDtoSend =
        {
            oldPrefix = dtDataDomain.oldPrefix
            newPrefix = dtDataDomain.newPrefix
            startDate = match dtDataDomain.startDate with Some value -> value | None -> DateTime.MinValue
            endDate = match dtDataDomain.endDate with Some value -> value | None -> DateTime.MinValue
            totalDateInterval = dtDataDomain.totalDateInterval
            suffix = dtDataDomain.suffix
            jsGeneratedString = dtDataDomain.jsGeneratedString
            completeLink = dtDataDomain.completeLink
            fileToBeSaved = dtDataDomain.fileToBeSaved
        }