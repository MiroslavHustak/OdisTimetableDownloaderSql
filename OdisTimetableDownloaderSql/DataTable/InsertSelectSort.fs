namespace DataTable

open System
open System.Data

open FsToolkit.ErrorHandling

open Types

open Helpers.Builders
open Helpers.TryWithRF

open Settings

open DomainModelling.Dto
open DomainModelling.DomainModel

open TransformationLayers.TransformationLayerGet

module InsertSelectSort = 
        
    let private dt = 

        let dtTimetableLinks = new DataTable()
        
        let addColumn (name : string) (dataType : Type) =

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

    let private insertIntoDataTable (dataToBeInserted : DtDataDtoSend list) =
            
        dataToBeInserted 
        |> List.iter 
            (fun item ->
                       (*
                       let (startDate, endDate) =   

                           pyramidOfDoom
                               {
                                   let! startDate = item.startDate, (DateTime.MinValue, DateTime.MinValue)                                                      
                                   let! endDate = item.endDate, (DateTime.MinValue, DateTime.MinValue)                             
                              
                                   return (startDate, endDate)
                               }
                       *)
                            
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

    let internal sortLinksOut (dataToBeInserted : DtDataDtoSend list) validity = 
                
        insertIntoDataTable dataToBeInserted  

        let condition dateValidityStart dateValidityEnd currentTime (fileToBeSaved : string) = 

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

        let dtDataDtoGetDataTable (row : DataRow) : DtDataDtoGet =                         
            {           
                newPrefix = row.["NewPrefix"]
                startDate = row.["StartDate"]
                endDate = row.["EndDate"]
                completeLink = row.["CompleteLink"]
                fileToBeSaved = row.["FileToBeSaved"]
            } 

        let dataTransformation row = 
            
            tryWith2 (lazy ()) (dtDataDtoGetDataTable >> dtDataTransformLayerGet <| row )           
                |> function    
                    | Ok value  ->
                                 value
                    | Error err -> 
                                 closeItBaby Messages.messagesDefault err   
                                 (dtDataDtoGetDataTable >> dtDataTransformLayerGet <| row ) 
        
        let seqFromDataTable = dt.AsEnumerable() |> Seq.distinct 

        match validity with
        | FutureValidity -> 
                          seqFromDataTable                          
                          |> Seq.filter
                              (fun row ->
                                        let startDate = (row |> dataTransformation).startDate
                                        let endDate = (row |> dataTransformation).endDate
                                        let fileToBeSaved = (row |> dataTransformation).fileToBeSaved                      
                                        condition startDate endDate currentTime fileToBeSaved
                              )     
                          |> Seq.map
                              (fun row ->
                                        (row |> dataTransformation).completeLink,
                                        (row |> dataTransformation).fileToBeSaved
                              )
                          |> Seq.distinct //na rozdil od SQL v ITVF se musi pouzit distinct
                          |> List.ofSeq

        | _              -> 
                          seqFromDataTable
                          |> Seq.filter
                              (fun row ->
                                        let startDate = (row |> dataTransformation).startDate
                                        let endDate = (row |> dataTransformation).endDate
                                        let fileToBeSaved = (row |> dataTransformation).fileToBeSaved                      
                                        condition startDate endDate currentTime fileToBeSaved
                              )           
                          |> Seq.sortByDescending (fun row -> (row |> dataTransformation).startDate)
                          |> Seq.groupBy (fun row -> (row |> dataTransformation).newPrefix)
                          |> Seq.map
                              (fun (newPrefix, group)
                                  ->
                                   newPrefix, 
                                   group |> Seq.head
                              )
                          |> Seq.map
                              (fun (_ , row) 
                                  ->
                                   (row |> dataTransformation).completeLink,
                                   (row |> dataTransformation).fileToBeSaved
                              )
                          |> Seq.distinct 
                          |> List.ofSeq