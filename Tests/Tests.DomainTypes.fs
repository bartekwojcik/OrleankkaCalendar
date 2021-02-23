namespace DomainTests

module Tests =

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


    [<Fact>]
    let ``Create RecurringTask from primitives with incorrect values - should fail `` () =

        let id = 1
        let title = ""
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
        | Success (v,msgs) -> Assert.True(false,"this should never succeed") 
        | Failure (msgs) -> Assert.True(true,"this should fail") 