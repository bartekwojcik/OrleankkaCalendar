namespace DomainTests
open Domain.Dto
open Domain.PublicTypes
open Domain.Implementation
open FSharp.Collections

module DomainTests =

    open System
    open Xunit
    open Domain.Types
    open Rop

    [<Fact>]
    let ``Create RecurringTask from primitives - should suceed `` () =

        let id = 1
        let title = "title"
        let startTime = DateTimeOffset (DateTime.UtcNow)
        let duration = TimeSpan.FromHours 1.0
        let category = "category"
        let description = "description"
        let subtasks :string list option = None
        let rf:RepeatFormat option = None

        let task = RecurringTask.crateFromPrimitive id title 
                                    startTime duration
                                    category description 
                                    subtasks
                                    rf

    
        match task with
        | Success (v,msgs) -> Assert.Equal(1, RecurringTaskId.getValue v.Id)
        | Failure (msgs) -> Assert.True(false,"it should never be failure") 

    let checkNumberOf  predicate list=
        let fn count item = if predicate item then (count+1)
                            else count
        let numberOfTrue = list |> List.fold fn 0
        numberOfTrue



    let oneMsgsToString (msg:RecurringTaskErrors):string= 
        match msg with
        | InvalidParameters s -> s
        | TaskByIdNotExist (id') ->
                    let idInt = id' |> RecurringTaskId.getValue 
                    $"recurringtaskid of value {idInt} is missing"

    [<Fact>]
    let ``Create RecurringTask from primitives with incorrect values - should fail `` () =

        let rid = -1 //negative id
        let title = "" // missing title
        let startTime = DateTimeOffset (DateTime.UtcNow)
        let duration = TimeSpan.FromHours 1.0
        let category = "" //missing category
        let description = "description"
        let subtasks :string list option = None
        let rf:RepeatFormat option = None

        let task = RecurringTask.crateFromPrimitive rid title 
                                    startTime duration
                                    category description 
                                    subtasks
                                    rf

    
        match task with
        | Success (v,msgs) -> Assert.True(false,"this should never succeed") 
        | Failure (msgs) -> 
            
                      
            let msgsStrings = msgs |> List.map oneMsgsToString
            let nOfErrorsWithReccuinrId = checkNumberOf 
                                            (fun (a:string) -> a.ToLower().Contains("recurringtaskid"))
                                            msgsStrings

            let nOfErrorsWithCategory = checkNumberOf 
                                            (fun (a:string) -> a.ToLower().Contains("category"))
                                            msgsStrings

            let nOfErrorsWithTitle = checkNumberOf 
                                            (fun (a:string) -> a.ToLower().Contains("title"))
                                            msgsStrings

            Assert.Equal(3, msgs.Length) //3 invalid paramters errors
            Assert.Equal(1, nOfErrorsWithReccuinrId)
            Assert.Equal(1, nOfErrorsWithCategory)
            Assert.Equal(1, nOfErrorsWithTitle)


    [<Fact>]
    let ``Create unvalidated task DTO object and convert to UnvalidatedTask - should succeed`` () =

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

        let unvalidTaskR = UnvalidatedTaskDTO.toUnvalidatedTask dto |> Async.RunSynchronously

        match unvalidTaskR with
        | Success (task,msgs) -> Assert.Equal(dto.Id,task.Id)
                                 Assert.True(task.Subtasks.IsSome)
                                 Assert.Equal(2,task.Subtasks.Value.Length)
        | Failure _ -> Assert.True(false, "UnvalidatedTask should be created when all paramters of DTO are provided")

    [<Fact>]
    let ``Create unvalidated task DTO object and convert to UnvalidatedTask - should fail`` () =

        let dto = UnvalidatedTaskDTO()
        dto.Id <- 1
        dto.TaskTitle <- "Title"
        //dto.StartTime <- Nullable() //missing  starttime
        //dto.Duration <- null // missing duration
        dto.Category <-"category"
        dto.Description <- "description"
        dto.Subtasks <- ["asd" ; "aaaa" ] |> ResizeArray<string>
        dto.RepeatFormatInterval <- -1 //negative number
        dto.RepeatFormatType <- -9999 //random numeber 

        let unvalidTaskR = UnvalidatedTaskDTO.toUnvalidatedTask dto |> Async.RunSynchronously

        match unvalidTaskR with
        | Success (task,msgs) -> Assert.True(false, "UnvalidatedTask should not be created when DTO paramters are missing")
        | Failure (msgs) ->  Assert.Equal(3, msgs.Length)

    [<Fact>]
    let ``Full workflow - Create validated task starting with uncorrupted DTO - should succeed`` () =

        let createFromPrimitives = RecurringTask.crateFromPrimitive //providing dependency
        let createRecurringTask = createRecurringTask createFromPrimitives

        let workflow = fullTaskWorkflow dtoToUnvalidatedTask createRecurringTask

        let titleString = "Title"

        let dto = UnvalidatedTaskDTO()
        dto.Id <- 1
        dto.TaskTitle <- titleString
        dto.StartTime <- DateTimeOffset.UtcNow
        dto.Duration <- TimeSpan.FromHours(1.0)
        dto.Category <-"category"
        dto.Description <- "description"
        dto.Subtasks <- ["asd" ; "aaaa" ] |> ResizeArray<string>
        dto.RepeatFormatInterval <- 7
        dto.RepeatFormatType <- 0        

        let recurringTaskR = workflow dto |> Async.RunSynchronously

        match recurringTaskR with
        | Success (task, msgs) -> Assert.Equal(Title.value task.TaskTitle,titleString)
        | Failure (msgs) -> Assert.False(true, "Worflow should always succeed given correct DTO")


    [<Fact>]
    let ``Full workflow - Create validated task starting with corrupted DTO - should fail with DTO conversion errors`` () =

        let createFromPrimitives = RecurringTask.crateFromPrimitive //providing dependency
        let createRecurringTask = createRecurringTask createFromPrimitives

        let workflow = fullTaskWorkflow dtoToUnvalidatedTask createRecurringTask

        let dto = UnvalidatedTaskDTO()
        dto.Id <- -1
        dto.TaskTitle <- ""
        dto.StartTime <- Nullable()
        dto.Duration <- Nullable()
        dto.Category <- ""
        dto.Description <- ""
        dto.Subtasks <- null
        dto.RepeatFormatInterval <- 7
        dto.RepeatFormatType <- 5        

        let recurringTaskR = workflow dto |> Async.RunSynchronously

        match recurringTaskR with
        | Success (task, msgs) -> Assert.True(true, "Worflow should always fail given corruputed DTO")
        | Failure (msgs) -> Assert.Equal(3,msgs.Length)
                            let startTimeIsNullErrorOption = List.tryFind (fun e -> 
                                                                                match e with 
                                                                                | InvalidParameters m when String.Equals(m.ToLower(),"RepeatFormatValueUnknown of 5".ToLower()) -> true
                                                                                | _ -> false
                                                                           ) msgs
                            match startTimeIsNullErrorOption with
                            | None -> Assert.True(false, "No error of \"InvalidParameters RepeatFormatValueUnknown of 5\" when should be one")
                            | Some error -> Assert.True(true,"Just like expected - there is \"InvalidParameters RepeatFormatValueUnknown of 5\"")


    [<Fact>]
    let ``Full workflow - Create validated task starting with corrupted DTO - should fail with Unvalidated to Validated RecurringTask errors`` () =

        let createFromPrimitives = RecurringTask.crateFromPrimitive //providing dependency
        let createRecurringTask = createRecurringTask createFromPrimitives

        let workflow = fullTaskWorkflow dtoToUnvalidatedTask createRecurringTask

        let dto = UnvalidatedTaskDTO()
        dto.Id <- -1
        dto.TaskTitle <- ""
        dto.StartTime <- DateTimeOffset.UtcNow
        dto.Duration <- TimeSpan.FromDays 1.0
        dto.Category <- ""
        dto.Description <- ""
        dto.Subtasks <- null
        dto.RepeatFormatInterval <- 7
        dto.RepeatFormatType <- 0        

        let recurringTaskR = workflow dto |> Async.RunSynchronously

        match recurringTaskR with
        | Success (task, msgs) -> Assert.True(true, "Worflow should always fail given corruputed DTO")
        | Failure (msgs) -> Assert.Equal(4,msgs.Length)
                            let stringToFind = "Field: RecurringTaskId. Intiger must be positive"
                            let startTimeIsNullErrorOption = List.tryFind (fun e -> 
                                                                                match e with 
                                                                                | InvalidParameters m when String.Equals(m,stringToFind,StringComparison.InvariantCultureIgnoreCase) -> true
                                                                                | _ -> false
                                                                           ) msgs
                            match startTimeIsNullErrorOption with
                            | None -> Assert.True(false, $"No error of \"{stringToFind}\" when should be one")
                            | Some error -> Assert.True(true,$"Just like expected - there is \"{stringToFind}\"")




        
        





        
        
        
