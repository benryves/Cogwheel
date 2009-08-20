; Setup Script for SlimDX dependency.

[Files]
Source: "Dependencies\SlimDX Runtime (March 2009 SP1).msi"; DestDir: "{tmp}"; Flags: ignoreversion;

[Run]
Filename: "MSIEXEC.EXE"; Parameters: "/I ""{tmp}\SlimDX Runtime (March 2009 SP1).msi"" /QB"; StatusMsg: "Installing SlimDX (March 2009 SP1)...";
