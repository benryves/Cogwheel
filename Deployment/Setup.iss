; Setup Script for Cogwheel.
; Ben Ryves / Bee Development 2009.
[Setup]
AppName=Cogwheel
AppVerName=Cogwheel
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
PrivilegesRequired=admin
InfoBeforeFile=InfoBefore.rtf
WizardImageFile=WizardImageFile.bmp
WizardSmallImageFile=WizardSmallImageFile.bmp

#include "SlimDX.iss"

[Files]
; Main executables:
Source: "..\Cogwheel SlimDX\bin\Release\Cogwheel Interface.exe"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\Cogwheel SlimDX\bin\Release\Cogwheel Interface.exe.config"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\Cogwheel SlimDX\bin\Release\Sega.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\Cogwheel SlimDX\bin\Release\Brazil.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";
Source: "..\State Thumbnailer\bin\Release\CogStateThumbnailer.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";

; ROM Data:
Source: "Dependencies\ROM Data\Master System.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "Dependencies\ROM Data\Game Gear.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "Dependencies\ROM Data\SG-1000.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "Dependencies\ROM Data\SC-3000.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "Dependencies\ROM Data\SF-7000.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";
Source: "Dependencies\ROM Data\ColecoVision.romdata"; DestDir: "{app}\ROM Data"; Flags: ignoreversion; Components: "RomData";

; VGM Player stub:
Source: "Dependencies\vgmplayer.stub"; DestDir: "{app}"; Flags: ignoreversion; Components: "Cogwheel";

[Registry]
; Register .cogstate file type:
Root: HKCR; Subkey: ".cogstate"; ValueType: string; ValueData: "Cogwheel.SaveState"
Root: HKCR; Subkey: "Cogwheel.SaveState"; ValueType: string; ValueData: "Cogwheel SaveState";
Root: HKCR; Subkey: "Cogwheel.SaveState\shell\open\command"; ValueType: string; ValueData: """{app}\Cogwheel Interface.exe"" ""%1""";
; Thumbnailer:
Root: HKCR; Subkey: "Cogwheel.SaveState\shellex\{{e357fccd-a995-4576-b01f-234630154e96}"; ValueType: string; ValueData: "{{f5d20abb-95b3-4a4c-8b60-b3df9d872a63}";


[Components]
Name: "Cogwheel"; Description: "Cogwheel emulator"; Types: full compact custom; Flags: fixed;
Name: "RomData"; Description: "ROM data files"; Types: full custom;

[INI]
Filename: "{app}\Cogwheel Website.url"; Section: "InternetShortcut"; Key: "URL"; String: "http://www.bee-dev.com/?go=cogwheel";

[Icons]
Name: "{group}\Cogwheel"; Filename: "{app}\Cogwheel Interface.exe";
Name: "{group}\{cm:ProgramOnTheWeb,Cogwheel}"; Filename: "{app}\Cogwheel Website.url";
Name: "{group}\{cm:UninstallProgram,Cogwheel}"; Filename: "{uninstallexe}";

[Run]
Filename: "{win}\Microsoft.NET\Framework\v2.0.50727\RegAsm.exe"; Parameters: "/codebase ""{app}\CogStateThumbnailer.dll"""; Flags: runhidden skipifdoesntexist;  StatusMsg: "Registering thumbnailer..."

[UninstallRun]
Filename: "{win}\Microsoft.NET\Framework\v2.0.50727\RegAsm.exe"; Parameters: "/unregister ""{app}\CogStateThumbnailer.dll"""; Flags: runhidden skipifdoesntexist;

[UninstallDelete]
Type: files; Name: "{app}\Cogwheel Website.url";
