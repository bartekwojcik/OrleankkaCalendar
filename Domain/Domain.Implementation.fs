module Domain.Implementation
open Rop
open Domain.PublicTypes
open Dto
open AsyncResult
open ResultComputationExpression

let dtoToUnvalidatedTask : DtoToUnvalidatedTask =
    fun dto ->
        dto |> UnvalidatedTaskDTO.toUnvalidatedTask

let createRecurringTask 
        createTaskFromPrimitive // dependency
        : CreateRecurringTask =
    fun unvalidatedTask -> 
        async {
            let taskR  = createTaskFromPrimitive
                            unvalidatedTask.Id
                            unvalidatedTask.TaskTitle
                            unvalidatedTask.StartTime
                            unvalidatedTask.Duration
                            unvalidatedTask.Category
                            unvalidatedTask.Description
                            unvalidatedTask.Subtasks
                            unvalidatedTask.RepeatFormat
            return taskR
        }

let errorMap = function
    | DtoConvertionError.DurationIsNull -> Types.RecurringTaskErrors.InvalidParameters "Duration is null"
    | DtoConvertionError.RepeatFormatValueUnknown a-> Types.RecurringTaskErrors.InvalidParameters $"RepeatFormatValueUnknown of {a}"
    | DtoConvertionError.StartTimeIsNull -> Types.RecurringTaskErrors.InvalidParameters "Start time is null"

let fullTaskWorkflow (dtoToUnvalidTask:DtoToUnvalidatedTask)
                    (unvalidToValidTask:CreateRecurringTask)  
                    : FullTaskWorkflow =
    fun dto -> 
        async {
            let! unvalidatedTaskR = dtoToUnvalidTask dto
            let convertedErrorTaskR = unvalidatedTaskR |> Rop.mapMessagesR errorMap
            let! validatedTaskR = AsyncResult.mapR unvalidToValidTask convertedErrorTaskR            

            return validatedTaskR
        }

