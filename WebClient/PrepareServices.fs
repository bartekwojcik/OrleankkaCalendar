namespace WebClient

open System.Threading.Tasks
open FSharp.Control.Tasks
open Domain.Types
open Domain.PublicTypes
open Domain.Implementation
open Domain.Dto
open OrleankkaClient

/// Service's type-safe interface to be provided to asp.net IOC
module PrepareServices =
    
    type CreateTaskWorkflow = {
        DtoToUnvalidatedTask : DtoToUnvalidatedTask
        CreateRecurringTask : CreateRecurringTask
        CreateTaskWorkflow : FullTaskWorkflow
        RecurringTaskToDto : RecurringTaskToDto
        }

    type FullTaskWorkflowWithNoFluff = {
        FullTaskWorkflowWithTaskDto : FullTaskWorkflowWithTaskDto
    }

    type GetTaskWithGrain =
        {
         GetTaskWithGrain : TaskFromGrain
        }
    


/// Implementations of interfaces to be provided to asp.net IOC
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


    let fullTaskWorkflowWithNoFluffFactory () =
        let createFromPrimitives = RecurringTask.crateFromPrimitive //providing dependency
        let taskWorkflow = createRecurringTask createFromPrimitives
        let workflow = fullTaskWorkflow dtoToUnvalidatedTask taskWorkflow
        
        let finalF = fullTaskWorkflowWithTaskDto workflow TaskDTO.recurringTaskToDto 
        
        {
            FullTaskWorkflowWithNoFluff.FullTaskWorkflowWithTaskDto = finalF
        }

    let getTaskWithGrainFactory () =
        let workflow = OrleankkaClient.getTaskFromGrain
        {
            GetTaskWithGrain.GetTaskWithGrain = workflow
        }
