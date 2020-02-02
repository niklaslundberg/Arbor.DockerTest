@ECHO OFF

SET Arbor.Build.Vcs.Branch.Name=%GITHUB_REF%

call dotnet arbor-build