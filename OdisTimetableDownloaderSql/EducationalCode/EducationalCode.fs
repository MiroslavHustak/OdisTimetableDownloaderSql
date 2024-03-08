﻿namespace EducationalCode

module OptionModule =

    let map f option =
        match option with
        | Some value -> Some (f value)
        | None       -> None

    let bind f option =
        match option with
        | Some value -> f value
        | None       -> None

module ResultModule =

    let map f result =
        match result with
        | Ok value    -> Ok (f value)
        | Error error -> Error error

    let bind f result =
        match result with
        | Ok value    -> f value
        | Error error -> Error error

type ReaderEC<'env,'a> = ReaderEC of action:('env -> 'a)
        
module ReaderEC =
    /// Run a ReaderEC with a given environment
    let run env (ReaderEC action)  =
        action env  // simply call the inner function

    /// Create a ReaderEC which returns the environment itself
    let ask = ReaderEC id

    /// Map a function over a ReaderEC
    let map f reader =
        ReaderEC (fun env -> f (run env reader))

    /// flatMap a function over a Reader
    let bind f reader =
        let newAction env =
            let x = run env reader
            run env (f x)
        ReaderEC newAction

module ReaderCE =

    type ReaderBuilderEC() =
        member __.Return(x) = ReaderEC (fun _ -> x)
        member __.Bind(x,f) = ReaderEC.bind f x
        member __.Zero() = ReaderEC (fun _ -> ())

    // the builder instance
    let reader = ReaderBuilderEC()
