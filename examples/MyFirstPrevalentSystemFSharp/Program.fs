open System
open System.Collections
open System.IO
open Bamboo.Prevalence

type Task = {
    ID : int
    Summary : string
    Done : bool 
    DateCreated : DateTime
}

type ToDoList() = 
    inherit MarshalByRefObject()
    let mutable _nextTaskID = 0
    let _tasks = Hashtable()
    member this.AddTask (task:Task) =
        _nextTaskID <- _nextTaskID + 1
        _tasks.Add(_nextTaskID, {task with ID=_nextTaskID; DateCreated=PrevalenceEngine.Now} )
    member this.PendingTasks () =
        _tasks.Values |> Seq.cast<Task> |> Seq.where( fun p -> not p.Done ) 

[<EntryPoint>]
let main argv = 
    let prevalenceBase = Path.Combine(Environment.CurrentDirectory, "data");

    let _engine = PrevalenceActivator.CreateTransparentEngine(typeof<ToDoList>, prevalenceBase);
    let _system : ToDoList = downcast _engine.PrevalentSystem
    
    let Prompt prompt =
        printfn "%s" prompt
        Console.ReadLine()

    let ShowPendingTasks () =
        printfn "ID\tDate Created\t\tSummary"
        _system.PendingTasks() |> Seq.iter( fun p -> printfn "%d\t%s" p.ID p.Summary )

    let firstLowerChar (s:string) =
        s.ToLower().Chars(0)

    let UserChoice () =
        Prompt "(A)dd task\t(D)one with task\t(S)napshot\t(Q)uit" |> firstLowerChar

    let DisplayUserMenu () =
        match UserChoice() with
        | 'a' -> 
            printfn "Add task"
            let summary = Prompt "Summary: "
            let task : Task = { ID=0; Summary=summary; Done=false; DateCreated=DateTime.MinValue } 
            _system.AddTask task
            false
        | 'q' ->  true

    let mutable quit = false

    while not(quit) do 
        ShowPendingTasks()
        quit <- DisplayUserMenu()

    0
