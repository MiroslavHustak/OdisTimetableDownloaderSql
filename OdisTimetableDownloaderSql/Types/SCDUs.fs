namespace Types 

open System

//SCDUs for type-driven development

//Not strictly necessary in such a small app

[<Struct>]
type CompleteLinkOpt = CompleteLinkOpt of string option

[<Struct>]
type FileToBeSavedOpt = FileToBeSavedOpt of string option

[<Struct>]
type OldPrefix = OldPrefix of string 

[<Struct>]
type NewPrefix = NewPrefix of string 

[<Struct>]
type StartDate = StartDate of string 

[<Struct>]
type EndDate = EndDate of string 

[<Struct>]
type TotalDateInterval = TotalDateInterval of string 

[<Struct>]
type Suffix = Suffix of string 

[<Struct>]
type JsGeneratedString = JsGeneratedString of string 

[<Struct>]
type CompleteLink = CompleteLink of string 

[<Struct>]
type FileToBeSaved = FileToBeSaved of string 

[<Struct>]
type StartDateDt = StartDateDt of DateTime 

[<Struct>]
type EndDateDt = EndDateDt of DateTime 

[<Struct>]
type StartDateDtOpt = StartDateDtOpt of DateTime option

[<Struct>]
type EndDateDtOpt = EndDateDtOpt of DateTime option