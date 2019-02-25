#include "pch.h"
#include <stdio.h>
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <string>

void print_args(int argc, char *argv[]);
void print_usage();
void apply_cipher(char *pBuf, unsigned int len);
int work(char op, int fileCount, char* argv[]);

const char *binMode = "rb";
const char *trMode = "r";
const char *fileext = ".nfs";

/* build params: /EHsc /MTd /Zi /Fa decrypt.cpp */
int main(int argc, char* argv[])
{
	//print_args(argc, argv);
	if (argc > 1)
	{
		char *pOpt = argv[1];

		if (*pOpt++ == '/')
		{
			switch (*pOpt)
			{
			case 'd':
				//validate file paths, decrypt files
				puts("decryption mode:\n");
				return work('d', argc - 2, argv);
				break;

			case 'e':
				puts("encryption mode:\n");
				work('e', argc - 2, argv);
				break;

			case '?':
			default:
				print_usage();
				break;
			}
		}
		else
		{
			puts("Invalid first argument.\n");
			print_usage();
		}
	}
	else
	{
		puts("You didn't specify enough arguments.\n");
		print_usage();
	}

	return 0;
}

void print_usage()
{
	puts("Usage: /d FILES \t decrypts the specified FILES\n");
	puts("       /e FILES \t encrypts the specified FILES\n");
	puts("       /?       \t prints usage\n");
}

long file_getlength(FILE* pFile)
{
	//TODO: this isn't supposed to work in binary, although it does. change it anyway.
	fseek(pFile, 0, SEEK_END);
	long len = ftell(pFile);
	rewind(pFile);

	return len;
}

void file_createname(char *pSrcPath, char *pNewPath, size_t len)
{

	strcpy_s(pNewPath, len, pSrcPath);
	strcat_s(pNewPath, len, fileext);
}

int work(char op, int fileCount, char* argv[])
{
	printf("\nStart");
	if (fileCount == 0)
	{
		puts("No files specified.\n");
		//system("pause");
		return 1001;
	}

	for (int i = 0; i < fileCount; ++i)
	{
		char *pFilePath = argv[i + 2]; //paths start at argv[2]
		printf("\nWay: %s", pFilePath);
		FILE *pSrcFile = nullptr;
		printf("\nFile created");
		errno_t err;

		printf("\ntrying to read a file");
		try
		{
			if (op == 'd')
				err = fopen_s(&pSrcFile, pFilePath, binMode);
			else
				err = fopen_s(&pSrcFile, pFilePath, trMode);
		}
		catch (int)
		{
			printf("\nunsucc");
			//system("pause");
			return 1001;
		}
		

		if (!err)
		{
			printf("\nsucc");
			long len = file_getlength(pSrcFile);
			printf("\nfile name: %s\nfile length: %ld\n", pFilePath, len);

			if (len > 0)
			{
				//allocate large enough buffer, read in file contents
				char *pBuf = (char *)malloc(len);
				fread(pBuf, sizeof(char), len, pSrcFile); //nfsw treats it as one element of size len for some reason

				apply_cipher(pBuf, len);

				std::string numError = "";

				for (int i = len, j = 0; j < 500; i--, j++)// Server::Callback::operator ()
				{
					if (pBuf[i] == ')' &&
						pBuf[i - 1] == '('	&&
						pBuf[i - 10] == 'o'	&&
						pBuf[i - 20] == 'C'	&&
						pBuf[i - 28] == 'S'
						)
					{
						i -= 29;
						for (; ; i--)//errorCode="5"
						{
							if (pBuf[i] == '\"' &&
								pBuf[i - 1] == '='	&&
								pBuf[i - 2] == 'e'	&&
								pBuf[i - 3] == 'd'	&&
								pBuf[i - 4] == 'o'	&&
								pBuf[i - 5] == 'C'	&&
								pBuf[i - 6] == 'r'	&&
								pBuf[i - 7] == 'o'	&&
								pBuf[i - 8] == 'r'	&&
								pBuf[i - 9] == 'r'	&&
								pBuf[i - 10] == 'e'
								)
							{
								for (i++; ; i++)
								{
									if (pBuf[i] != '\"') numError += pBuf[i];
									else
									{
										printf("\nreturn %s\n", numError.c_str());
										//system("pause");
										return atoi(numError.c_str());
									}
								}
							}
						}

					}
				}
				printf("\nreturn 0\n");
				//system("pause");
				return 0;
			}

		}
		else
		{
			printf("%s could not be opened. Make sure you entered a valid path.\n", *pFilePath);
		}
	}
}

void print_args(int argc, char *argv[])
{
	printf("argc: %d\n");
	int i = 0;
	do
	{
		printf("argv[%d] = %s\n", i, argv[i]);
		++i;
	} while (i < argc);
}

void apply_cipher(char* pBuf, unsigned int len)
{
	unsigned int eax, ecx = 0;
	unsigned int edi = 0x00519753;
	for (int i = 0; i < len; ++i)
	{
		eax = edi;
		eax ^= 0x1D872B41;
		ecx = eax >> 0x5;
		ecx ^= eax;
		edi = ecx << 0x1B;
		edi ^= ecx;
		ecx = 0x00B0ED68;
		edi ^= eax;
		eax = edi >> 0x17;
		*pBuf ^= (char)eax;
		++pBuf;
	}
}