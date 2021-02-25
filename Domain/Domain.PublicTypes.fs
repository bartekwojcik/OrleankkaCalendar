module Domain.PublicTypes

open AsyncResult
open Domain.Types
open Dto

type DtoToUnvalidatedTask =
    UnvalidatedTaskDTO ->  AsyncResult<UnvalidatedRecurringTask,DtoConvertionError>

type CreateRecurringTask =
    UnvalidatedRecurringTask -> AsyncResult<RecurringTask,RecurringTaskErrors>



