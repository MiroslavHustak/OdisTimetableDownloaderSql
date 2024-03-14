namespace Types 

[<Struct>]
type internal Validity =
    | CurrentValidity 
    | FutureValidity 
    | ReplacementService 
    | WithoutReplacementService 

type internal Msg =
    | Incr of int
    | Fetch of AsyncReplyChannel<int>

type MailboxMessage =
    | First of int  

type MailBoxProcessorType =
    | Json 
    | Pdf