namespace DataTable

open System
open System.Data

open Fugit
open FsToolkit.ErrorHandling

open Types
open Helpers
open DomainModelling.DomainModel
open DomainModelling.DtoGet
open TransformationLayers.TransormationLayerGet

module InsertInto = 
        
    let private dt = //timetableLinksTable () !!!

        let dtTimetableLinks = new DataTable()
        
        let addColumn (name: string) (dataType: Type) =

            let dtColumn = new DataColumn()
            dtColumn.DataType <- dataType
            dtColumn.ColumnName <- name
            dtTimetableLinks.Columns.Add(dtColumn)
        
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
        

    let insertIntoDataTable (dataToBeInserted : DbDataDomainSend list) =
            
        dataToBeInserted 
        |> List.iter 
            (fun item ->
                       let newRow = dt.NewRow()
                       newRow.["OldPrefix"] <- item.oldPrefix
                       newRow.["NewPrefix"] <- item.newPrefix
                       newRow.["StartDate"] <- item.startDate
                       newRow.["EndDate"] <- item.endDate
                       newRow.["TotalDateInterval"] <- item.totalDateInterval
                       newRow.["VT_Suffix"] <- item.suffix
                       newRow.["JS_GeneratedString"] <- item.jsGeneratedString
                       newRow.["CompleteLink"] <- item.completeLink
                       newRow.["FileToBeSaved"] <- item.fileToBeSaved
                       dt.Rows.Add(newRow)
            )
                  

    let internal filter dataToBeInserted validity = 

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
        

        // LINQ query to filter the DataTable
        let currentTime = DateTime.Now.Date
        
        let query =
            dt.AsEnumerable()
            |> Seq.filter
                (fun row ->
                          let startDate = Convert.ToDateTime(row.["StartDate"])
                          let endDate = Convert.ToDateTime(row.["EndDate"])
                          let fileToBeSaved = Convert.ToString(row.["FileToBeSaved"])
                          condition startDate endDate currentTime fileToBeSaved
                )
            |> Seq.sortByDescending (fun row -> Convert.ToDateTime(row.["StartDate"]))
            |> Seq.groupBy (fun row -> row.["NewPrefix"])
            |> Seq.map
                (fun (newPrefix, group)
                    ->
                     newPrefix,
                     group |> Seq.head
                )
            |> Seq.map
                (fun (_ , row) 
                    ->
                     Convert.ToString(row.["CompleteLink"]),
                     Convert.ToString(row.["FileToBeSaved"])
                )
            (*
            |> Seq.map 
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
                                                               failwith "Chyba při čtení z databáze" //zcela vyjimecne //TODO, kdyz bude cas, predelat na result type 
                                                               None
                )
                *)
            //|> Seq.choose id 
            |> List.ofSeq

        query
           