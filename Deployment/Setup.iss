; Setup Script for Cogwheel.
; Ben Ryves / Bee Development 2008.
[Setup]
AppName=Cogwheel
AppVerName=Cogwheel Beta 2
AppPublisher=Bee Development
AppPublisherURL=http://www.bee-dev.com/?go=cogwheel
AppSupportURL=http://www.bee-dev.com/?go=cogwheel
AppUpdatesURL=http://www.bee-dev.com/?go=cogwheel
DefaultDirName={pf}\Cogwheel
DefaultGroupName=Cogwheel
AllowNoIcons=yes
OutputDir=..\Deployment
OutputBaseFilename=CogwheelSetup
Compression=lzma
SolidCompression=yes

[Files]
; Main executables:
Source: "..\Cogwheel SlimDX\bin\Release\Cogwheel Interface.exe"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\Cogwheel SlimDX\bin\Release\Cogwheel Interface.exe.config"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\Cogwheel SlimDX\bin\Release\Sega.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\Cogwheel SlimDX\bin\Release\Brazil.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\Cogwheel SlimDX\bin\Release\BeeZip.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\Cogwheel SlimDX\bin\Release\SlimDX.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";

; ROM Data:
Source: "..\Cogwheel SlimDX\bin\Release\ROM Data\Master System.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "..\Cogwheel SlimDX\bin\Release\ROM Data\Game Gear.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "..\Cogwheel SlimDX\bin\Release\ROM Data\SG-1000.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "..\Cogwheel SlimDX\bin\Release\ROM Data\SC-3000.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "..\Cogwheel SlimDX\bin\Release\ROM Data\SF-7000.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";

[Components]
Name: "Cogwheel"; Description: "Cogwheel Emulator"; Types: full compact custom; Flags: fixed;
Name: "RomData"; Description: "ROM Data Files"; Types: full custom;

[INI]
Filename: "{app}\Cogwheel Website.url"; Section: "InternetShortcut"; Key: "URL"; String: "http://www.bee-dev.com/?go=cogwheel";

[Icons]
Name: "{group}\Cogwheel"; Filename: "{app}\Cogwheel Interface.exe";
Name: "{group}\{cm:ProgramOnTheWeb,Cogwheel}"; Filename: "{app}\Cogwheel Website.url";
Name: "{group}\{cm:UninstallProgram,Cogwheel}"; Filename: "{uninstallexe}";

[UninstallDelete]
Type: files; Name: "{app}\Cogwheel Website.url";
