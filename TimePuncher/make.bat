@echo off
setlocal enabledelayedexpansion

@rem **************************************************************************
title (1/9) 環境変数の設定
@rem **************************************************************************

set project_name=RedmineTimePuncher

@rem Visual Studio 2022
set VisualStudioPath=C:\Program Files\Microsoft Visual Studio\2022\Community\Common7
if not exist "!VisualStudioPath!" (
    @rem Visual Studio 2022がなければ、2019に変更する。
    @rem 「"」をつけずに　C:\Program Files (x86)　配下のパスを指定しようとすると、
    @rem 「\Microsoft の使い方が間違っています。」のエラーが発生し、make.bat の実行が失敗してしまう。
    @rem よって、「"」をつけて変数に格納する。この後の処理含め、動作に問題ないことは確認済み。
    set VisualStudioPath="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7"
)
if not exist "!VisualStudioPath!" (
	call :echoErr "Visual Studioがインストールされていません。"
	goto ERROR
)
set ConfuserPath=C:\Program Files (x86)\ConfuserEx-CLI
if not exist "%ConfuserPath%" (
	call :echoErr "Confuserがインストールされていません。"
	goto ERROR
)
set InnoSetupPath=C:\Program Files (x86)\Inno Setup 6
if not exist "%InnoSetupPath%" (
	call :echoErr "Inno がインストールされていません。"
	goto ERROR
)
set FfftpExe=C:\Program Files\ffftp\ffftp.exe
if not exist "%FfftpExe%" (
	call :echoErr "Ffftp がインストールされていません。"
	goto ERROR
)

pushd "!VisualStudioPath!"
call .\Tools\VsDevCmd.bat
@rem **** MS-Installerを使わないので不要になった。**** 
@rem call .\IDE\CommonExtensions\Microsoft\VSI\DisableOutOfProcBuild\DisableOutOfProcBuild.exe
@rem if errorlevel 1 (
@rem 	call :echoErr "DisableOutOfProcBuild.exe の実行に失敗しました。"
@rem 	goto ERROR
@rem )
popd

set bin_Release=%project_name%\bin\Release
set bin_Packed=%project_name%\bin\Packed
set bin_Confused=%project_name%\bin\Confused

@rem **************************************************************************
title (2/9) Costura.Fodyが有効な状態でビルド
@rem **************************************************************************
if exist %bin_Release% (
	rmdir /s /q %bin_Release%
)
call devenv ..\SampleProject.sln /Rebuild "Release" /project %project_name% /projectconfig "Release"
if errorlevel 1 (
	call :echoErr "Costura.Fody　が有効な状態でリリースビルド の実行に失敗しました。"
	goto ERROR
)

@rem **************************************************************************
title (3/9) 有効な状態の成果物を bin/Packed に退避
@rem **************************************************************************
rmdir /s /q %bin_Packed%
mkdir %bin_Packed%
echo D | xcopy /E /Y %bin_Release% %bin_Packed%

@rem **************************************************************************
title (4/9) Costura.Fodyが無効な状態でビルド
@rem **************************************************************************
ren %project_name%\FodyWeavers.xml FodyWeavers.xml.org 
echo F | xcopy /Y empty_FodyWeavers.xml %project_name%\FodyWeavers.xml
call devenv ..\SampleProject.sln /Rebuild "Release" /project %project_name% /projectconfig "Release"
if errorlevel 1 (
	call :echoErr "Costura.Fody　を無効にしてリリースビルド の実行に失敗しました。"
	goto ERROR
)
del /q %project_name%\FodyWeavers.xml
ren %project_name%\FodyWeavers.xml.org FodyWeavers.xml 

@rem **************************************************************************
title (5/9) Costura.Fodyが有効な状態の成果物で上書き
@rem **************************************************************************
echo D | xcopy /E /Y %bin_Packed% %bin_Release%
if errorlevel 1 (
	call :echoErr "Costura.Fodyが有効な状態の成果物で上書きで失敗しました。"
	goto ERROR
)

@rem **************************************************************************
title (6/9) 難読化
@rem **************************************************************************
"%ConfuserPath%\Confuser.CLI.exe" -n Confuser.crproj
if errorlevel 1 (
	call :echoErr "難読化に失敗しました。"
	goto ERROR
)

@rem **************************************************************************
title (7/9) 難読化された exe で bin/Packed の exe を上書きし、 bin/Release にリネーム
@rem **************************************************************************
echo D | xcopy /E /Y %bin_Confused% %bin_Packed%
if errorlevel 1 (
	call :echoErr "難読化された exe で bin/Packed の exe を上書きするのに失敗しました。"
	goto ERROR
)
rmdir /S /Q %bin_Confused%
rmdir /S /Q %bin_Release%
ren %bin_Packed% Release

@rem **************************************************************************
title (8/9) インストーラ作成
@rem **************************************************************************
"%InnoSetupPath%\ISCC.exe" %~dp0\Setup\Setup.iss
if errorlevel 1 (
	call :echoErr "インストーラ作成に失敗しました。"
	goto ERROR
)

@rem **************************************************************************
title (9/9) Explorer と FFFTP の起動
@rem **************************************************************************
explorer.exe "%~dp0Setup\Output"
start "ffftp" "%FfftpExe%" -s "redmine-power"

echo.
echo ###########################
echo #                         #
echo #   make.bat completed    #
echo #                         #
echo ###########################
echo.
title コマンドプロンプト

goto EOF

:echoErr
echo %ESC%[41m%~1%ESC%[0m
exit /b

:ERROR
echo error occured.
pause

:EOF
