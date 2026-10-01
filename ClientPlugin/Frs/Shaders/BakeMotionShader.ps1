param([string]$Compiler)
$ErrorActionPreference = 'Stop'
if (-not $Compiler) {
    $sdkRoot = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
    $Compiler = Get-ChildItem -LiteralPath $sdkRoot -Filter fxc.exe -Recurse |
        Where-Object FullName -Match '\\x64\\fxc.exe$' |
        Sort-Object FullName | Select-Object -Last 1 -ExpandProperty FullName
}
if (-not $Compiler) { throw 'Install the Windows SDK shader compiler, or provide -Compiler.' }
$compiledPath = [IO.Path]::GetTempFileName()
try {
    & $Compiler /nologo /T ps_5_0 /E PSMain /O3 /Fo $compiledPath (Join-Path $PSScriptRoot 'Mv.hlsl')
    if ($LASTEXITCODE -ne 0) { throw 'Motion shader compilation failed.' }
    $bytes = [IO.File]::ReadAllBytes($compiledPath)
    $rows = for ($index = 0; $index -lt $bytes.Length; $index += 16) {
        $last = [Math]::Min($index + 15, $bytes.Length - 1)
        '        ' + ($bytes[$index..$last] -join ', ') + $(if ($last -lt $bytes.Length - 1) { ',' })
    }
    $bytecodePath = Join-Path $PSScriptRoot '..\ShaderBytecode.cs'
    $source = [IO.File]::ReadAllText($bytecodePath)
    $pattern = '(?s)(internal static readonly byte\[\] MvPs\s*=\s*\[).*?(\s*\];)'
    if ([regex]::Matches($source, $pattern).Count -ne 1) { throw 'Cannot locate the motion shader byte array.' }
    $replacement = [regex]::Replace($source, $pattern, [Text.RegularExpressions.MatchEvaluator]{
        param($match)
        $match.Groups[1].Value + "`r`n" + ($rows -join "`r`n") + "`r`n    ];"
    })
    [IO.File]::WriteAllText($bytecodePath, $replacement, [Text.UTF8Encoding]::new($false))
    Write-Output "Baked MvPs ($($bytes.Length) bytes)."
}
finally { Remove-Item -LiteralPath $compiledPath }
