namespace WebClient.Controllers

open System
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2


[<ApiController>]
[<Route("[controller]")>]
type TaskController (logger : ILogger<TaskController>) =
    inherit ControllerBase()


    [<HttpGet>]
    member __.Get() : unit =
        ()

    //TODO add post and use createTask workflow
      
