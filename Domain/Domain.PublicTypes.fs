module Domain.PublicTypes

open AsyncResult
open Domain.Types


type CreateRecurringTask =
    UnvalidatedRecurringTask -> AsyncResult<RecurringTask,RecurringTaskErrors>



