namespace Types 

[<Struct>]
type internal Validity =
    | CurrentValidity 
    | FutureValidity 
    | ReplacementService 
    | WithoutReplacementService 

type internal msg =
    | Incr of int
    | Fetch of AsyncReplyChannel<int>