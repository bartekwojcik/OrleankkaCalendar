namespace Contracts

open Orleankka
open Orleankka.FSharp
open System.Net
open Rop
open Domain.Dto


module TaskGrain =

    type TaskMessage =
        | CreateTask of UnvalidatedTaskDTO

    type ITaskCreate =
      inherit IActorGrain<TaskMessage>
      inherit Orleans.IGrainWithStringKey