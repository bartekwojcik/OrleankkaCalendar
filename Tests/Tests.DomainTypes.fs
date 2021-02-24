namespace DomainTests

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

        let rid = -1
        let title = ""
        let startTime = DateTimeOffset (DateTime.UtcNow)
        let duration = TimeSpan.FromHours 1.0
        let category = ""
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
