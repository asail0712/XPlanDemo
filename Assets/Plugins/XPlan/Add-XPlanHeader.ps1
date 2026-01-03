# ===============================
# Encoding settings
# ===============================
$sourceEncoding = [System.Text.Encoding]::GetEncoding(950)          # CP950 / Big5
$targetEncoding = New-Object System.Text.UTF8Encoding($false)       # UTF-8 (no BOM)

# ===============================
# Header content
# ===============================
$header = @"
// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================

"@

# ===============================
# Process files
# ===============================
Get-ChildItem -Recurse -Filter *.cs | ForEach-Object {

    # 用「指定來源編碼」讀檔
    $content = [System.IO.File]::ReadAllText($_.FullName, $sourceEncoding)

    if ($content -notmatch "XPlan Framework") {

        $newContent = $header + $content

        # 用「指定目標編碼」寫檔
        [System.IO.File]::WriteAllText($_.FullName, $newContent, $targetEncoding)

        Write-Host "Header added: $($_.FullName)"
    }
}

<#
執行方式
powershell -NoProfile -ExecutionPolicy Bypass -File .\Add-XPlanHeader.ps1
#>