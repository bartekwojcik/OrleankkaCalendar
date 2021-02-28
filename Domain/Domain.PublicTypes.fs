module Domain.PublicTypes

open AsyncResult
open Domain.Types
open Dto
open Rop
open System.Threading.Tasks

type DtoToUnvalidatedTask =
    UnvalidatedTaskDTO ->  AsyncResult<UnvalidatedRecurringTask,DtoConvertionError>

type CreateRecurringTask =
    UnvalidatedRecurringTask -> AsyncResult<RecurringTask,RecurringTaskErrors>

type RecurringTaskToDto =
    RecurringTask -> TaskDTO

type FullTaskWorkflow =
   UnvalidatedTaskDTO -> AsyncResult<RecurringTask,RecurringTaskErrors>

type FullTaskWorkflowWithTaskDto =
    UnvalidatedTaskDTO -> AsyncResult<TaskDTO,RecurringTaskErrors>

///Creates new RecurringTask using rocket science and bleeding edge technology of Orleans
type TaskFromGrain = UnvalidatedTaskDTO ->  Task<RopResult<RecurringTask,RecurringTaskErrors>>
