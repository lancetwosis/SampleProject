import time
from pypytest import ffi, lib
import sys
sys.path.append('./utils/')
import main

@ffi.def_extern()
def pypyrun(str1, str2):
    try:
        src = ffi.string(str1, 255).decode()
        print('src =', src)
        dst = ffi.string(str2, 255).decode()
        print('dst =', dst)

        time_sta = time.time()

        main.process_db(src, dst);

        time_end = time.time()
        tim = time_end- time_sta
        print('time =', tim)

        return 1;
    except Exception as e:
        print(e, file=sys.stderr)
        return -1;
        
