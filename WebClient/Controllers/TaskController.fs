namespace WebClient.Controllers

open System
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2
open WebClient.PrepareServices
open Domain.Dto
open Rop

[<ApiController>]
[<Route("[controller]")>]
type TaskController (logger : ILogger<TaskController>,
                     dep : CreateTaskWorkflow) =
    inherit ControllerBase()


    let toResponse content =
        match content with
        | Success (c, msgs) -> OkObjectResult c :> IActionResult
        | Failure (msgs) -> BadRequestObjectResult msgs :> IActionResult
        
    
    ///Example get instead of post so you dont have to send request to get the same response
    [<HttpGet>]
    [<Route("simpleTask")>]
    member __.Get() : IActionResult =
        let workflow = dep.CreateTaskWorkflow
        let taskToDtoFun task = succeed (dep.RecurringTaskToDto task)
        let taskToDto task = bindR taskToDtoFun task

        //some random DTO that would come normally in POST request
        let dto = UnvalidatedTaskDTO()
        dto.Id <- 1
        dto.TaskTitle <- "Title"
        dto.StartTime <- DateTimeOffset.UtcNow
        dto.Duration <- TimeSpan.FromHours(1.0)
        dto.Category <-"category"
        dto.Description <- "description"
        dto.Subtasks <- ["asd" ; "aaaa" ] |> ResizeArray<string>
        dto.RepeatFormatInterval <- 7
        dto.RepeatFormatType <- 0   
        
        let httpResult = dto
                        |> workflow
                        |> Async.RunSynchronously                     
                        |> taskToDto 
                        |> toResponse
        httpResult


    //TODO create POST to use GRAIN to do workflow and save to Database

    //TODO create docker composer with databse image
      
