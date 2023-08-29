@rem  
pypy3 cffitest.py
clang -o test.exe test.c Release/pypytest.lib

xcopy /y utils Pint-34287\utils\
xcopy /y utils\ccl_chrome_indexeddb Pint-34287\utils\ccl_chrome_indexeddb\

cd Pint-34287\
del /F /S /Q .gitignore
cd ..

copy /y test.exe Pint-34287
copy /y pypytest.dll Pint-34287

pause