# Testing rules

- Project: .NET (net10.0), tests live in FotmobSync.Tests, organised by feature folder.
- Follow the style of existing tests (framework, naming, assertions). Name tests Method_Scenario_ExpectedResult.
- Tests must be pure unit tests: no database, no network, no Playwright.
- Never change production code to make a test pass. If you think the code has a bug, stop and report it.
- Always run `dotnet test FootballSystem.slnx` and report the result.
