module ResultComputationExpression
    open Rop
    type ResultBuilder() =
        member __.Return(x) = RopResult.Success (x,[])
        member __.Bind(x, f) = bindR f x

        member __.ReturnFrom(x) = x
        member this.Zero() = this.Return ()

        member __.Delay(f) = f
        member __.Run(f) = f()

        member _.BindReturn(x, f) = mapR f x

        member _.MergeSources(result1, result2) =
            match result1, result2 with
            | Success (ok1,msgs1), Success (ok2,msgs2) ->
                    Success ((ok1,ok2),msgs1@msgs2 ) // compiler will automatically de-tuple these - very cool!
            | Failure errs1, Success _ -> Failure errs1
            | Success _, Failure errs2 -> Failure errs2
            | Failure errs1, Failure errs2 -> Failure (errs1 @ errs2)   // accumulate errors

        member this.While(guard, body) =
            if not (guard()) 
            then this.Zero() 
            else this.Bind( body(), fun () -> 
                this.While(guard, body))  

        member this.TryWith(body, handler) =
            try this.ReturnFrom(body())
            with e -> handler e

        member this.TryFinally(body, compensation) =
            try this.ReturnFrom(body())
            finally compensation() 

        member this.Using(disposable:#System.IDisposable, body) =
            let body' = fun () -> body disposable
            this.TryFinally(body', fun () -> 
                match disposable with 
                    | null -> () 
                    | disp -> disp.Dispose())

        member this.For(sequence:seq<_>, body) =
            this.Using(sequence.GetEnumerator(),fun enum -> 
                this.While(enum.MoveNext, 
                    this.Delay(fun () -> body enum.Current)))

        member this.Combine (a,b) = 
            this.Bind(a, fun () -> b())

    let result = new ResultBuilder()



