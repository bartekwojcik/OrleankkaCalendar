//namespace RopHelpers
//open Rop
// /// combine a list of results, monadically
module RopResultHelpers
open Rop
let sequence aListOfResults =

    let cons head tail = head :: tail
    let consR headR tailR = applyR (mapR cons headR) tailR
    let initialValue = Success ([],[]) // empty list inside Result

    List.foldBack consR aListOfResults initialValue

let fromOptionToSuccess (resulROption:RopResult<'T,'E> option) : RopResult<'T option,'E> =
    match resulROption with
    | None -> succeed None
    | Some resultR -> 
        match resultR with
        | Success (a,msgs)-> Success ((Some a),msgs)
        | Failure msgs -> Failure (msgs)
