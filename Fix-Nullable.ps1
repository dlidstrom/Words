"Updating files..."
$files = Get-ChildItem -Filter *.cs -Recurse -Path . |
    Where-Object { $_.FullName -notmatch "obj\\Debug|obj\\Release|AssemblyInfo" } |
    Where-Object { (Get-Content -Raw $_.FullName) -notmatch "auto-generated" }

$files |
    Where-Object { (Get-Content -Raw $_.FullName) -notmatch "\#nullable" } |
    ForEach-Object { $c = (Get-Content -Raw $_.FullName); ("#nullable enable`r`n`r`n" + $c) |
    Out-File -NoNewline -Encoding utf8BOM $_.FullName; $_.FullName }
