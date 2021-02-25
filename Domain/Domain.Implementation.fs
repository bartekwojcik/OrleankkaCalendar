﻿module Domain.Implementation
open Rop
open Domain.PublicTypes


let createRecurringTask 
        createTaskFromPrimitive // dependency
        : CreateRecurringTask =
    fun unvalidatedTask -> 
        async {
            let taskR  = createTaskFromPrimitive
                            unvalidatedTask.Id
                            unvalidatedTask.TaskTitle
                            unvalidatedTask.StartTime
                            unvalidatedTask.Duration
                            unvalidatedTask.Category
                            unvalidatedTask.Description
                            unvalidatedTask.Subtasks
                            unvalidatedTask.RepeatFormat
            return taskR
        }