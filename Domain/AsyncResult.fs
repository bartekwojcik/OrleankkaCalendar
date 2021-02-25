module AsyncResult
open Rop

type AsyncResult<'Success,'TMessages> = 
    Async<RopResult<'Success,'TMessages>>


