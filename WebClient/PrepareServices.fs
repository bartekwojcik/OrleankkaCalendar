﻿module PrepareServices

open System.Threading.Tasks
open FSharp.Control.Tasks
open Domain.Types
open Domain.PublicTypes
open Domain.Implementation

/// Service's type-safe interface to be provided to IOC
module PrepareServices =
    
    type CreateTaskWorkflow = {
        CreateRecurringTask : CreateRecurringTask
        }

/// Implementations of interfaces to be provided to IOC
module ServicesImplementation =
    open PrepareServices

    let createTaskWorkflowFactory () =
        let createFromPrimitives = RecurringTask.crateFromPrimitive //providing dependency
        let workflow = createRecurringTask createFromPrimitives
        { CreateTaskWorkflow.CreateRecurringTask = workflow}



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