#ifndef _sets_h_included
#define _sets_h_included
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include <sys/types.h>
#include <time.h>
#include <sys/timeb.h>
#include <math.h>
#ifdef WIN32
#include <windows.h>
#include <conio.h>
#endif
#include "Define.h"

class Sets{
public:
	Sets();
	int loadFile(char *filename);
	int saveFile();
	void freadline();
	bool sisaltaa(char *data, char *teksti, int pituus);
	int arvo(char *data);
	int arvo(char *data, int kohta);
	void nimi(char *nick, char *data);
	int width;
	int height;
	int fps;
	char *filee;
	char nick[4][21];
	bool fileluettu;
	int ai;
	FILE *ini;
//	char rivi[40];
//	char *array;
//	int kohta, size;
	int efects, musics;
	int fullscreen;
	double credits;
	float musicvolume, soundvolume;
	int playermaara;
	bool ihminen[maxplayers];
	bool aikohteet;
	int team[maxplayers];
	int cammode;
	int detail, maxdetail;
	bool mode256;
};

extern int sirpaleita;
extern int alkusirpaleita;
extern bool varjomode;

#endif
