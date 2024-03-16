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

    let internal dtDataTransformLayerGetDefault (dtDataDtoGet : DtDataDtoGet) : DtDataDomainGet = 
        {      
            newPrefix = String.Empty
            startDate = DateTime.MinValue
            endDate = DateTime.MinValue
            completeLink = String.Empty
            fileToBeSaved = String.Empty
        } 

    let internal dtDataTransformLayerGet (dtDataDtoGet : DtDataDtoGet) : Result<DtDataDomainGet, string> =  
        
        let newPrefix = Convert.ToString(dtDataDtoGet.newPrefix) //u datatable nelze Casting.castAs<string>, musi se pouzit Convert
        let startDate = Convert.ToDateTime(dtDataDtoGet.startDate)
        let endDate = Convert.ToDateTime(dtDataDtoGet.endDate)
        let completeLink = Convert.ToString(dtDataDtoGet.completeLink)
        let fileToBeSaved = Convert.ToString(dtDataDtoGet.fileToBeSaved)

        let testString = 
            [
                newPrefix |> Option.ofNull  
                completeLink |> Option.ofNull
                fileToBeSaved |> Option.ofNull
            ] 

        let testDateTime = 
            [
                startDate |> Option.ofNull
                endDate |> Option.ofNull
            ] 

        pyramidOfHell
           {
               let! tString = not (testString |> List.contains None), Error "Chyba při načítání hodnot z datatable" 
               let! tDateTime = not (testDateTime |> List.contains None), Error "Chyba při načítání hodnot z datatable" 

               let result = 
                   {      
                       newPrefix = newPrefix
                       startDate = startDate
                       endDate = endDate
                       completeLink = completeLink
                       fileToBeSaved = fileToBeSaved
                   } 

               return Ok result
           }

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