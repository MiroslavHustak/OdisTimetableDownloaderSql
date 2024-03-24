namespace TransformationLayers

open System

open Types

open Helpers
open Helpers.Builders
open Helpers.TryParserDate

open DataModelling.Dto
open DataModelling.DataModel

//Type-driven design

module TransformationLayerGet =
        
    let internal dbDataTransformLayerGet (dbDtoGet : DbDtoGet) : DbDataGet =
        {      
            completeLink = CompleteLinkOpt (Casting.castAs<string> dbDtoGet.completeLink)
            fileToBeSaved = FileToBeSavedOpt (Casting.castAs<string> dbDtoGet.fileToBeSaved)
        }

    let private dtDataTransformLayerGetDefault : DtDataGet = 
        {      
            newPrefix = NewPrefix String.Empty
            startDate = StartDateDt DateTime.MinValue
            endDate = EndDateDt DateTime.MinValue
            completeLink = CompleteLink String.Empty
            fileToBeSaved = FileToBeSaved String.Empty
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
                       newPrefix = NewPrefix newPrefix
                       startDate = StartDateDt startDate
                       endDate = EndDateDt endDate
                       completeLink = CompleteLink completeLink
                       fileToBeSaved = FileToBeSaved fileToBeSaved
                   } 
           }

module TransformationLayerSend =
        
    let internal dbDataTransformLayerSend (dbDataSend : DbDataSend) : DbDtoSend =
        {
            oldPrefix = dbDataSend.oldPrefix |> function OldPrefix value -> value
            newPrefix = dbDataSend.newPrefix |> function NewPrefix value -> value
            startDate =
                let startdate = dbDataSend.startDate |> function StartDate value -> value
                match parseDate () startdate with Some value -> value | None -> DateTime.MinValue
            endDate = 
                let endDate = dbDataSend.endDate |> function EndDate value -> value
                match parseDate () endDate with Some value -> value | None -> DateTime.MinValue
            totalDateInterval = dbDataSend.totalDateInterval |> function TotalDateInterval value -> value
            suffix = dbDataSend.suffix |> function Suffix value -> value
            jsGeneratedString = dbDataSend.jsGeneratedString |> function JsGeneratedString value -> value
            completeLink = dbDataSend.completeLink |> function CompleteLink value -> value
            fileToBeSaved = dbDataSend.fileToBeSaved |> function FileToBeSaved value -> value
        } 

    let internal dtDataTransformLayerSend (dtDataSend : DtDataSend) : DtDtoSend =
        {
            oldPrefix = dtDataSend.oldPrefix |> function OldPrefix value -> value
            newPrefix = dtDataSend.newPrefix |> function NewPrefix value -> value
            startDate =
                let startdate = dtDataSend.startDate |> function StartDateDtOpt value -> value
                match startdate with Some value -> value | None -> DateTime.MinValue
            endDate = 
                let endDate = dtDataSend.endDate |> function EndDateDtOpt value -> value
                match endDate with Some value -> value | None -> DateTime.MinValue
            totalDateInterval = dtDataSend.totalDateInterval |> function TotalDateInterval value -> value
            suffix = dtDataSend.suffix |> function Suffix value -> value
            jsGeneratedString = dtDataSend.jsGeneratedString |> function JsGeneratedString value -> value
            completeLink = dtDataSend.completeLink |> function CompleteLink value -> value
            fileToBeSaved = dtDataSend.fileToBeSaved |> function FileToBeSaved value -> value
        }