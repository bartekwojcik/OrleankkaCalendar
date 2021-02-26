namespace Contracts
open Orleankka
open Orleankka.FSharp
open System.Net
module Say =

  type INumeric1 =
    abstract Add: x: int -> y: int -> int

  type IDupa =
    abstract member Dupuj : x:int -> int

  type HelloMessages =
    | Hi
    | Hello of string
    | Bue

  type IHello =
    inherit IActorGrain<HelloMessages>
    inherit Orleans.IGrainWithStringKey
    



