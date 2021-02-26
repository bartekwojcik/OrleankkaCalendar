module Domain.Dto
open Rop
open System
open Domain.Types
open ResultComputationExpression
open AsyncResult

type DtoConvertionError =
    | RepeatFormatValueUnknown of int
    | StartTimeIsNull
    | DurationIsNull

[<AllowNullLiteralAttribute>]
type UnvalidatedTaskDTO() = 
    member val Id = 0 with get, set
    member val TaskTitle : string = null with get, set 
    member val StartTime : Nullable<DateTimeOffset> = Nullable<DateTimeOffset>() with get, set // Use the ISO 8601 format string: "yyyy-MM-ddTHH:mm:ssZZZ" https://stackoverflow.com/questions/55015041/datetimeoffset-in-the-asp-net-core-api-model
    member val Duration : Nullable<TimeSpan> = Nullable<TimeSpan>()  with get, set //D.HH:mm:nn https://stackoverflow.com/questions/50156326/json-format-for-a-timespan-that-can-be-bound-using-microsoft-extensions-configur
    member val Category : string = null  with get, set
    member val Description:string = null with get, set
    member val Subtasks : ResizeArray<string> = ResizeArray<string>() with get, set
    /// 0 - RepeatFormat.OnEveryNDays,
    /// 1 - RepeatFormat.OnEveryNWeeks,
    /// 2 - None,
    /// anything else - RepeatFormatValueUnknown error
    member val RepeatFormatType = 0 with get, set
    member val RepeatFormatInterval = 0 with get, set

let private intToRepeatFormat typeValue intervalValue =
    match typeValue with
    | 0 -> succeed (Some (RepeatFormat.OnEveryNDays intervalValue))
    | 1 -> succeed (Some (RepeatFormat.OnEveryNWeeks intervalValue))
    | 2 -> succeed (None)
    | n  -> fail (RepeatFormatValueUnknown n)

let private repeatFormatToInt rfOption =
    match rfOption with
    | Some rf -> match rf with
                    | RepeatFormat.OnEveryNDays days -> (0, days)
                    | RepeatFormat.OnEveryNWeeks weeks -> (1, weeks)
    | None -> (2,0)

/// Functions for converting between the DTO and corresponding domain object
module UnvalidatedTaskDTO =

    let doAsync f = async {
            return f
        }
    
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
            let! durationR = valueOrFail taskDto.Duration DurationIsNull |> doAsync
            let! subtasks = arrayToOption taskDto.Subtasks |> doAsync

            let resultFun = result {
                let! rf = repeatFormatR
                and! st = startTimeR
                and! d = durationR

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


type TaskDTO() =
    member val Id = 0 with get, set
    member val TaskTitle : string = null with get, set 
    member val StartTime : DateTimeOffset = DateTimeOffset() with get, set // Use the ISO 8601 format string: "yyyy-MM-ddTHH:mm:ssZZZ" https://stackoverflow.com/questions/55015041/datetimeoffset-in-the-asp-net-core-api-model
    member val Duration : TimeSpan = TimeSpan()  with get, set //D.HH:mm:nn https://stackoverflow.com/questions/50156326/json-format-for-a-timespan-that-can-be-bound-using-microsoft-extensions-configur
    member val Category : string = null  with get, set
    member val Description:string = null with get, set
    member val Subtasks : ResizeArray<string> = ResizeArray<string>() with get, set
    /// 0 - RepeatFormat.OnEveryNDays,
    /// 1 - RepeatFormat.OnEveryNWeeks,
    /// 2 - None,
    /// anything else - RepeatFormatValueUnknown error
    member val RepeatFormatType = 0 with get, set
    member val RepeatFormatInterval = 0 with get, set

module TaskDTO =
    let recurringTaskToDto (task:RecurringTask) : TaskDTO =
        let dto = TaskDTO()

        let subtasks = match task.Subtasks with
                        | None -> ResizeArray<string>()
                        | Some v-> v |> List.map Subtask.value |> ResizeArray
                                   
                                   
        let repeatTuple = repeatFormatToInt task.RepeatFormat
        let format,interval = repeatTuple

        dto.Id <- RecurringTaskId.getValue task.Id
        dto.TaskTitle <- Title.value task.TaskTitle
        dto.StartTime <- StartTime.value task.StartTime
        dto.Duration <- Duration.value task.Duration
        dto.Category <- Category.value task.Category
        dto.Description <- Description.value task.Description
        dto.Subtasks <- subtasks
        dto.RepeatFormatType <- format
        dto.RepeatFormatInterval <- interval

        dto