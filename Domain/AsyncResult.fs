module AsyncResult
open Rop


type AsyncResult<'Success,'TMessages> = 
    Async<RopResult<'Success,'TMessages>>

[<RequireQualifiedAccess>]
module Async =
    
    let map f xA = 
        async { 
        let! x = xA
        return f x 
    }


[<RequireQualifiedAccess>]
module AsyncResult =
    
    let mapR (f:'a -> AsyncResult<'b,_>) (ropR:RopResult<'a,_>) =
        async {

            match ropR with        
            | Success (x, msgs) -> let! res = f x
                                   return res
            | Failure (msgs) -> return Failure (msgs)
            
        }
    
    let bindA f x = async {
        let! result = f x
        return result
        }


    /////Lift a function to AsyncResult
    //let map f (x:AsyncResult<_,_>) : AsyncResult<_,_> =
    //    Async.map(mapR f) x
