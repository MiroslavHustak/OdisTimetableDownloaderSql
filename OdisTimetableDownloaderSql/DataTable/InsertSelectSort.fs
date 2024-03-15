namespace DataTable

open System
open System.Data

open FsToolkit.ErrorHandling

open Types
open Helpers
open DomainModelling.DomainModel
open DomainModelling.Dto
open TransformationLayers.TransormationLayerGet
open TransformationLayers.TransormationLayerSend
open Helpers.Builders

module InsertSelectSort = 
        
    let private dt = 

        let dtTimetableLinks = new DataTable()
        
        let addColumn (name: string) (dataType: Type) =

            let dtColumn = new DataColumn()
            dtColumn.DataType <- dataType
            dtColumn.ColumnName <- name
            dtTimetableLinks.Columns.Add(dtColumn)
        
        //musi byt jen .NET type, aby nebyly problemy 
        addColumn "OldPrefix" typeof<string>
        addColumn "NewPrefix" typeof<string>
        addColumn "StartDate" typeof<DateTime>
        addColumn "EndDate" typeof<DateTime>
        addColumn "TotalDateInterval" typeof<string>
        addColumn "VT_Suffix" typeof<string>
        addColumn "JS_GeneratedString" typeof<string>
        addColumn "CompleteLink" typeof<string>
        addColumn "FileToBeSaved" typeof<string>
        
        dtTimetableLinks

    let private insertIntoDataTable dataToBeInserted =
            
        dataToBeInserted 
        |> List.iter 
            (fun item ->
                       let (startDate, endDate) =   

                           pyramidOfDoom
                               {
                                   let! startDate = item.startDate, (DateTime.MinValue, DateTime.MinValue)                                                      
                                   let! endDate = item.endDate, (DateTime.MinValue, DateTime.MinValue)                             
                              
                                   return (startDate, endDate)
                               }
                            
                       let newRow = dt.NewRow()
                       newRow.["OldPrefix"] <- item.oldPrefix
                       newRow.["NewPrefix"] <- item.newPrefix
                       newRow.["StartDate"] <- startDate
                       newRow.["EndDate"] <- endDate
                       newRow.["TotalDateInterval"] <- item.totalDateInterval
                       newRow.["VT_Suffix"] <- item.suffix
                       newRow.["JS_GeneratedString"] <- item.jsGeneratedString
                       newRow.["CompleteLink"] <- item.completeLink
                       newRow.["FileToBeSaved"] <- item.fileToBeSaved
                       dt.Rows.Add(newRow)
            )                  

    let internal sortLinksOut dataToBeInserted validity = 

        insertIntoDataTable dataToBeInserted  

        let condition (dateValidityStart : DateTime) (dateValidityEnd : DateTime) (currentTime: DateTime) (fileToBeSaved : string)  = 
            match validity with 
            | CurrentValidity           -> 
                                         ((dateValidityStart <= currentTime
                                         && 
                                         dateValidityEnd >= currentTime)
                                         ||
                                         (dateValidityStart = currentTime 
                                         && 
                                         dateValidityEnd = currentTime))

            | FutureValidity            ->
                                         dateValidityStart > currentTime

            | ReplacementService        -> 
                                         ((dateValidityStart <= currentTime 
                                         && 
                                         dateValidityEnd >= currentTime)
                                         ||
                                         (dateValidityStart = currentTime 
                                         && 
                                         dateValidityEnd = currentTime))
                                         &&
                                         (fileToBeSaved.Contains("_v") 
                                         || fileToBeSaved.Contains("X")
                                         || fileToBeSaved.Contains("NAD"))

            | WithoutReplacementService -> 
                                         ((dateValidityStart <= currentTime 
                                         && 
                                         dateValidityEnd >= currentTime)
                                         ||
                                         (dateValidityStart = currentTime 
                                         && 
                                         dateValidityEnd = currentTime))
                                         &&
                                         (not <| fileToBeSaved.Contains("_v") 
                                         && not <| fileToBeSaved.Contains("X")
                                         && not <| fileToBeSaved.Contains("NAD"))        

        let currentTime = DateTime.Now.Date

        dt.AsEnumerable()
        |> Seq.filter
            (fun row ->
                      let startDate = Convert.ToDateTime(row.["StartDate"])
                      let endDate = Convert.ToDateTime(row.["EndDate"])
                      let fileToBeSaved = Convert.ToString(row.["FileToBeSaved"])
                      condition startDate endDate currentTime fileToBeSaved
            )
        |> Seq.sortByDescending (fun row -> Convert.ToDateTime(row.["StartDate"]))
        |> Seq.groupBy (fun row -> Convert.ToString(row.["NewPrefix"]))
        |> Seq.map
            (fun (newPrefix, group)
                ->
                 newPrefix,
                 group |> Seq.head
            )
        |> Seq.map
            (fun (_ , row) 
                ->
                 //Convert.ToString(row.["CompleteLink"]),
                 //Convert.ToString(row.["FileToBeSaved"])
                 row.["CompleteLink"],
                 row.["FileToBeSaved"]
            )
        |> List.ofSeq
        |> List.map 
            (fun (link, file) ->                                       
                               let record : DbDataDtoGet = 
                                   {
                                       completeLink = link
                                       fileToBeSaved = file
                                   }

                               let result = dbDataTransferLayerGet record

                               (result.completeLink, result.fileToBeSaved)
                               |> function
                                   | Some link, Some file -> 
                                                           Some (link, file)
                                   | _                    ->
                                                           failwith "Chyba při čtení z databáze" //zcela vyjimecne //TODO predelat na result type az se bude zmobilnovat 
                                                           None
            )
        |> List.choose id

        
           