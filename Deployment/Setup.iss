; Setup Script for Cogwheel.
; Ben Ryves / Bee Development 2008.
[Setup]
AppName=Cogwheel
AppVerName=Cogwheel Beta 3
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
ChangesAssociations=yes
ChangesEnvironment=yes

[Files]
; Main executables:
Source: "..\Cogwheel SlimDX\bin\Release\Cogwheel Interface.exe"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\Cogwheel SlimDX\bin\Release\Cogwheel Interface.exe.config"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\Cogwheel SlimDX\bin\Release\Sega.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\Cogwheel SlimDX\bin\Release\Brazil.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\Cogwheel SlimDX\bin\Release\SlimDX.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\State Thumbnailer\bin\Release\CogStateThumbnailer.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";

; ROM Data:
Source: "..\Cogwheel SlimDX\bin\Release\ROM Data\Master System.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "..\Cogwheel SlimDX\bin\Release\ROM Data\Game Gear.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "..\Cogwheel SlimDX\bin\Release\ROM Data\SG-1000.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "..\Cogwheel SlimDX\bin\Release\ROM Data\SC-3000.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "..\Cogwheel SlimDX\bin\Release\ROM Data\SF-7000.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";

[Registry]
; Register .cogstate file type:
Root: HKCR; Subkey: ".cogstate"; ValueType: string; ValueData: "Cogwheel.SaveState"
Root: HKCR; Subkey: "Cogwheel.SaveState"; ValueType: string; ValueData: "Cogwheel SaveState";
Root: HKCR; Subkey: "Cogwheel.SaveState\shell\open\command"; ValueType: string; ValueData: """{app}\Cogwheel Interface.exe"" ""%1""";
; Thumbnailer:
Root: HKCR; Subkey: "Cogwheel.SaveState\shellex\{{e357fccd-a995-4576-b01f-234630154e96}"; ValueType: string; ValueData: "{{f5d20abb-95b3-4a4c-8b60-b3df9d872a63}";


[Components]
Name: "Cogwheel"; Description: "Cogwheel Emulator"; Types: full compact custom; Flags: fixed;
Name: "RomData"; Description: "ROM Data Files"; Types: full custom;

[INI]
Filename: "{app}\Cogwheel Website.url"; Section: "InternetShortcut"; Key: "URL"; String: "http://www.bee-dev.com/?go=cogwheel";

[Icons]
Name: "{group}\Cogwheel"; Filename: "{app}\Cogwheel Interface.exe";
Name: "{group}\{cm:ProgramOnTheWeb,Cogwheel}"; Filename: "{app}\Cogwheel Website.url";
Name: "{group}\{cm:UninstallProgram,Cogwheel}"; Filename: "{uninstallexe}";

[Run]
Filename: "{win}\Microsoft.NET\Framework\v2.0.50727\RegAsm.exe"; Parameters: "/codebase ""{app}\CogStateThumbnailer.dll"""; Flags: runhidden skipifdoesntexist;

[UninstallRun]
Filename: "{win}\Microsoft.NET\Framework\v2.0.50727\RegAsm.exe"; Parameters: "/unregister ""{app}\CogStateThumbnailer.dll"""; Flags: runhidden skipifdoesntexist;


[UninstallDelete]
Type: files; Name: "{app}\Cogwheel Website.url";
