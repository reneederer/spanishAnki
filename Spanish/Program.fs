open System.Net
open System.Text.RegularExpressions
open System.IO
open System



let downloadUrl (url:string) = 
    use client = new WebClient ()
    client.Encoding <- System.Text.Encoding.UTF8
    client.DownloadString url


let (|Match|_|) pattern input = 
    let m = Regex.Matches (input, pattern)
    in
        Some ([for i in [0..m.Count-1] -> (m.[i].Groups.[1].Value, m.[i].Groups.[2].Value)])
        
let getArticle (str:string) = 
    let m = if str.Contains("masculine noun") then "el" else ""
    let f = if str.Contains("feminine noun") then "la" else ""
    let sep = if m <> "" && f <> "" then " / " else ""
    let blank = if m <> "" || f <> "" then " " else ""
    in
        m + sep + f + blank
        

let groupMatches stringToSearch = 
    match stringToSearch with
    | Match """.*?<div class="thinguser"><i class="ico ico-seed ico-purple"></i></div><div class="ignore-ui pull-right"><input type="checkbox" ></div><div class="col_a col text"><div class="text">(.*?)</div></div><div class="col_b col text"><div class="text">(.*?)</div></div></div>""" result -> result
    | _ -> []


let toCSV wordSeq = 
    wordSeq
    |> Seq.rev
    |> Seq.fold
        (fun spanishEnglish (k,v) ->
            let a = sprintf "%s\t%s%s%s" k v Environment.NewLine spanishEnglish
            a)
            ""

let addArticles (wordList:seq<string*string>) =
    wordList
    |> Seq.map (fun (spanish, english) ->
          if english.StartsWith("to ")
          then (spanish, english)
          else
              let article = getArticle <| downloadUrl ("http://www.spanishdict.com/translate/" + spanish)
              in
                  (article + spanish, english))

let createWordFile = 
    for file in DirectoryInfo(System.IO.Directory.GetCurrentDirectory()).EnumerateFiles("*.txt") do
        file.Delete()
    Seq.fold (fun state i ->
                downloadUrl ("http://www.memrise.com/course/228070/1000-most-common-spanish-words-4/" + i.ToString())
                |> groupMatches
                |> Seq.append state)
              Seq.empty (seq [1..20])
    |> addArticles
    |> (fun wordSeq ->
            wordSeq
            |> toCSV
            |> (fun spanishEnglish ->
                File.AppendAllText ("spanishEnglish.txt", spanishEnglish)
        )
    )

[<EntryPoint>]
let main argv = 
    createWordFile
    0




