namespace Domain.Types
open Rop
open System
open ResultComputationExpression

type RecurringTaskErrors =
    | InvalidParameters of string  // when some of the primitive paramters can not be mapped to their proper values
    | TaskByIdNotExist of RecurringTaskId.T


type RepeatFormat = 
    | OnEveryNWeeks of int
    | OnEveryNDays of int

type RecurringTask = {
    Id : RecurringTaskId.T
    TaskTitle : Title
    StartTime : StartTime
    Duration : Duration
    Category : Category
    Description : Description
    Subtasks : Subtask list option
    RepeatFormat : RepeatFormat option
    }



module RecurringTask =

    let private mapIntErrors = function
        | MustBePositiveInteger field -> InvalidParameters $"Field: {field}. Intiger must be positive"

    let private mapStringErrors = function 
        | StringError.Missing field -> InvalidParameters $"Field: {field}. Is missing"
        | StringError.MustNotBeLongerThan (field, length)-> InvalidParameters $"Field: {field} can't be longer than {length} paramters"
    
    let create taskId title startTime duration category description subtasks repeatFormat =

        let value ={
                Id = taskId
                TaskTitle = title
                StartTime = startTime
                Duration = duration
                Category = category
                Description = description
                Subtasks = subtasks
                RepeatFormat = repeatFormat
                } 
        succeed value


    let crateFromPrimitive taskId title startTime duration category description (subtasks:string list option) (repeatFormat:RepeatFormat option) =
        result {
            let! id = RecurringTaskId.create taskId |> mapMessagesR mapIntErrors
            let! tt = Title.create title  |> mapMessagesR mapStringErrors
            let! st = StartTime.create startTime  |> mapMessagesR mapIntErrors
            let! dur = Duration.create duration  |> mapMessagesR mapIntErrors
            let! cat = Category.create category  |> mapMessagesR mapStringErrors
            let! desc = Description.create description  |> mapMessagesR mapStringErrors

            let strintToSubTask = (Subtask.create >> (mapMessagesR mapStringErrors)) 
            let sListToSubtaskList value =  List.map strintToSubTask value
                                              |> RopResultHelpers.sequence
            let! subtOption = someOrNone sListToSubtaskList subtasks |> RopResultHelpers.fromOptionToSuccess 
            let! value = create id tt st dur cat desc subtOption repeatFormat

            return value
        }






