namespace Contracts

open Orleankka
open Orleankka.FSharp
open System.Net
open Rop
open Domain.Dto

///Again, this task refers to Domain type "RecurringTask" - i could have done better job in naming
module TaskGrain =
    
    type TaskMessage =
        | CreateTask of UnvalidatedTaskDTO

    type ITaskCreate =
      inherit IActorGrain<TaskMessage>
      inherit Orleans.IGrainWithStringKey