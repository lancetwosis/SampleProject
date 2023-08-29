Option Explicit

call readVersion(WScript.Arguments.Item(0))

'───────────────────────────────────────
' バージョンを取得する
'───────────────────────────────────────
sub readVersion(fileName)

    Dim i, j
    Dim readData 
    Dim lineData 
    Dim splitData
    Dim versionStr
    readData = ReadAll(fileName)
    lineData = split(readData, vbCrLf)
    For i = 0 to Ubound(lineData)
        If Instr(1, lineData(i), "[assembly: AssemblyVersion(") = 1 then
            splitData = split(split(lineData(i),"""")(1),".")
            for j = 0 to 2
                versionStr = versionStr & splitData(j)
            next
        end if
    Next
    WScript.StdOut.WriteLine("set MYVER=" & versionStr)
end sub


'───────────────────────────────────────
' ファイル読み込み
'───────────────────────────────────────
function ReadAll(filename)

    With CreateObject("ADODB.Stream")
      .charSet = "UTF-8"
      .Open
      .LoadFromFile (filename)
      ReadAll = .ReadText
      .Close
    End With
    
End function
