open System
open FSharp.Control.Tasks.V2
open Microsoft.Extensions.Logging
open Orleans
open Orleans.Configuration
open Orleans.Hosting
open Orleankka.Cluster
open Contracts.TaskGrain
open Grains.TaskGain


let getSilo () = task {
  use codeGenLoggerFactory = new LoggerFactory();
  let host =
     SiloHostBuilder()
      .UseLocalhostClustering()
      .Configure(fun (x:ClusterOptions) -> x.ClusterId <- "dev" 
                                           x.ServiceId <- "OrleansBasic")
      .ConfigureApplicationParts(fun parts -> 
        parts.AddApplicationPart(typeof<ITaskCreate>.Assembly).WithCodeGeneration(codeGenLoggerFactory)
             .AddApplicationPart(typeof<CreateTaskGrain>.Assembly).WithCodeGeneration(codeGenLoggerFactory) |> ignore)
      .ConfigureLogging(fun (logging:ILoggingBuilder) -> logging.AddConsole() |> ignore)
      .UseOrleankka()
      .Build()
  do! host.StartAsync()
  return host
}

let runMainAsync () = task {
  let! silo = getSilo()
  printfn "\n\n Press Enter to terminate...\n\n"
  Console.ReadLine() |> ignore
  do! silo.StopAsync()
}
  
  

[<EntryPoint>]
let main argv =
    runMainAsync ()
    |> Async.AwaitTask
    |> Async.RunSynchronously
    0 