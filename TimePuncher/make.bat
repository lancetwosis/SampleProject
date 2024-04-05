@echo off
setlocal enabledelayedexpansion

@rem **************************************************************************
title (1/9) ���ϐ��̐ݒ�
@rem **************************************************************************

set project_name=RedmineTimePuncher

@rem Visual Studio 2022
set VisualStudioPath=C:\Program Files\Microsoft Visual Studio\2022\Community\Common7
if not exist "!VisualStudioPath!" (
    @rem Visual Studio 2022���Ȃ���΁A2019�ɕύX����B
    @rem �u"�v�������Ɂ@C:\Program Files (x86)�@�z���̃p�X���w�肵�悤�Ƃ���ƁA
    @rem �u\Microsoft �̎g�������Ԉ���Ă��܂��B�v�̃G���[���������Amake.bat �̎��s�����s���Ă��܂��B
    @rem ����āA�u"�v�����ĕϐ��Ɋi�[����B���̌�̏����܂߁A����ɖ��Ȃ����Ƃ͊m�F�ς݁B
    set VisualStudioPath="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7"
)
if not exist "!VisualStudioPath!" (
	call :echoErr "Visual Studio���C���X�g�[������Ă��܂���B"
	goto ERROR
)
set ConfuserPath=C:\Program Files (x86)\ConfuserEx-CLI
if not exist "%ConfuserPath%" (
	call :echoErr "Confuser���C���X�g�[������Ă��܂���B"
	goto ERROR
)
set InnoSetupPath=C:\Program Files (x86)\Inno Setup 6
if not exist "%InnoSetupPath%" (
	call :echoErr "Inno ���C���X�g�[������Ă��܂���B"
	goto ERROR
)
set FfftpExe=C:\Program Files\ffftp\ffftp.exe
if not exist "%FfftpExe%" (
	call :echoErr "Ffftp ���C���X�g�[������Ă��܂���B"
	goto ERROR
)

pushd "!VisualStudioPath!"
call .\Tools\VsDevCmd.bat
@rem **** MS-Installer���g��Ȃ��̂ŕs�v�ɂȂ����B**** 
@rem call .\IDE\CommonExtensions\Microsoft\VSI\DisableOutOfProcBuild\DisableOutOfProcBuild.exe
@rem if errorlevel 1 (
@rem 	call :echoErr "DisableOutOfProcBuild.exe �̎��s�Ɏ��s���܂����B"
@rem 	goto ERROR
@rem )
popd

set bin_Release=%project_name%\bin\Release
set bin_Packed=%project_name%\bin\Packed
set bin_Confused=%project_name%\bin\Confused

@rem **************************************************************************
title (2/9) Costura.Fody���L���ȏ�ԂŃr���h
@rem **************************************************************************
if exist %bin_Release% (
	rmdir /s /q %bin_Release%
)
call devenv ..\SampleProject.sln /Rebuild "Release" /project %project_name% /projectconfig "Release"
if errorlevel 1 (
	call :echoErr "Costura.Fody�@���L���ȏ�ԂŃ����[�X�r���h �̎��s�Ɏ��s���܂����B"
	goto ERROR
)

@rem **************************************************************************
title (3/9) �L���ȏ�Ԃ̐��ʕ��� bin/Packed �ɑޔ�
@rem **************************************************************************
rmdir /s /q %bin_Packed%
mkdir %bin_Packed%
echo D | xcopy /E /Y %bin_Release% %bin_Packed%

@rem **************************************************************************
title (4/9) Costura.Fody�������ȏ�ԂŃr���h
@rem **************************************************************************
ren %project_name%\FodyWeavers.xml FodyWeavers.xml.org 
echo F | xcopy /Y empty_FodyWeavers.xml %project_name%\FodyWeavers.xml
call devenv ..\SampleProject.sln /Rebuild "Release" /project %project_name% /projectconfig "Release"
if errorlevel 1 (
	call :echoErr "Costura.Fody�@�𖳌��ɂ��ă����[�X�r���h �̎��s�Ɏ��s���܂����B"
	goto ERROR
)
del /q %project_name%\FodyWeavers.xml
ren %project_name%\FodyWeavers.xml.org FodyWeavers.xml 

@rem **************************************************************************
title (5/9) Costura.Fody���L���ȏ�Ԃ̐��ʕ��ŏ㏑��
@rem **************************************************************************
echo D | xcopy /E /Y %bin_Packed% %bin_Release%
if errorlevel 1 (
	call :echoErr "Costura.Fody���L���ȏ�Ԃ̐��ʕ��ŏ㏑���Ŏ��s���܂����B"
	goto ERROR
)

@rem **************************************************************************
title (6/9) ��ǉ�
@rem **************************************************************************
"%ConfuserPath%\Confuser.CLI.exe" -n Confuser.crproj
if errorlevel 1 (
	call :echoErr "��ǉ��Ɏ��s���܂����B"
	goto ERROR
)

@rem **************************************************************************
title (7/9) ��ǉ����ꂽ exe �� bin/Packed �� exe ���㏑�����A bin/Release �Ƀ��l�[��
@rem **************************************************************************
echo D | xcopy /E /Y %bin_Confused% %bin_Packed%
if errorlevel 1 (
	call :echoErr "��ǉ����ꂽ exe �� bin/Packed �� exe ���㏑������̂Ɏ��s���܂����B"
	goto ERROR
)
rmdir /S /Q %bin_Confused%
rmdir /S /Q %bin_Release%
ren %bin_Packed% Release

@rem **************************************************************************
title (8/9) �C���X�g�[���쐬
@rem **************************************************************************
"%InnoSetupPath%\ISCC.exe" %~dp0\Setup\Setup.iss
if errorlevel 1 (
	call :echoErr "�C���X�g�[���쐬�Ɏ��s���܂����B"
	goto ERROR
)

@rem **************************************************************************
title (9/9) Explorer �� FFFTP �̋N��
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
title �R�}���h�v�����v�g

goto EOF

:echoErr
echo %ESC%[41m%~1%ESC%[0m
exit /b

:ERROR
echo error occured.
pause

:EOF
