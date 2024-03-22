namespace TransformationLayers

open System

open Helpers
open Helpers.Builders
open Helpers.TryParserDate

open DataModelling.Dto
open DataModelling.DataModel

module TransformationLayerGet =
        
    let internal dbDataTransformLayerGet (dbDtoGet : DbDtoGet) : DbDataGet =
        {      
            completeLink = Casting.castAs<string> dbDtoGet.completeLink
            fileToBeSaved = Casting.castAs<string> dbDtoGet.fileToBeSaved
        }

    let private dtDataTransformLayerGetDefault : DtDataGet = 
        {      
            newPrefix = String.Empty
            startDate = DateTime.MinValue
            endDate = DateTime.MinValue
            completeLink = String.Empty
            fileToBeSaved = String.Empty
        } 

    let internal dtDataTransformLayerGet (dtDtoGet : DtDtoGet) : DtDataGet =  
        
        let newPrefix = Convert.ToString(dtDtoGet.newPrefix) //u datatable nelze Casting.castAs<string>, musi se pouzit Convert
        let startDate = Convert.ToDateTime(dtDtoGet.startDate)
        let endDate = Convert.ToDateTime(dtDtoGet.endDate)
        let completeLink = Convert.ToString(dtDtoGet.completeLink)
        let fileToBeSaved = Convert.ToString(dtDtoGet.fileToBeSaved)

        let testString = 
            [
                newPrefix |> Option.ofNull  
                completeLink |> Option.ofNull
                fileToBeSaved |> Option.ofNull
            ] 
        
        //ja vim, ze DataTime je non-nullable, ale radeji to tady tak nechavam, kdyby se neco menilo a preslo se na string a parseDate 
        let testDateTime = 
            [
                startDate |> Option.ofNull 
                endDate |> Option.ofNull
            ] 

        pyramidOfHell
           {
               let! tString = not (testString |> List.contains None), dtDataTransformLayerGetDefault 
               let! tDateTime = not (testDateTime |> List.contains None), dtDataTransformLayerGetDefault 

               return 
                   {      
                       newPrefix = newPrefix
                       startDate = startDate
                       endDate = endDate
                       completeLink = completeLink
                       fileToBeSaved = fileToBeSaved
                   } 
           }

module TransformationLayerSend =
        
    let internal dbDataTransformLayerSend (dbDataSend : DbDataSend) : DbDtoSend =
        {
            oldPrefix = dbDataSend.oldPrefix
            newPrefix = dbDataSend.newPrefix
            startDate = match parseDate () dbDataSend.startDate with Some value -> value | None -> DateTime.MinValue
            endDate = match parseDate () dbDataSend.endDate with Some value -> value | None -> DateTime.MinValue
            totalDateInterval = dbDataSend.totalDateInterval
            suffix = dbDataSend.suffix
            jsGeneratedString = dbDataSend.jsGeneratedString
            completeLink = dbDataSend.completeLink
            fileToBeSaved = dbDataSend.fileToBeSaved
        } 

    let internal dtDataTransformLayerSend (dtDataSend : DtDataSend) : DtDtoSend =
        {
            oldPrefix = dtDataSend.oldPrefix
            newPrefix = dtDataSend.newPrefix
            startDate = match dtDataSend.startDate with Some value -> value | None -> DateTime.MinValue
            endDate = match dtDataSend.endDate with Some value -> value | None -> DateTime.MinValue
            totalDateInterval = dtDataSend.totalDateInterval
            suffix = dtDataSend.suffix
            jsGeneratedString = dtDataSend.jsGeneratedString
            completeLink = dtDataSend.completeLink
            fileToBeSaved = dtDataSend.fileToBeSaved
        }