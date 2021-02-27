

module OrleankkaClient

open Rop
open Microsoft.Extensions.Logging
open Orleans
open Orleans.Configuration
open System
open System.Reflection
open FSharp.Control.Tasks.V2
open Contracts.TaskGrain
open Contracts.Say
open Orleans.Hosting
open Orleankka
open Orleankka.Client
open Orleankka.FSharp
open Domain.Dto
open System.Threading.Tasks
open Domain.PublicTypes

let doClientWork (system:IActorSystem) = task {
  let greeter = ActorSystem.typedActorOf<IHello, HelloMessages>(system,"greeter")

  let! hi = greeter <? Hi
  let! hello = greeter <? Hello "Roman"
  do! greeter <! Bue
  //do! greeter.Tell Bue
  do printfn "hi: %s \nhello: %s" hi hello

}

let createTaskJob(system:IActorSystem) (taskDto:UnvalidatedTaskDTO) = task {
  let grainId = $"{taskDto.Id}"
  let grain = ActorSystem.typedActorOf<ITaskCreate, TaskMessage>(system,grainId) //this id might be null

  let! taskR = grain <? CreateTask taskDto
  return taskR
  }


let runClient () = task {
    let codeGenLoggerFactory = new LoggerFactory();
    let client =
        ClientBuilder()
            .UseLocalhostClustering()
            .Configure(fun (x:ClusterOptions) -> x.ClusterId <- "dev";x.ServiceId <- "OrleansBasic" )
            .ConfigureLogging(fun (logging:ILoggingBuilder) -> logging.AddConsole() |> ignore)
            .ConfigureApplicationParts(fun parts -> 
            parts.AddApplicationPart(typeof<IHello>.Assembly).WithCodeGeneration(codeGenLoggerFactory) |> ignore)
            .UseOrleankka()
            .Build()
    do! client.Connect()
    Console.WriteLine("Client successfully connected to silo host \n");
    do! doClientWork (client.ActorSystem())
    //do Console.ReadKey() |> ignore
    return client
}

let getTaskFromGrain : TaskFromGrain=
        fun dto -> task {
        let! client = runClient()
        let actorSystem = client.ActorSystem()
        let! taskR = createTaskJob actorSystem dto
        return taskR
    }

[<EntryPoint>]
let main argv =
    // printfn "Hello World from F#!"
    let orleansClient = runClient ()
                        |> Async.AwaitTask
                        |> Async.RunSynchronously
    0 // return an integer exit code
