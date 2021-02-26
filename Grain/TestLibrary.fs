namespace Grains

open Orleankka
open Orleankka.FSharp
open Contracts.Say
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2
open FSharp.Control.CommonExtensions
open System.Threading.Tasks



module Say =

  type HelloGrain (loggerFactory:ILoggerFactory) =
    inherit ActorGrain()
    let log = loggerFactory.CreateLogger(typeof<HelloGrain>)

    interface IDupa with
        member this.Dupuj (x:int) =
            x *2

    interface INumeric1 with
        member this.Add x y =
            x + y
        
    interface IHello 
    override this.Receive(msg) = task {
        match msg with
        
        | :? HelloMessages as m ->
            match m with
            | Hi -> 
                log.LogInformation("Client asked to say Hi!")
                return some "Oh, hi!"
            | Hello s ->
                log.LogInformation (sprintf "Client asked to say Hello to %s" s)
                return sprintf "Hello, %s" s |> some
            | Bue -> 
                log.LogInformation ("Client wants to go")
                return none()
            | _ ->  return unhandled()

        | :? Orleankka.Activate as a ->  printfn $"!##! grain {this.Id} is doing {a}. Message: {Activate.Message}"; return none()
        | a -> printfn $"####!! grain {this.Id} is doing {a} and its WEIRD"; return unhandled()
    }

    override this.OnActivateAsync () = 
        Task.Run(fun () -> printfn "##### activated") |> ignore
        base.OnActivateAsync()
        