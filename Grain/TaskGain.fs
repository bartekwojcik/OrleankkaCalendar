namespace Grains

open Orleankka
open Orleankka.FSharp
open Contracts.TaskGrain
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks
open FSharp.Control.CommonExtensions
open System.Threading.Tasks
open Domain.Types
open Domain.Implementation
open Domain.Dto
open Contracts.TaskGrain
open Rop
open AsyncResult

module TaskGain =
    ///Creates new RecurringTask using bleeding edge, rocket science Orleans messaging
    ///Extremely advanced logging technology
    type CreateTaskGrain (loggerFactory:ILoggerFactory) =
        inherit ActorGrain()
        let log = loggerFactory.CreateLogger(typeof<CreateTaskGrain>)

        let logF id= 
            log.LogInformation (sprintf "Client asked for task of id: %i" id)
        
        let logSuccess (t:RecurringTask,msgs) =
            let taskId = RecurringTaskId.getValue t.Id
            log.LogInformation (sprintf "Client asked for task of id: %i and it was given, o lord" taskId)

        let logFailure dtoId msgs =
            log.LogInformation 
                (sprintf "client asked to create task of Id \"%i\" but DTO was corrupted. Reasons: %O " dtoId msgs)


        let workflow =
            let createFromPrimitives = RecurringTask.crateFromPrimitive //providing dependency
            let taskWorkflow = createRecurringTask createFromPrimitives
            let workflow = fullTaskWorkflow dtoToUnvalidatedTask taskWorkflow
            workflow
        
        interface ITaskCreate
        override this.Receive(msg) = task {
            match msg with
            | :? TaskMessage as m-> 
                match m with
                | CreateTask dto -> 
                    logF dto.Id

                    let taskR = AsyncResult.bindA workflow dto |>  Async.RunSynchronously
                    successTee logSuccess taskR |> ignore
                    failureTee (logFailure dto.Id) taskR |> ignore
                    
                    return some taskR

            | _ -> return unhandled() 

            }