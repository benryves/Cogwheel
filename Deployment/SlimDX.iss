; Setup Script for SlimDX dependency.

[Files]
Source: "Dependencies\SlimDX Runtime (November 2008).msi"; DestDir: "{tmp}"; Flags: ignoreversion;

[Run]
Filename: "MSIEXEC.EXE"; Parameters: "/I ""{tmp}\SlimDX Runtime (November 2008).msi"" /QB"; StatusMsg: "Installing SlimDX (November 2008)...";
