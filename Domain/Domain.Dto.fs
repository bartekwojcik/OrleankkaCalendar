module Domain.Dto
open Rop
open System
open Domain.Types
open ResultComputationExpression

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
    
    let toUnvalidatedTask (taskDto:UnvalidatedTaskDTO) =
        result {
            let! repeatFormat = intToRepeatFormat taskDto.RepeatFormatType taskDto.RepeatFormatInterval
            let! startTime = valueOrFail taskDto.StartTime StartTimeIsNull
            let! duration = valueOrFail taskDto.Duration StartTimeIsNull
            let subtasks = arrayToOption taskDto.Subtasks
        
            let result : UnvalidatedRecurringTask = {
                Id = taskDto.Id
                TaskTitle = taskDto.TaskTitle
                StartTime = startTime
                Duration = duration
                Description = taskDto.Description
                Category = taskDto.Category
                Subtasks = subtasks
                RepeatFormat = repeatFormat
                }

            return result
        }