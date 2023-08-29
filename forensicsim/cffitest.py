import cffi
ffibuilder = cffi.FFI()

#pypyrun()を読み込むためのCスクリプト「test.h」を読み込む
with open('test.h') as f:
    ffibuilder.embedding_api(f.read())

#pythonスクリプトを読み込む
with open('test.py') as f2:
    ffibuilder.embedding_init_code(f2.read())

#pypytest.dll(dylib)にプラグイン(埋め込みを)する
ffibuilder.set_source("pypytest", "")

ffibuilder.compile(verbose=True)
