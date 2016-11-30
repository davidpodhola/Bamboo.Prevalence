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
    member this.UpdateTask (task:Task) =
        _tasks.Item(task.ID) <- task
    member this.PendingTasks () =
        _tasks.Values |> Seq.cast<Task> |> Seq.where( fun p -> not p.Done ) 
    member this.LastTaskId () =
        _nextTaskID

[<EntryPoint>]
let main argv = 
    System.Threading.ThreadPool.SetMaxThreads( 100, 100 ) |> ignore

    let prevalenceBase = Path.Combine(Environment.CurrentDirectory, "data");

    let _engine = PrevalenceActivator.CreateTransparentEngine(typeof<ToDoList>, prevalenceBase);
    let _system : ToDoList = downcast _engine.PrevalentSystem
    
    let Prompt prompt =
        printfn "%s" prompt
        Console.ReadLine()

    let ShowPendingTasks () =
        printfn "ID\tDate Created\t\tSummary"
        _system.PendingTasks() |> Seq.iter( fun p -> printfn "%d\t%s" p.ID p.Summary )
        printfn "LastTaskId:%d" (_system.LastTaskId())

    let firstLowerChar (s:string) =
        s.ToLower().Chars(0)

    let UserChoice () =
        Prompt "(A)dd task\t(D)one with task\t(S)napshot\t(Q)uit\t(P)rint\t(T)est" |> firstLowerChar

    let DisplayUserMenu () =
        match UserChoice() with
        | 'a' -> 
            printfn "Add task"
            let summary = Prompt "Summary: "
            let task : Task = { ID=0; Summary=summary; Done=false; DateCreated=DateTime.MinValue } 
            _system.AddTask task
            false
        | 'q' ->  true
        | 's' -> 
            _engine.TakeSnapshot()
            false
        | 't' -> 
            for i in [1..100000] do
                async {
                    let task : Task = { ID=0; Summary=i.ToString(); Done=true; DateCreated=DateTime.MinValue } 
                    _system.AddTask task
                } |> Async.Start
            false
        | 'p' -> 
            ShowPendingTasks()
            false

    let mutable quit = false

    while not(quit) do 
        ShowPendingTasks()
        quit <- DisplayUserMenu()

    0
