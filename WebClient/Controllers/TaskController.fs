namespace WebClient.Controllers

open System
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2
open WebClient.PrepareServices
open Domain.Dto

[<ApiController>]
[<Route("[controller]")>]
type TaskController (logger : ILogger<TaskController>,
                     dep : CreateTaskWorkflow) =
    inherit ControllerBase()


    [<HttpGet>]
    member __.Get() : unit =
        ()


    //TODO add post and use createTask workflow

    //TODO create POST to use GRAIN to do workflow and save to Database

    //TODO create docker composer with databse image
      
