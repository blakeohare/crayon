// GameInternals.cpp : Defines the entry point for the application.
//

#include "GameInternals.h"
#include <stdlib.h>

using namespace std;

int* create_buffer(int size)
{
	int* buffer = (int*)malloc(sizeof(int) * (size + 1));
	buffer[0] = size;
	return buffer + 1;
}

int* blarg_test(int argc, int* argv)
{
	int* buffer = create_buffer(argc);
	for (int i = 0; i < argc; ++i) {
		buffer[i] = argv[i] * 2;
	}
	return buffer;
}
