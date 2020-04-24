open System.IO
open System.Text.RegularExpressions

let output (packageName, duplicates) =
    duplicates
    |> Seq.head
    |> fst
    |> printfn "Duplicate %s found in %s" packageName

let getPath args =
    args
    |> Array.tryHead
    |> Option.defaultValue (Directory.GetCurrentDirectory())

let getFileText fileName = fileName, File.ReadAllText fileName

let getFiles path =
    printfn "Looking for duplicates in %s" path
    Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories)

let parseFile (fileName, text) =
    Regex.Matches(text, "PackageReference Include=\"(.+)\"")
    |> Seq.cast<Match>
    |> Seq.choose (fun mtch -> mtch.Groups |> Seq.tryItem 1)
    |> Seq.map (fun group -> fileName, group.Value)

let hasDuplicates group =
    group
    |> snd
    |> Seq.length > 1

let processProjectFiles args =
    args
    |> getPath
    |> getFiles
    |> Seq.map getFileText
    |> Seq.collect parseFile
    |> Seq.groupBy snd
    |> Seq.filter hasDuplicates

[<EntryPoint>]
let main args =
    let duplicates = processProjectFiles args
    printfn "Duplicate detection complete"
    match duplicates |> Seq.isEmpty with
    | true -> 0
    | _ ->
        duplicates |> Seq.iter output
        1
