@echo off

@rem --------------------------------------------------------------------------
@rem バージョン名を取得する
@rem --------------------------------------------------------------------------
call cscript /nologo ..\Tool\readVersion.vbs RedmineTimePuncher\Properties\AssemblyInfo.cs > setVersion.bat
call setVersion.bat
del setVersion.bat

@rem --------------------------------------------------------------------------
@rem ZIPファイル名を生成する
@rem --------------------------------------------------------------------------
set ZipFile=RedmineTimePuncher_v%MYVER%.zip
echo %ZipFile%

@rem --------------------------------------------------------------------------
@rem ファイルを置換する
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
@rem ファイル転送
@rem --------------------------------------------------------------------------
ftp -s:target.tmp redmine-power.sakura.ne.jp
del target.tmp

pause
