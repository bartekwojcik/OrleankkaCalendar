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
                     dep : CreateTaskWorkflow,
                     grainDep : GetTaskWithGrain,
                     worflowDep :FullTaskWorkflowWithNoFluff) =
    inherit ControllerBase()


    let toResponse content =
        match content with
        | Success (c, msgs) -> OkObjectResult c :> IActionResult
        | Failure (msgs) -> BadRequestObjectResult msgs :> IActionResult
        
    
    ///Example get instead of post so you dont have to send request to get the same response
    [<HttpGet>]
    [<Route("getTask1")>]
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

    [<HttpGet>]
    [<Route("getTask2")>]
    ///Same as "getTask1" but with full workflow encapsulated in dependency
    member __.GetTask2() : IActionResult =
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
                        |> worflowDep.FullTaskWorkflowWithTaskDto
                        |> Async.RunSynchronously                                             
                        |> toResponse
        httpResult

    [<HttpGet>]
    [<Route("getTask3")>]
    ///Same as "getTask1" but with asking Orleans/Orleankka to perform action
    member __.GetTask3() : IActionResult =
        
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


        let taskR = grainDep.GetTaskWithGrain dto |> Async.AwaitTask |> Async.RunSynchronously       
        let result = taskR |>  taskToDto |> toResponse 

        result



    //TODO create docker composer with databse image
      
