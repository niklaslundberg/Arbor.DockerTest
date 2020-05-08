@ECHO OFF

SET Arbor.Build.Vcs.Branch.Name=%GITHUB_REF%
SET Arbor.Build.BuilderNumber.UnixEpochSecondsEnabled=true
SET Arbor.Build.Bootstrapper.AllowPrerelease=true

call dotnet arbor-build