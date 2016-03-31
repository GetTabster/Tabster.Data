$zip_archive = "$env:APPVEYOR_BUILD_FOLDER\Tabster.Data.zip"

Get-ChildItem "$env:APPVEYOR_BUILD_FOLDER\bin\$env:CONFIGURATION" -Recurse -Include *.dll, *.xml | %{7z a "$zip_archive" $_.FullName}