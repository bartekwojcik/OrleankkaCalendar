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

            //TODO instead of this builder just use regular function (no result {} workflow) and collect all 
            // let (NO BANG) id = RecurringTaskId.create taskId |> mapMessagesR mapIntErrors
            // then pass them somewhere to some function ?? (like some binder) eaither combines all the failures or pass it to the create function? sounds wild

            let strintToSubTask = (Subtask.create >> (mapMessagesR mapStringErrors)) 
            let sListToSubtaskList value =  List.map strintToSubTask value
                                              |> RopResultHelpers.sequence

            let! id = RecurringTaskId.create taskId |> mapMessagesR mapIntErrors
            and! tt = Title.create title  |> mapMessagesR mapStringErrors
            and! st = StartTime.create startTime  |> mapMessagesR mapIntErrors
            and! dur = Duration.create duration  |> mapMessagesR mapIntErrors
            and! cat = Category.create category  |> mapMessagesR mapStringErrors
            and! desc = Description.create description  |> mapMessagesR mapStringErrors
            and! subtOption = someOrNone sListToSubtaskList subtasks |> RopResultHelpers.fromOptionToSuccess 
            let! value = create id tt st dur cat desc subtOption repeatFormat

            return value//create id tt st dur cat desc subtOption repeatFormat
        }







