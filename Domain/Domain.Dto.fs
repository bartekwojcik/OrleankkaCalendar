module Domain.Dto
open Rop
open System
open Domain.Types
open ResultComputationExpression
open AsyncResult

type DtoConvertionError =
    | RepeatFormatValueUnknown of int
    | StartTimeIsNull

[<AllowNullLiteralAttribute>]
type UnvalidatedTaskDTO() =
    member val Id = 0 with get, set
    member val TaskTitle : string = null with get, set 
    member val StartTime : Nullable<DateTimeOffset> = Nullable<DateTimeOffset>() with get, set // Use the ISO 8601 format string: "yyyy-MM-ddTHH:mm:ssZZZ" https://stackoverflow.com/questions/55015041/datetimeoffset-in-the-asp-net-core-api-model
    member val Duration : Nullable<TimeSpan> = Nullable<TimeSpan>()  with get, set //D.HH:mm:nn https://stackoverflow.com/questions/50156326/json-format-for-a-timespan-that-can-be-bound-using-microsoft-extensions-configur
    member val Category : string = null  with get, set
    member val Description:string = null with get, set
    member val Subtasks : ResizeArray<string> = new ResizeArray<string>() with get, set
    member val RepeatFormatType = 0 with get, set
    member val RepeatFormatInterval = 0 with get, set

/// Functions for converting between the DTO and corresponding domain object
module UnvalidatedTaskDTO =

    let doAsync f = async {
            return f
        }
    
    let private intToRepeatFormat typeValue intervalValue =
        match typeValue with
        | 0 -> succeed (Some (RepeatFormat.OnEveryNDays intervalValue))
        | 1 -> succeed (Some (RepeatFormat.OnEveryNWeeks intervalValue))
        | _ -> succeed None
        

    let arrayToOption (subtasks:ResizeArray<'T>) =
        match subtasks with 
        | null -> None
        | _ when subtasks.Count = 0 -> None
        | _ -> List.ofSeq(subtasks) |> Some

    let valueOrFail (nullable:Nullable<'T>) error =
        if nullable.HasValue then
            succeed nullable.Value
        else fail error


    
    let toUnvalidatedTask (taskDto:UnvalidatedTaskDTO): AsyncResult<UnvalidatedRecurringTask,DtoConvertionError> =
        async {
            let! repeatFormatR = intToRepeatFormat taskDto.RepeatFormatType taskDto.RepeatFormatInterval |> doAsync
            let! startTimeR = valueOrFail taskDto.StartTime StartTimeIsNull |> doAsync
            let! durationR = valueOrFail taskDto.Duration StartTimeIsNull |> doAsync
            let! subtasks = arrayToOption taskDto.Subtasks |> doAsync

            let resultFun = result {
                let! rf = repeatFormatR
                let! st = startTimeR
                let! d = durationR

                let task : UnvalidatedRecurringTask = {
                    Id = taskDto.Id
                    TaskTitle = taskDto.TaskTitle
                    StartTime = st
                    Duration = d
                    Description = taskDto.Description
                    Category = taskDto.Category
                    Subtasks = subtasks
                    RepeatFormat = rf
                    }
                return task
                }
        

            return resultFun
        }