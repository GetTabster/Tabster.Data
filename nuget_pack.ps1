$path = $env:APPVEYOR_BUILD_FOLDER + "\Tabster.Data.nuspec"
nuget pack $path -Version $env:APPVEYOR_BUILD_VERSION