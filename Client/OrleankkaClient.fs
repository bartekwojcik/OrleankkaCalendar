

module OrleankkaClient

open Rop
open Microsoft.Extensions.Logging
open Orleans
open Orleans.Configuration
open System
open System.Reflection
open FSharp.Control.Tasks.V2
open Contracts.TaskGrain

open Orleans.Hosting
open Orleankka
open Orleankka.Client
open Orleankka.FSharp
open Domain.Dto
open System.Threading.Tasks
open Domain.PublicTypes

///Example of calling grain logic.
///Unfortunate overlap in naming my domain type "RecurringTask" (just like google calendar's 'task') and System.Threading.Tasks.Task 
///makes function's name less readable.
let createTaskJob(system:IActorSystem) (taskDto:UnvalidatedTaskDTO) = task {
  let grainId = $"{taskDto.Id}"
  let grain = ActorSystem.typedActorOf<ITaskCreate, TaskMessage>(system,grainId) //this id might be null

  let! taskR = grain <? CreateTask taskDto
  return taskR
  }

/// Create and connect client to silo.
let runClient () = task {
    let codeGenLoggerFactory = new LoggerFactory();
    let client =
        ClientBuilder()
            .UseLocalhostClustering()
            .Configure(fun (x:ClusterOptions) -> x.ClusterId <- "dev";x.ServiceId <- "OrleansBasic" )
            .ConfigureLogging(fun (logging:ILoggingBuilder) -> logging.AddConsole() |> ignore)
            .ConfigureApplicationParts(fun parts -> 
            parts.AddApplicationPart(typeof<ITaskCreate>.Assembly).WithCodeGeneration(codeGenLoggerFactory) |> ignore)
            .UseOrleankka()
            .Build()
    do! client.Connect()
    Console.WriteLine("Client successfully connected to silo host \n");
    return client
}

///contract Interface-like function to be used in WebClient
let getTaskFromGrain : TaskFromGrain=
        fun dto -> task {
        let! client = runClient()
        let actorSystem = client.ActorSystem()
        let! taskR = createTaskJob actorSystem dto
        return taskR
    }

