# Microsoft Developer Studio Project File - Name="saoss" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Application" 0x0101

CFG=saoss - Win32 Debug
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE 
!MESSAGE NMAKE /f "saoss.mak".
!MESSAGE 
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE 
!MESSAGE NMAKE /f "saoss.mak" CFG="saoss - Win32 Debug"
!MESSAGE 
!MESSAGE Possible choices for configuration are:
!MESSAGE 
!MESSAGE "saoss - Win32 Release" (based on "Win32 (x86) Application")
!MESSAGE "saoss - Win32 Debug" (based on "Win32 (x86) Application")
!MESSAGE 

# Begin Project
# PROP AllowPerConfigDependencies 0
# PROP Scc_ProjName ""
# PROP Scc_LocalPath ""
CPP=cl.exe
MTL=midl.exe
RSC=rc.exe

!IF  "$(CFG)" == "saoss - Win32 Release"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "saoss___Win32_Release"
# PROP BASE Intermediate_Dir "saoss___Win32_Release"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "Release"
# PROP Intermediate_Dir "Release"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_MBCS" /YX /FD /c
# ADD CPP /nologo /MDd /W3 /GX /Ot /Oy /I "src\\" /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_MBCS" /YX /FD /c
# ADD BASE MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x40b /d "NDEBUG"
# ADD RSC /l 0x40b /fo"saoss.res" /i ".\\" /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /machine:I386
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib msvcrt.lib /nologo /subsystem:windows /machine:I386 /nodefaultlib:"MSVCRTD.lib"

!ELSEIF  "$(CFG)" == "saoss - Win32 Debug"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir "saoss___Win32_Debug"
# PROP BASE Intermediate_Dir "saoss___Win32_Debug"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 1
# PROP Output_Dir "saoss___Win32_Debug"
# PROP Intermediate_Dir "saoss___Win32_Debug"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /D "_MBCS" /YX /FD /GZ /c
# ADD CPP /nologo /W3 /Gm /GX /ZI /Od /I "c:\projects\saoss\src" /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /D "_MBCS" /YX /FD /GZ /c
# ADD BASE MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x40b /d "_DEBUG"
# ADD RSC /l 0x40b /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /debug /machine:I386 /pdbtype:sept
# ADD LINK32 msvcrt.lib kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /debug /machine:I386 /pdbtype:sept

!ENDIF 

# Begin Target

# Name "saoss - Win32 Release"
# Name "saoss - Win32 Debug"
# Begin Group "Source Files"

# PROP Default_Filter "cpp;c;cxx;rc;def;r;odl;idl;hpj;bat"
# Begin Source File

SOURCE=.\src\Alus.cpp
# End Source File
# Begin Source File

SOURCE=.\src\aluspiirto.cpp
# End Source File
# Begin Source File

SOURCE=.\src\Alustuksia.cpp
# End Source File
# Begin Source File

SOURCE=.\src\audio.cpp
# End Source File
# Begin Source File

SOURCE=.\src\chat.cpp
# End Source File
# Begin Source File

SOURCE=.\src\ctf.cpp
# End Source File
# Begin Source File

SOURCE=.\src\dmenu.cpp
# End Source File
# Begin Source File

SOURCE=.\src\domination.cpp
# End Source File
# Begin Source File

SOURCE=.\src\glapi.cpp
# End Source File
# Begin Source File

SOURCE=.\src\lippu.cpp
# End Source File
# Begin Source File

SOURCE=.\src\localgame.cpp
# End Source File
# Begin Source File

SOURCE=.\src\Main.cpp
# End Source File
# Begin Source File

SOURCE=.\src\Menu.cpp
# End Source File
# Begin Source File

SOURCE=.\src\net.cpp
# End Source File
# Begin Source File

SOURCE=.\src\netav.cpp
# End Source File
# Begin Source File

SOURCE=.\src\netgamesets.cpp
# End Source File
# Begin Source File

SOURCE=.\src\nethelp.cpp
# End Source File
# Begin Source File

SOURCE=.\src\netpeli.cpp
# End Source File
# Begin Source File

SOURCE=.\src\peliloppu.cpp
# End Source File
# Begin Source File

SOURCE=.\src\Player.cpp
# End Source File
# Begin Source File

SOURCE=.\saoss.rc
# End Source File
# Begin Source File

SOURCE=.\src\scene.cpp
# End Source File
# Begin Source File

SOURCE=.\src\Sets.cpp
# End Source File
# Begin Source File

SOURCE=.\src\sieni.cpp
# End Source File
# Begin Source File

SOURCE=.\src\Teksti.cpp
# End Source File
# End Group
# Begin Group "Header Files"

# PROP Default_Filter "h;hpp;hxx;hm;inl"
# Begin Source File

SOURCE=.\src\Alus.h
# End Source File
# Begin Source File

SOURCE=.\src\aluspiirto.h
# End Source File
# Begin Source File

SOURCE=.\src\audio.h
# End Source File
# Begin Source File

SOURCE=.\src\begin_code.h
# End Source File
# Begin Source File

SOURCE=.\src\chat.h
# End Source File
# Begin Source File

SOURCE=.\src\close_code.h
# End Source File
# Begin Source File

SOURCE=.\src\ctf.h
# End Source File
# Begin Source File

SOURCE=.\src\Define.h
# End Source File
# Begin Source File

SOURCE=.\src\dmenu.h
# End Source File
# Begin Source File

SOURCE=.\src\domination.h
# End Source File
# Begin Source File

SOURCE=.\src\Eaija.h
# End Source File
# Begin Source File

SOURCE=.\src\glapi.h
# End Source File
# Begin Source File

SOURCE=.\src\Kaasujalki.h
# End Source File
# Begin Source File

SOURCE=.\src\lippu.h
# End Source File
# Begin Source File

SOURCE=.\src\LippuStart.h
# End Source File
# Begin Source File

SOURCE=.\src\main.h
# End Source File
# Begin Source File

SOURCE=.\src\net.h
# End Source File
# Begin Source File

SOURCE=.\src\netav.h
# End Source File
# Begin Source File

SOURCE=.\src\netgamesets.h
# End Source File
# Begin Source File

SOURCE=.\src\nethelp.h
# End Source File
# Begin Source File

SOURCE=.\src\Pati.h
# End Source File
# Begin Source File

SOURCE=.\src\peliloppu.h
# End Source File
# Begin Source File

SOURCE=.\src\Player.h
# End Source File
# Begin Source File

SOURCE=.\src\scene.h
# End Source File
# Begin Source File

SOURCE=.\src\Sets.h
# End Source File
# Begin Source File

SOURCE=.\src\sieni.h
# End Source File
# Begin Source File

SOURCE=.\src\sirpale.h
# End Source File
# Begin Source File

SOURCE=.\src\Star.h
# End Source File
# Begin Source File

SOURCE=.\src\teksti.h
# End Source File
# Begin Source File

SOURCE=.\src\Viivakasa.h
# End Source File
# End Group
# Begin Group "Resource Files"

# PROP Default_Filter "ico;cur;bmp;dlg;rc2;rct;bin;rgs;gif;jpg;jpeg;jpe"
# Begin Source File

SOURCE=.\icon.ico
# End Source File
# End Group
# Begin Source File

SOURCE=.\lib\smpeg.lib
# End Source File
# Begin Source File

SOURCE=.\lib\SDL.lib
# End Source File
# Begin Source File

SOURCE=.\lib\SDL_image.lib
# End Source File
# Begin Source File

SOURCE=.\lib\SDL_net.lib
# End Source File
# Begin Source File

SOURCE=.\lib\SDLmain.lib
# End Source File
# Begin Source File

SOURCE="..\..\Program Files\Microsoft Visual Studio\VC98\Lib\OPENGL32.LIB"
# End Source File
# Begin Source File

SOURCE="..\..\Program Files\Microsoft Visual Studio\VC98\Lib\GLU32.LIB"
# End Source File
# Begin Source File

SOURCE=.\lib\fmodvc.lib
# End Source File
# End Target
# End Project
