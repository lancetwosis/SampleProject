@echo off

@rem --------------------------------------------------------------------------
@rem �o�[�W���������擾����
@rem --------------------------------------------------------------------------
call cscript /nologo ..\Tool\readVersion.vbs RedmineTimePuncher\Properties\AssemblyInfo.cs > setVersion.bat
call setVersion.bat
del setVersion.bat

@rem --------------------------------------------------------------------------
@rem ZIP�t�@�C�����𐶐�����
@rem --------------------------------------------------------------------------
set ZipFile=RedmineTimePuncher_v%MYVER%.zip
echo %ZipFile%

@rem --------------------------------------------------------------------------
@rem �t�@�C����u������
@rem --------------------------------------------------------------------------
set ofilename=target.tmp
type nul >%ofilename%
setlocal ENABLEDELAYEDEXPANSION
for /f "delims=" %%A in (target.ftp) do (
    set line=%%A
    echo !line:ZIPFILE=%ZipFile%!>>%ofilename%
)
endlocal

@rem --------------------------------------------------------------------------
@rem �t�@�C���]��
@rem --------------------------------------------------------------------------
ftp -s:target.tmp redmine-power.sakura.ne.jp
del target.tmp

pause
