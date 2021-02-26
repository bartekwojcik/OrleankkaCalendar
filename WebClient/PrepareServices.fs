namespace WebClient

open System.Threading.Tasks
open FSharp.Control.Tasks
open Domain.Types
open Domain.PublicTypes
open Domain.Implementation
open Domain.Dto

/// Service's type-safe interface to be provided to IOC
module PrepareServices =
    
    type CreateTaskWorkflow = {
        DtoToUnvalidatedTask : DtoToUnvalidatedTask
        CreateRecurringTask : CreateRecurringTask
        CreateTaskWorkflow : FullTaskWorkflow
        RecurringTaskToDto : RecurringTaskToDto
        }
    


/// Implementations of interfaces to be provided to IOC
module ServicesImplementation =
    open PrepareServices

    let createTaskWorkflowFactory () =
        let createFromPrimitives = RecurringTask.crateFromPrimitive //providing dependency
        let taskWorkflow = createRecurringTask createFromPrimitives

        let dtoWorkflow = UnvalidatedTaskDTO.toUnvalidatedTask
        let workflow = fullTaskWorkflow dtoToUnvalidatedTask taskWorkflow

        { 
            CreateTaskWorkflow.CreateRecurringTask = taskWorkflow 
            DtoToUnvalidatedTask = dtoWorkflow
            CreateTaskWorkflow = workflow
            RecurringTaskToDto = TaskDTO.recurringTaskToDto
        }



/// Module where i registered some dummy functions to play with IOC in functional manner
module ServicesTests =
    
    type DummyWorkflow = DummyWorkflow of (unit -> Task<string>)
    type DummyWorkflow2 = DummyWorkflow2 of (string -> string)
    type DummyWorkflow3 = {
        DoSomething : (string -> string)
        }

    let getDW3 () =
        let someFuction i s =
            $"%s{s} dupa %i{i}"

        let partalSomeFunction = someFuction 5

        {DoSomething = partalSomeFunction}
 
    type GetPerson = string -> int
 
    let topLevel b (s:string) =
        if b then
            s.Length
        else (s.Length) - 1
 
 
    let getGetPerson : GetPerson =
        fun s -> 
            topLevel true s