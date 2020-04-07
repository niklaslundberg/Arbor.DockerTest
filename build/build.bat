@ECHO OFF

SET Arbor.Build.Vcs.Branch.Name=%GITHUB_REF%
SET Arbor.Build.BuilderNumber.UnixEpochSecondsEnabled=true

call dotnet arbor-build