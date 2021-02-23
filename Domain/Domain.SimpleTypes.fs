namespace Domain.Types
open Rop
open System

type StringError =
    | Missing of name:string
    | MustNotBeLongerThan of name:string*length:int


type IntegerError =
    | MustBePositiveInteger of name:string

module StringN =
    type T = StringN of string

    let create (s:string) (maxLength:int) (name:string) =
        match s with
        | null -> fail (StringError.Missing name)
        | _ when String.IsNullOrEmpty s -> fail (StringError.Missing name)
        | _ when s.Length > maxLength -> fail (MustNotBeLongerThan (name, maxLength))
        | _ -> succeed (StringN s)

    let create500 s = create s 500

    let apply f (StringN s) =
        f s


type Title = private Title of string
type StartTime = private StartTime of System.DateTimeOffset
type EndTime = private EndTime of System.DateTimeOffset
type Duration = private Duration of System.TimeSpan
type Category = private Category of string
type Subtask = private Subtask of string
type Description = private Description of string


module Title =
    let value (Title title) = title
    let create (s:string) =
        match StringN.create500 s "Title" with
        | Failure errs -> Failure errs
        | Success (s',_) -> succeed (Title s)

module StartTime =
    let value (StartTime offset) = offset
    let create (date:DateTimeOffset) =
        succeed (StartTime date)

module EndTime = 
    let value (EndTime offset) = offset
    let create (date:DateTimeOffset) =
        succeed (EndTime date)

module Duration = 
    let value (Duration offset) = offset
    let create (time:TimeSpan) =
        succeed (Duration time)
    
module Category =
    let value (Category title) = title
    let create (s:string) =
        match StringN.create500 s "Category" with
        | Failure errs -> Failure errs
        | Success (s',_) -> succeed (Category s)

module Subtask =
    let value (Subtask title) = title
    let create (s:string) =
        match StringN.create500 s "Subtask" with
        | Failure errs -> Failure errs
        | Success (s',_) -> succeed (Subtask s)

module Description =
    let value (Description title) = title
    let create (s:string) =
        match StringN.create500 s "Description" with
        | Failure errs -> Failure errs
        | Success (s',_) -> succeed (Description s)




module RecurringTaskId =
    type T = RecurringTaskId of int

    let create (i: int) =
        if i < 1 then
            fail (MustBePositiveInteger "RecurringTaskId")
        else 
            succeed (RecurringTaskId i)

    let apply f (RecurringTaskId i) =
        f i

    let getValue (i) =
        apply id i

module DoneTaskId =
    type T = DoneTaskId of int

    let create (i: int) =
        if i < 1 then
            fail ( MustBePositiveInteger "DoneTaskId")
        else 
            succeed (DoneTaskId i)

    let apply f (DoneTaskId i) =
        f i

    let getValue (i) =
        apply id i

//to mark some task as done 
type UnvalidatedTickTime = UnvalidatedTickTime of taskId: RecurringTaskId.T * time: DateTimeOffset

type ValidTickTime = { 
    TaskId: RecurringTaskId.T 
    Time: DateTimeOffset
    }

module ValidTickTime =
    
    let dateTimeSeqToTickTime taskId dateTimeSeq  =
        dateTimeSeq |>  Seq.map (fun time ->  {
                                               TaskId = taskId
                                               Time = time
                                                
                                              }) |> succeed



