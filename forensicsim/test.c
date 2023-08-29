#include <stdio.h>
#include "test.h"

int main(int argc, char *argv[]) {
    int rc;

    rc = pypyrun(argv[1], argv[2]);
    if(rc == 1)
    {
        printf("successfully completed");
    }
    else
    {
        fprintf(stderr, "Error was occured. (%d)", rc);
    }
    return rc;
}