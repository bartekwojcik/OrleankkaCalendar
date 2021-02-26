module Domain.PublicTypes

open AsyncResult
open Domain.Types
open Dto
open Rop

type DtoToUnvalidatedTask =
    UnvalidatedTaskDTO ->  AsyncResult<UnvalidatedRecurringTask,DtoConvertionError>

type CreateRecurringTask =
    UnvalidatedRecurringTask -> AsyncResult<RecurringTask,RecurringTaskErrors>

type RecurringTaskToDto =
    RecurringTask -> TaskDTO

type FullTaskWorkflow =
   UnvalidatedTaskDTO -> AsyncResult<RecurringTask,RecurringTaskErrors>


